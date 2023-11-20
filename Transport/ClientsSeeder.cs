using OpenIddict.Abstractions;
using System.Globalization;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Transport
{
    public class ClientsSeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientsSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddScopes()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var apiScope = await manager.FindByNameAsync("TrackingPanel");

            if (apiScope != null)
            {
                await manager.DeleteAsync(apiScope);
            }

            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                DisplayName = "TrackingPanel scope",
                Name = "TrackingPanel",
                Resources =
                {
                    "CustomerTrackingService"
                }
            });
        }

        public async Task AddClients()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var client = await manager.FindByClientIdAsync("tracking-client");
            if (client != null)
            {
                await manager.DeleteAsync(client);
            }

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "tracking-client",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Tracking client application",
                RedirectUris =
                {
                    new Uri("https://localhost:7002/swagger/oauth2-redirect.html")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:7002/resources")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                   $"{Permissions.Prefixes.Scope}TrackingPanel"
                },
                //Requirements =
                //{
                //    Requirements.Features.ProofKeyForCodeExchange
                //}
            });
        }
    }
}
