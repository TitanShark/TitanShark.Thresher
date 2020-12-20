using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using TitanShark.Thresher.Core;
using Xunit;
using Xunit.Abstractions;

namespace TitanShark.Thresher.Realm.Tests
{
    public class RealmRecordsPersistenceTests
    {
        private const string AcceptJson = "application/json";
        private const string BasicAuthScheme = "Basic";
        private const string BasicAuthValue = "cm9vdDpyb290";

        private readonly ITestOutputHelper _output;

        public RealmRecordsPersistenceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Record_Replay_With_Default_Settings()
        {
            // prepares
            var stats = new Dictionary<HttpStatusCode, int>
            {
                [HttpStatusCode.OK] = 0,
                [HttpStatusCode.NotAcceptable] = 0,
                [HttpStatusCode.Unauthorized] = 0,
                [HttpStatusCode.BadRequest] = 0,
                [HttpStatusCode.NotFound] = 0
            };

            var transmitter = new Transmitter
                (
                    (callId, request, cancellationToken) =>
                    {
                        _output.WriteLine($"Request to '{request.RequestUri}' was sent out.");

                        var response = Mock.Build(request);

                        stats[response.StatusCode] += 1;

                        return Task.FromResult(response);
                    }
                );

            // acts
            // ... recording
            RealmRecordsPersistence persistence = CreateRealmPersistence();

            var recorder = new Recorder(persistence);
            var handler = new InterceptableHttpClientHandler(
                transmitter: transmitter,
                interceptorsRunner: new SequentialInterceptorsRunner(recorder));

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AcceptJson));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BasicAuthScheme, BasicAuthValue);

            var started = DateTime.UtcNow;
            await client.GetAsync("https://testing.only/Successful").ConfigureAwait(false);
            await client.GetAsync("https://testing.only/Failed").ConfigureAwait(false);
            await client.GetAsync("https://testing.only/NotFound").ConfigureAwait(false);
            var ended = DateTime.UtcNow;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await client.GetAsync("https://testing.only/Failed").ConfigureAwait(false);

            // asserts
            stats[HttpStatusCode.OK].Should().Be(1);
            stats[HttpStatusCode.NotAcceptable].Should().Be(0);
            stats[HttpStatusCode.Unauthorized].Should().Be(0);
            stats[HttpStatusCode.BadRequest].Should().Be(2);
            stats[HttpStatusCode.NotFound].Should().Be(1);

            // cleans up
            stats[HttpStatusCode.OK] = 0;
            stats[HttpStatusCode.NotAcceptable] = 0;
            stats[HttpStatusCode.Unauthorized] = 0;
            stats[HttpStatusCode.BadRequest] = 0;
            stats[HttpStatusCode.NotFound] = 0;
            client.Dispose();

            // acts
            // ... snapshots
            // ... gets only logic-failed requests
            var snapshot = await persistence.Snapshot(
                started, ended,
                new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound });

            // ... re-creates a fresh instance of HttpClient, without recorder.
            handler = new InterceptableHttpClientHandler(transmitter: transmitter);
            client = new HttpClient(handler);

            // ... replays
            var replayer = new Replayer(new SequentialReplayingStrategy(), client, snapshot);
            await replayer.Start();
            replayer.Stop();

            // cleans up
            client.Dispose();

            // asserts
            stats[HttpStatusCode.OK].Should().Be(0);
            stats[HttpStatusCode.BadRequest].Should().Be(1);
            stats[HttpStatusCode.NotFound].Should().Be(1);
        }

        [Fact]
        public async Task Record_Replay_Long_Running_In_Parallel()
        {
            // prepares
            const int total = 1000;
            var successCounter = 0;

            var transmitter = new Transmitter
                (
                    (callId, request, cancellationToken) =>
                    {
                        _output.WriteLine($"Request to '{request.RequestUri}' was sent out.");

                        var response = Mock.Build(request);

                        if (response.IsSuccessStatusCode)
                        {
                            successCounter++;
                        }

                        return Task.FromResult(response);
                    }
                );

            // acts
            // ... recording
            RealmRecordsPersistence persistence = CreateRealmPersistence();

            var recorder = new Recorder(persistence);
            var handler = new InterceptableHttpClientHandler(
                transmitter: transmitter,
                interceptorsRunner: new SequentialInterceptorsRunner(recorder));

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AcceptJson));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BasicAuthScheme, BasicAuthValue);

            var started = DateTime.UtcNow;
            await Task.WhenAll(Enumerable
                                .Range(1, total)
                                .Select(num => Task.Run(async () =>
                                {
                                    await client.GetAsync("https://testing.only/Successful").ConfigureAwait(false);
                                })));
            var ended = DateTime.UtcNow;

            // asserts
            successCounter.Should().Be(total);

            // cleans up
            successCounter = 0;
            client.Dispose();

            // acts
            // ... snapshots
            var snapshot = await persistence.Snapshot(started, ended);
            snapshot.TotalRecords.Should().Be(total);

            // ... re-creates a fresh instance of HttpClient, without recorder.
            handler = new InterceptableHttpClientHandler(transmitter: transmitter);
            client = new HttpClient(handler);

            // ... replays
            var replayer = new Replayer(
                new SequentialReplayingStrategy
                {
                    BatchSize = total / 100 // changes default Batch Size 
                },
                client,
                snapshot);

            await replayer.Start();
            replayer.Stop();

            // asserts
            successCounter.Should().Be(total);

            // cleans up
            client.Dispose();
        }

        private static RealmRecordsPersistence CreateRealmPersistence()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var persistence = new RealmRecordsPersistence(Path.Combine(Path.GetDirectoryName(location), "records.realm"));
            return persistence;
        }

        private class Mock
        {
            public const string Ok = "Okay!";
            public const string Unauthorized = "No auth!";
            public const string NotAcceptable = "No auth!";
            public const string BadRequest = "Bad request!";
            public const string NotFound = "Not found!";

            public static HttpResponseMessage Build(HttpRequestMessage request)
            {
                if (!request.Headers.Accept.Any(accept => string.Equals(accept.MediaType, AcceptJson, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                    {
                        Content = new StringContent(NotAcceptable)
                    };
                }

                if (!string.Equals(request.Headers.Authorization.ToString(), $"{BasicAuthScheme} {BasicAuthValue}", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent(Unauthorized)
                    };
                }

                return BuildInternal(request);
            }

            private static HttpResponseMessage BuildInternal(HttpRequestMessage request) => request.RequestUri.PathAndQuery switch
            {
                "/Successful" => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(Ok)
                },

                "/Failed" => new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(BadRequest)
                },

                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(NotFound)
                }
            };
        }
    }
}
