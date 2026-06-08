using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WireMock.Server;

namespace PaymentGateway.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public WireMockServer WireMockServer { get; private set; }
        public HttpClient Client { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("AcquiringBank:BaseAddress", $"{WireMockServer.Urls[0]}");
        }

        public async Task InitializeAsync()
        {
            WireMockServer = WireMockServer.Start();
            Client = CreateClient();
        }

        public new async Task DisposeAsync()
        {
            Client.Dispose();
            WireMockServer.Stop();
            await base.DisposeAsync();
        }
    }
}
