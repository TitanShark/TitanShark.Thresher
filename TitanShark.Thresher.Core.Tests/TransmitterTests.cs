using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace TitanShark.Thresher.Core.Tests
{
    public class TransmitterTests
    {
        private readonly ITestOutputHelper _output;

        public TransmitterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Custom_SendFunction_In_Transmitter_Is_Possible()
        {
            // prepares
            const string responseContentText = "Hello World!";
            var transmitter = new Transmitter
                (
                    (callId, request, cancellationToken) =>
                    {
                        _output.WriteLine($"Request to '{request.RequestUri}' was sent out.");

                        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Created) 
                        { 
                            Content = new StringContent(responseContentText) 
                        });
                    }
                );
            var handler = new InterceptableHttpClientHandler(transmitter: transmitter);
            var sut = new HttpClient(handler);

            // acts
            var response = await sut.GetAsync("https://testing.only").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // asserts
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            content.Should().Be(responseContentText);
        }
    }
}
