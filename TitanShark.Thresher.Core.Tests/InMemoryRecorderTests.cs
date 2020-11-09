using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Linq;

namespace TitanShark.Thresher.Core.Tests
{
    public class InMemoryRecorderTests
    {
        private readonly ITestOutputHelper _output;

        public InMemoryRecorderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InMemoryRecorder_Works_WithSingleCall()
        {
            // prepares
            var persistence = new InMemoryRecordsPersistence();
            var recorder = new Recorder(persistence);
            var handler = new SerialInterceptionHttpClientHandler(recorder);
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
        public async Task InMemoryRecorder_Works_WithMultipleCalls()
        {
            // prepares
            var persistence = new InMemoryRecordsPersistence();
            var recorder = new Recorder(persistence);
            var handler = new SerialInterceptionHttpClientHandler(recorder);
            var sut = new HttpClient(handler);

            const int numberOfCalls = 100;

            // acts
            var tasks = Enumerable.Range(1, numberOfCalls).Select(number => sut.GetAsync("https://google.com"));
            var responses = await Task.WhenAll(tasks);

            // asserts
            var contents = await Task.WhenAll(responses.Select(response => response.Content.ReadAsStringAsync()));
            contents.ToList().ForEach(content => content.Length.Should().BeGreaterThan(0));

            persistence.ReadOnlyRecords.Count.Should().Be(numberOfCalls);

            // cleans up
            sut.Dispose();
        }
    }
}
