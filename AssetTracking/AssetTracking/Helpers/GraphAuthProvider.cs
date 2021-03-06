﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using AssetTracking.Extensions;

namespace AssetTracking.Helpers
{
    public class GraphAuthProvider : IGraphAuthProvider
    {
        private IConfidentialClientApplication _app;
        private readonly string[] _scopes;
        public GraphAuthProvider(IConfiguration configuration)
        {
            var azureOptions = new AzureOptions();
            configuration.Bind("AzureAd", azureOptions);

            _app = ConfidentialClientApplicationBuilder.Create(azureOptions.ClientId)

                    .WithClientSecret(azureOptions.ClientSecret)

                    .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)

                    .WithRedirectUri(azureOptions.BaseUrl + azureOptions.CallbackPath)

                    .Build();
            Authority = _app.Authority;
            _scopes = azureOptions.GraphScopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public string Authority { get; }
        public async Task<string> GetUserAccessTokenAsync(string userId)
        {
            var account = await _app.GetAccountAsync(userId);

            if (account == null) throw new ServiceException(new Error
            {
                Code = "TokenNotFound",
                Message = "User not found in token cache. Maybe the server was restarted."
            });

            try
            {
                var result = await _app.AcquireTokenSilent(_scopes, account).ExecuteAsync();

                return result.AccessToken;
            }            
            catch (Exception)
            {
                throw new ServiceException(new Error
                {
                    Code = GraphErrorCode.AuthenticationFailure.ToString(),
                    Message = "Caller needs to authenticate. Unable to retrieve the access token silently."

                });
            }
        }
        public async Task<AuthenticationResult> GetUserAccessTokenByAuthorizationCode(string authorizationCode)
        {
            return await _app.AcquireTokenByAuthorizationCode(_scopes, authorizationCode).ExecuteAsync();
        }
    }

    public interface IGraphAuthProvider
    {
        string Authority { get; }
        Task<string> GetUserAccessTokenAsync(string userId);
        Task<AuthenticationResult> GetUserAccessTokenByAuthorizationCode(string authorizationCode);
    }
}