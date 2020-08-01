﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client.ApiConfig.Parameters;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Instance;
using Microsoft.Identity.Client.Internal.Broker;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Client.OAuth2;
using Microsoft.Identity.Client.UI;
using Microsoft.Identity.Client.Utils;
using Windows.Foundation.Metadata;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace Microsoft.Identity.Client.Platforms.netdesktop.Broker
{
    //TODO: bogavril - C++ impl catches all exceptions and emits telemetry - consider the same?
    internal class WamBroker : IBroker
    {
        private readonly IWamPlugin _aadPlugin;
        private readonly IWamPlugin _msaPlugin;


        private readonly CoreUIParent _uiParent;
        private readonly ICoreLogger _logger;


        public WamBroker(CoreUIParent uiParent, ICoreLogger logger)
        {

            _uiParent = uiParent;
            _logger = logger;

            _aadPlugin = new AadPlugin(_logger, _uiParent);
            _msaPlugin = new MsaPlugin(_logger, _uiParent);

        }

        public Task<MsalTokenResponse> AcquireTokenInteractiveAsync(AuthenticationRequestParameters authenticationRequestParameters, AcquireTokenInteractiveParameters acquireTokenInteractiveParameters)
        {
            throw new NotImplementedException();
        }

        // TODO: bogavril - in C++ impl, ROPC is also included here. Will ommit for now.
        public async Task<MsalTokenResponse> AcquireTokenSilentAsync(
            AuthenticationRequestParameters authenticationRequestParameters,
            AcquireTokenSilentParameters acquireTokenSilentParameters)
        {
            using (_logger.LogMethodDuration())
            {
                // Important: MSAL will have already resolved the authority by now, 
                // so we are not expecting "common" or "organizations" but a tenanted authority
                bool isMsa = IsMsaSilentRequest(authenticationRequestParameters.Authority);

                IWamPlugin wamPlugin = isMsa ? _msaPlugin : _aadPlugin;
                string tid = isMsa ? "consumers" : authenticationRequestParameters.Authority.TenantId;
                WebAccountProvider provider = await GetAccountProviderAsync(tid)
                    .ConfigureAwait(false);

                // TODO: store WAM client IDs to support 3rd parties
                WebAccount webAccount = await FindWamAccountForMsalAccountAsync(
                    provider,
                    wamPlugin,
                    authenticationRequestParameters.Account,
                    authenticationRequestParameters.LoginHint,
                    authenticationRequestParameters.ClientId).ConfigureAwait(false);

                if (webAccount == null)
                {
                    return new MsalTokenResponse()
                    {
                        Error = MsalError.InteractionRequired, // this will get translated to MSALUiRequiredEx
                        ErrorDescription = "Could not find a WAM account for the silent requst"
                    };
                }

                WebTokenRequest webTokenRequest = await wamPlugin.CreateWebTokenRequestAsync(
                    provider,
                    false /* is interactive */,
                    webAccount != null, /* is account in WAM */
                    authenticationRequestParameters)
                    .ConfigureAwait(false);

                AddExtraParamsToRequest(webTokenRequest, authenticationRequestParameters.ExtraQueryParameters);
                // TODO bogavril: add POP support by adding "token_type" = "pop" and "req_cnf" = req_cnf

                WebTokenRequestResult wamResult;
                using (_logger.LogBlockDuration("WAM:GetTokenSilentlyAsync:"))
                {
                    wamResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest, webAccount);

                    // TODO bogavril - WAM allows to sign in with "default" account. MSAL has no such concept.                    
                    // wamResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest);

                }

                return CreateMsalTokenResponse(wamResult, wamPlugin, isInteractive: false);
            }
        }

        private async Task<WebAccount> FindWamAccountForMsalAccountAsync(
           WebAccountProvider provider,
           IWamPlugin wamPlugin,
           IAccount account,
           string loginHint,
           string clientId)
        {
            WamProxy wamProxy = new WamProxy(provider, _logger);

            var webAccounts = await wamProxy.FindAllWebAccountsAsync(clientId).ConfigureAwait(false);

            WebAccount matchedAccountByLoginHint = null;
            foreach (var webAccount in webAccounts)
            {
                string homeAccountId = wamPlugin.GetHomeAccountIdOrNull(webAccount);
                if (string.Equals(homeAccountId, account.HomeAccountId.Identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return webAccount;
                }

                if (string.Equals(loginHint, webAccount.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    matchedAccountByLoginHint = webAccount;
                }
            }

            return matchedAccountByLoginHint;
        }

        private const string WamErrorPrefix = "WAM Error";

        private MsalTokenResponse CreateMsalTokenResponse(
            WebTokenRequestResult wamResponse,
            IWamPlugin wamPlugin,
            bool isInteractive)
        {
            string internalErrorCode = null;
            string errorMessage;
            string errorCode;

            switch (wamResponse.ResponseStatus)
            {
                case WebTokenRequestStatus.Success:
                    return wamPlugin.ParseSuccesfullWamResponse(wamResponse.ResponseData[0]);
                case WebTokenRequestStatus.UserInteractionRequired:
                    errorCode =
                        wamPlugin.MapTokenRequestError(wamResponse.ResponseStatus, wamResponse.ResponseError.ErrorCode, isInteractive);
                    internalErrorCode = wamResponse.ResponseError.ErrorCode.ToString(CultureInfo.InvariantCulture);
                    errorMessage = WamErrorPrefix + wamPlugin.GetType() + wamResponse.ResponseError.ErrorMessage;
                    break;
                case WebTokenRequestStatus.UserCancel:
                    errorCode = MsalError.AuthenticationCanceledError;
                    errorMessage = MsalErrorMessage.AuthenticationCanceled;
                    break;
                case WebTokenRequestStatus.ProviderError:
                    errorCode =
                        wamPlugin.MapTokenRequestError(wamResponse.ResponseStatus, wamResponse.ResponseError.ErrorCode, isInteractive);
                    errorMessage = WamErrorPrefix + wamPlugin.GetType() + wamResponse.ResponseError.ErrorMessage;
                    internalErrorCode = wamResponse.ResponseError.ErrorCode.ToString(CultureInfo.InvariantCulture);
                    break;
                case WebTokenRequestStatus.AccountSwitch: // TODO: bogavril - what does this mean?
                    errorCode = "account_switch";
                    errorMessage = "WAM returned AccountSwitch";
                    break;

                default:
                    errorCode = MsalError.UnknownBrokerError;
                    internalErrorCode = wamResponse.ResponseError.ErrorCode.ToString(CultureInfo.InvariantCulture);
                    errorMessage = $"Unknown WebTokenRequestStatus {wamResponse.ResponseStatus} (internal error code {internalErrorCode})";
                    break;
            }

            return new MsalTokenResponse()
            {
                Error = errorCode,
                ErrorCodes = internalErrorCode != null ? new[] { internalErrorCode } : null,
                ErrorDescription = errorMessage
            };
        }



        private void AddExtraParamsToRequest(WebTokenRequest webTokenRequest, IDictionary<string, string> extraQueryParameters)
        {
            if (extraQueryParameters != null)
            {
                // MSAL uses instance_aware=true, but WAM calls it discover=home, so we rename the parameter before passing
                // it to WAM.
                foreach (var kvp in extraQueryParameters)
                {
                    string key = kvp.Key;
                    string value = kvp.Value;

                    if (string.Equals("instance_aware", key) && string.Equals("true", value))
                    {
                        key = "discover";
                        value = "home";
                    }

                    webTokenRequest.AppProperties.Add(key, value);
                }
            }
        }

        // TODO - experiment - can I call both AAD and MSA providers for a token?
        private bool IsMsaSilentRequest(Authority authority)
        {
            if (authority.AuthorityInfo.AuthorityType != AuthorityType.Aad)
            {
                // TODO: confirm this and move exception higher up
                throw new MsalClientException("Broker is only supported for AAD authorities, not ADFS or B2C");
            }

            if ((authority as AadAuthority).IsCommonOrganizationsOrConsumersTenant())
            {
                _logger.Error("[WAM Broker] Cannot decide which plugin (AAD or MSA) to use. Using AAD. Tenant should have been resolved by now.");
                return false;
            }

            string tenantId = authority.TenantId;
            if (string.Equals("consumers", tenantId, StringComparison.OrdinalIgnoreCase) ||
                // Microsoft accounts are authenticated in AAD tenant 9188040d-6c67-4c5b-b112-36a304b66dad.
                string.Equals("9188040d-6c67-4c5b-b112-36a304b66dad", tenantId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Info("[WAM Broker] Tenant is consumers. ATS will try WAM-MSA ");
                return true;
            }

            _logger.Info("[WAM Broker] Tenant is not consumers and ATS will try WAM-AAD");
            return false;
        }

        public async Task<IEnumerable<IAccount>> GetAccountsAsync(string clientID, string redirectUri)
        {
            using (_logger.LogMethodDuration())
            {
                if (!ApiInformation.IsMethodPresent(
                    "Windows.Security.Authentication.Web.Core.WebAuthenticationCoreManager",
                    "FindAllAccountsAsync"))
                {
                    _logger.Info("WAM::FindAllAccountsAsync method does not exist. Returning 0 broker accounts. ");
                    return Enumerable.Empty<IAccount>();
                }

                var aadAccounts = await _aadPlugin.GetAccountsAsync(clientID).ConfigureAwait(false);
                var msaAccounts = await _msaPlugin.GetAccountsAsync(clientID).ConfigureAwait(false);

                return aadAccounts.Concat(msaAccounts);

                // TODO: these accounts need to be cached
            }
        }

        public void HandleInstallUrl(string appLink)
        {
            throw new NotImplementedException();
        }

        public bool IsBrokerInstalledAndInvokable()
        {
            return true;
        }

        public Task RemoveAccountAsync(string clientID, IAccount account)
        {
            _logger.Verbose("WAM accounts are not removable.");
            return Task.CompletedTask;
        }

        #region Helpers
        private static async Task<WebAccountProvider> GetDefaultAccountProviderAsync()
        {
            return await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.windows.local");
        }

        public static async Task<bool> IsDefaultAccountMsaAsync()
        {
            var provider = await GetDefaultAccountProviderAsync().ConfigureAwait(false);
            return provider != null && string.Equals("consumers", provider.Authority);
        }

        public static string GetEffectiveScopes(SortedSet<string> scopes)
        {
            var effectiveScopeSet = scopes.Union(OAuth2Value.ReservedScopes);
            return effectiveScopeSet.AsSingleString();
        }

        public static async Task<WebAccountProvider> GetAccountProviderAsync(string tenant)
        {
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", // TODO bogavril: what about other clouds?
               tenant);

            return provider;
        }

        #endregion
    }


}