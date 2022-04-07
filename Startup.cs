
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using SharePointFileToBlob.Interfaces;
using SharePointFileToBlob.Models;
using SharePointFileToBlob.Services;
using System;
using System.Reflection;

[assembly: FunctionsStartup(typeof(SharePointFileToBlob.Startup))]


namespace SharePointFileToBlob
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<BlobConfiguration>()
               .Configure<IConfiguration>((settings, configuration) =>
               {
                   configuration.GetSection("BlobConfiguration").Bind(settings);
               });

            builder.Services.AddSingleton<IBlobService, BlobService>();

            builder.Services.AddSingleton<IGraphApiClient, GraphApiClient>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var builtConfig = builder.ConfigurationBuilder.Build();
            var keyVaultEndpoint = builtConfig["AzureKeyVaultEndpoint"];

            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                // using Key Vault, either local dev or deployed
                builder.ConfigurationBuilder
                        .SetBasePath(Environment.CurrentDirectory)
                        .AddAzureKeyVault(new AzureKeyVaultConfigurationOptions
                        {
                            Vault = keyVaultEndpoint,
                            ReloadInterval = TimeSpan.FromMinutes(10),
                            Client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                            new AzureServiceTokenProvider().KeyVaultTokenCallback))
                        })
                        .AddJsonFile("local.settings.json", true)
                        .AddEnvironmentVariables()
                    .Build();
            }
            else
            {
                // local dev no Key Vault
                builder.ConfigurationBuilder
                   .SetBasePath(Environment.CurrentDirectory)
                   .AddJsonFile("local.settings.json", true)
                   .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                   .AddEnvironmentVariables()
                   .Build();
            }

        }

    }
}
