using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace TitanShark.Thresher.Core.Tests
{
    public class ReplayerTests
    {
        private readonly ITestOutputHelper _output;

        public ReplayerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Record_And_Sequential_Replay_With_Filter_On_StatusCodes()
        {
            // prepares
            var stats = new Dictionary<HttpStatusCode, int>
            {
                [HttpStatusCode.OK] = 0,
                [HttpStatusCode.BadRequest] = 0,
                [HttpStatusCode.NotFound] = 0
            };

            var transmitter = new Transmitter
                (
                    (callId, request, cancellationToken) =>
                    {
                        _output.WriteLine($"Request to '{request.RequestUri}' was sent out.");

                        var response = Mock.BuildResponse(request);

                        stats[response.StatusCode] += 1; 

                        return Task.FromResult(response);
                    }
                );
            var persistence = new InMemoryRecordsPersistence();
            var recorder = new Recorder(persistence);
            var handler = new InterceptableHttpClientHandler(
                transmitter: transmitter, 
                interceptorsRunner: new SequentialInterceptorsRunner(recorder));
            var client = new HttpClient(handler);

            var started = DateTime.UtcNow;
            await client.GetAsync("https://testing.only/Successful").ConfigureAwait(false);
            await client.GetAsync("https://testing.only/Failed").ConfigureAwait(false);
            await client.GetAsync("https://testing.only/NotFound").ConfigureAwait(false);
            var ended = DateTime.UtcNow;
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            await client.GetAsync("https://testing.only/Failed").ConfigureAwait(false);

            stats[HttpStatusCode.OK].Should().Be(1);
            stats[HttpStatusCode.BadRequest].Should().Be(2);
            stats[HttpStatusCode.NotFound].Should().Be(1);

            stats[HttpStatusCode.OK] = 0;
            stats[HttpStatusCode.BadRequest] = 0;
            stats[HttpStatusCode.NotFound] = 0;

            // acts
            var snapshot = await persistence.Snapshot(
                CancellationToken.None, 
                started, ended, 
                new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound });

            var replayer = new Replayer(new SequentialReplayingStrategy(), client, snapshot);
            replayer.Start();
            
            // as the Replayer is in run, we should wait a litte bit,
            // to make sure that all Records were re-played.
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            replayer.Stop();

            // asserts
            stats[HttpStatusCode.OK].Should().Be(0);
            stats[HttpStatusCode.BadRequest].Should().Be(1);
            stats[HttpStatusCode.NotFound].Should().Be(1);
        }

        private class Mock
        {
            public const string Ok = "Okay!";
            public const string BadRequest = "Bad request!";
            public const string NotFound = "Not found!";

            public static HttpResponseMessage BuildResponse(HttpRequestMessage request) => request.RequestUri.PathAndQuery switch
            {
                "/Successful" => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(Ok)
                },

                "/Failed" => new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(BadRequest)
                },

                _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
                {
                    Content = new StringContent(NotFound)
                }
            };
        }
    }
}
