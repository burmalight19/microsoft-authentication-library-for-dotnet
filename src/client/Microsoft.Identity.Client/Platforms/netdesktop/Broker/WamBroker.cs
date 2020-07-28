using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Identity.Client.ApiConfig.Parameters;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Internal.Broker;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Client.OAuth2;
using Microsoft.Identity.Client.UI;
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


        private CoreUIParent _uiParent;
        private ICoreLogger _logger;


        public WamBroker(CoreUIParent uiParent, ICoreLogger logger)
        {

            _uiParent = uiParent;
            _logger = logger;

            _aadPlugin = new WamAADPlugin(_logger, _uiParent);
            _msaPlugin = new WamMSAPlugin();

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
            // TODO: bogavril - too many authority objects...
            string tenantId = authenticationRequestParameters.OriginalAuthority.TenantId;
            bool isMsa = await IsMsaSilentRequestAsync(tenantId).ConfigureAwait(false);
            IWamPlugin wanPlugin = isMsa ? _msaPlugin : _aadPlugin;
            WebAccountProvider provider = await wanPlugin.
                GetAccountProviderAsync(authenticationRequestParameters.AuthorityInfo.CanonicalAuthority).ConfigureAwait(false);

            // In C++ impl WAM Account ID is stored in the cache and GetAccounts write the WAM derived accounts to the cache, which is a perf optimization.
            WebAccount webAccount = await wanPlugin.FindWamAccountForMsalAccountAsync(
                provider,
                authenticationRequestParameters.Account,
                authenticationRequestParameters.LoginHint,
                authenticationRequestParameters.ClientId).ConfigureAwait(false);

            WebTokenRequest webTokenRequest = wanPlugin.CreateWebTokenRequest(
                provider,
                false /* is interactive */,
                webAccount != null, /* is account in WAM */
                authenticationRequestParameters);

            AddExtraParamsToRequest(webTokenRequest, authenticationRequestParameters.ExtraQueryParameters);
            // TODO bogavril: add POP support by adding "token_type" = "pop" and "req_cnf" = req_cnf

            WebTokenRequestResult wamResult;
            if (webAccount!=null)
            {
                wamResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest, webAccount);
            }
            else
            {
                // TODO bogavril - question - what does this do ?
                wamResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest);
            }

            return CreateMsalTokenResponse(wamResult, wanPlugin, isInteractive: false);
        }

        private MsalTokenResponse CreateMsalTokenResponse(
            WebTokenRequestResult wamResponse, 
            IWamPlugin wamPlugin, 
            bool isInteractive)
        {
            switch (wamResponse.ResponseStatus)
            {
                case WebTokenRequestStatus.Success:
                    return wamPlugin.ParseSuccesfullWamResponse(wamResponse.ResponseData[0]);
                case WebTokenRequestStatus.UserInteractionRequired:
                    string status = 
            }

            return null;
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

        /// <summary>
        /// 
        /// MSA request if: 
        ///  - tenant is "common" AND default WAM account in MSA
        ///  - tenant is "consumers"
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<bool> IsMsaSilentRequestAsync(string tenantId)
        {
            if (string.Equals("common", tenantId, StringComparison.OrdinalIgnoreCase))
            {
                bool isMsa = await IsDefaultAccountMsaAsync().ConfigureAwait(false);
                _logger.Verbose("[WAM Broker] Tenant: common. Default WAM account is MSA? " + isMsa);
                return isMsa;
            }

            if (string.Equals("consumers", tenantId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Verbose("[WAM Broker] Tenant is consumers. ATS will try WAM-MSA ");
                return true;
            }

            _logger.Verbose("[WAM Broker] ATS will try WAM-AAD");
            return false;
        }



        public async Task<IEnumerable<IAccount>> GetAccountsAsync(string clientID, string redirectUri)
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
            throw new NotImplementedException();
        }

        private async Task<WebAccountProvider> GetDefaultAccountProviderAsync()
        {
            return await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.windows.local");
        }

        private async Task<bool> IsDefaultAccountMsaAsync()
        {
            var provider = await GetDefaultAccountProviderAsync().ConfigureAwait(false);
            return provider != null && string.Equals("consumers", provider.Authority);
        }
    }


}
