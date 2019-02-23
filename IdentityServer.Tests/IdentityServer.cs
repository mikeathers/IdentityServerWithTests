using System.Net;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Startup = IdentityServer.Api.Startup;

namespace IdentityServer.Tests
{
    public class IdentityServer
    {
        private readonly HttpClient _httpClient;
        private string _tokenEndPoint;

        public IdentityServer()
        {
            var webhost = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseUrls("http://localhost:8000")
                .UseStartup<Startup>();

            var server = new TestServer(webhost);
            _httpClient = server.CreateClient();

            var client = new HttpClient();
            var disco = client.GetDiscoveryDocumentAsync("http://localhost:5000").Result;
            _tokenEndPoint = disco.TokenEndpoint;
        }

        [Fact]
        public async void ShouldNotAllowAnonymousUser()
        {
            var result = await _httpClient.GetAsync("http://localhost:8000/api/values");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async void ShouldReturnValuesForAuthenticatedUserUsingPasswordFlow()
        {
            var _tokenClient = new HttpClient();

            var response = await _tokenClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = _tokenEndPoint,

                Scope = "api1",
                ClientId = "ro.client",
                ClientSecret = "secret",

                UserName = "alice",
                Password = "password",

            });

             _httpClient.SetBearerToken(response.AccessToken);

            var result = await _httpClient.GetStringAsync("http://localhost:8000/api/values");
            Assert.Equal("[\"value1\",\"value2\"]", result);
        }

        
    }
}
