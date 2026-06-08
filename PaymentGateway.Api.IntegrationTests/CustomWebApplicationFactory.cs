using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using WireMock.Server;

namespace PaymentGateway.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public WireMockServer WireMockServer { get; private set; }

        public HttpClient Client { get; init; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("AcquiringBank:BaseAddress", $"{WireMockServer.Urls[0]}");
        }

        public HttpClient CreateClient()
        { 
        }

        public async Task InitializeAsync()
        {
            WireMockServer = WireMockServer.Start();
        }

        public new async Task DisposeAsync()
        {
            WireMockServer.Dispose();
            await base.DisposeAsync();
        }
    }
}
