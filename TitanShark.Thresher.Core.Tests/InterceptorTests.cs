using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Threading;

namespace TitanShark.Thresher.Core.Tests
{
    public class InterceptorTests
    {
        private readonly ITestOutputHelper _output;

        public InterceptorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Single_Interceptor_Sequential_Runner()
        {
            // prepares
            var persistence = new InMemoryRecordsPersistence();
            var recorder = new Recorder(persistence);
            var handler = new InterceptableHttpClientHandler(interceptorsRunner: new SequentialInterceptorsRunner(recorder));
            var sut = new HttpClient(handler);

            // acts
            var response = await sut.GetAsync("https://google.com").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // asserts
            content.Length.Should().BeGreaterThan(0);
            _output.WriteLine(content);

            persistence.ReadOnlyRecords.Count.Should().Be(1);
            var keypair = persistence.ReadOnlyRecords.First();
            _output.WriteLine(keypair.Key.ToString());
            _output.WriteLine(keypair.Value);

            // cleans up
            sut.Dispose();
        }

        [Fact]
        public Task Multiple_Interceptors_Sequential_Runner_DefaultJsonRecordSerializer()
        {
            return RunMultipleInterceptors(interceptors => new SequentialInterceptorsRunner(interceptors), new SystemJsonRecordSerializer());
        }

        [Fact]
        public Task Multiple_Interceptors_Parallel_Runner_DefaultJsonRecordSerializer()
        {
            return RunMultipleInterceptors(interceptors => new ParallelInterceptorsRunner(interceptors), new SystemJsonRecordSerializer());
        }

        [Fact]
        public Task Multiple_Interceptors_Sequential_Runner_NewtonsoftJsonRecordSerializer()
        {
            return RunMultipleInterceptors(interceptors => new SequentialInterceptorsRunner(interceptors), new NewtonsoftJsonRecordSerializer());
        }

        [Fact]
        public Task Multiple_Interceptors_Parallel_Runner_NewtonsoftJsonRecordSerializer()
        {
            return RunMultipleInterceptors(interceptors => new ParallelInterceptorsRunner(interceptors), new NewtonsoftJsonRecordSerializer());
        }

        private static async Task RunMultipleInterceptors(Func<IInterceptor[], InterceptorsRunner> runnerCreator, IRecordSerializer<string> serializer)
        {
            // prepares
            var persistenceOne = new InMemoryRecordsPersistence(serializer);
            var recorderOne = new Recorder(persistenceOne);

            var persistenceTwo = new InMemoryRecordsPersistence(serializer);
            var recorderTwo = new Recorder(persistenceTwo);

            var handler = new InterceptableHttpClientHandler(interceptorsRunner: runnerCreator(new[] { recorderOne, recorderTwo }));
            var sut = new HttpClient(handler);

            const int numberOfCalls = 10;

            // acts
            var tasks = Enumerable.Range(1, numberOfCalls).Select(number => sut.GetAsync("https://google.com"));
            var responses = await Task.WhenAll(tasks);

            // asserts
            var contents = await Task.WhenAll(responses.Select(response => response.Content.ReadAsStringAsync()));
            contents.ToList().ForEach(content => content.Length.Should().BeGreaterThan(0));

            persistenceOne.ReadOnlyRecords.Count.Should().Be(numberOfCalls);
            persistenceTwo.ReadOnlyRecords.Count.Should().Be(numberOfCalls);

            // cleans up
            sut.Dispose();
        }

        private class NewtonsoftJsonRecordSerializer : IRecordSerializer<string>
        {
            public Task<string> Serialize(Record record, CancellationToken cancellationToken)
            {
                var json = JsonConvert.SerializeObject(record);

                return Task.FromResult(json);
            }

            public Task<Record> Deserialize(string json, CancellationToken cancellationToken)
            {
                var record = JsonConvert.DeserializeObject<Record>(json);

                return Task.FromResult(record);
            }
        }
    }
}
