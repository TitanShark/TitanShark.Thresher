using FluentAssertions;
using System.Net.Http;
using System.Net.Http.Json;
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
        public async Task Custom_SendFunction_Is_Possible()
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

            // cleans up
            sut.Dispose();
        }

        [Fact]
        public async Task Mock_Is_Possible()
        {
            // prepares
            var transmitter = new Transmitter
                (
                    (callId, request, cancellationToken) =>
                    {
                        _output.WriteLine($"Request to '{request.RequestUri}' was sent out.");

                        var response = Mock.BuildResponse(request);

                        return Task.FromResult(response);
                    }
                );
            var handler = new InterceptableHttpClientHandler(transmitter: transmitter);
            var sut = new HttpClient(handler);

            // acts
            var correctResponse = await sut.GetAsync("https://testing.only/ResourceA/1").ConfigureAwait(false);
            var correctContent = await correctResponse.Content.ReadFromJsonAsync<Mock.ResInfo>().ConfigureAwait(false);

            var incorrectResponse = await sut.GetAsync("https://testing.only/TypeA/1").ConfigureAwait(false);
            var incorrectContent = await incorrectResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            // asserts
            correctResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            correctContent.Resource.Should().Be("ResourceA");
            correctContent.Id.Should().Be(1);

            incorrectResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            incorrectContent.Should().Be(Mock.NotFound);

            // cleans up
            sut.Dispose();
        }

        private class Mock
        {
            public const string NotFound = "Not found!";

            public static HttpResponseMessage BuildResponse(HttpRequestMessage request) => request.RequestUri.PathAndQuery switch
            {
                "/ResourceA/1" => new HttpResponseMessage(System.Net.HttpStatusCode.Created)
                {
                    Content = JsonContent.Create(new ResInfo { Resource = "ResourceA", Id = 1 })
                },

                _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
                {
                    Content = new StringContent(NotFound)
                }
            };

            public class ResInfo
            {
                public string Resource { get; set; }

                public int Id { get; set; }
            }
        }
    }
}
