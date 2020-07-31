using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client.ApiConfig.Parameters;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Client.OAuth2;
using Microsoft.Identity.Client.UI;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace Microsoft.Identity.Client.Platforms.netdesktop.Broker
{
    internal class MsaPlugin : IWamPlugin
    {
        private readonly ICoreLogger _logger;
        private readonly CoreUIParent _uiParent;

        public MsaPlugin(ICoreLogger logger, CoreUIParent uiParent)
        {
            _logger = logger;
            _uiParent = uiParent;
        }

        public WebTokenRequest CreateWebTokenRequest(WebAccountProvider provider, bool isInteractive, bool isAccountInWam, AuthenticationRequestParameters authenticationRequestParameters)
        {
            throw new NotImplementedException();
        }

        public Task<WebAccount> FindWamAccountForMsalAccountAsync(WebAccountProvider provider, IAccount account, string loginHint, string clientId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generally the MSA plugin will NOT return the accounts back to the app. This is due
        /// to privacy concerns. However, some test apps are allowed to do this, hence the code. 
        /// Normal 1st and 3rd party apps must use AcquireTokenInteractive to login first, and then MSAL will
        /// save the account for later use.
        /// </summary>
        public async Task<IEnumerable<IAccount>> GetAccountsAsync(string clientID)
        {
            var webAccounProvider = await WamBroker.GetAccountProviderAsync("consumers").ConfigureAwait(false);
            WamProxy wamProxy = new WamProxy(webAccounProvider, _logger);

            var webAccounts = await wamProxy.FindAllWebAccountsAsync(clientID).ConfigureAwait(false);

            var msalAccounts = webAccounts
                .Select(webAcc => ConvertToMsalAccountOrNull(webAcc))
                .Where(a => a != null)
                .ToList();

            _logger.Info($"[WAM MSA Provider] GetAccountsAsync converted {webAccounts.Count()} MSAL accounts");
            return msalAccounts;
        }

        private IAccount ConvertToMsalAccountOrNull(WebAccount webAccount)
        {
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad"; // TODO: bogavril - is there a different value in PPE?
            const string environment = "login.windows.net"; //TODO: bogavril - other clouds?

            if (!webAccount.Properties.TryGetValue("SafeCustomerId", out string cid))
            {
                _logger.Warning("[WAM MSA Provider] MSAL account cannot be created without MSA CID");
                return null;
            }

            if (!TryConvertCidToGuid(cid, out string localAccountId))
            {
                _logger.WarningPii($"[WAM MSA Provider] Invalid CID: {cid}", $"[WAM MSA Provider] Invalid CID, lenght {cid.Length}");
                return null;
            }

            if (localAccountId == null)
            {
                return null;
            }

            string homeAccountId = localAccountId + "." + msaTenantId;

            return new Account(homeAccountId, webAccount.UserName, environment);
        }

        // There are two commonly used formats for MSA CIDs:
        //    - hex format, which is a fixed length 16 characters string.
        //    - GUID format, which is the hex value CID prefixed with '00000000-0000-0000-'
        // For example for hex CID value '540648eb0b3075bb' the corresponding GUID representation is
        // '00000000-0000-0000-5406-48eb0b3075bb'
        // This helper method converts MSA CID from the Hex format to GUID format.
        private bool TryConvertCidToGuid(string cid, out string localAccountId)
        {
            if (cid.Length != 16)
            {
                localAccountId = null;
                return false;
            }

            string lowercaseCid = cid.ToLowerInvariant();
            localAccountId = "00000000-0000-0000-" + lowercaseCid.Insert(4, "-");
            return true;
        }


        public string MapTokenRequestError(WebTokenRequestStatus status, uint errorCode, bool isInteractive)
        {
            throw new NotImplementedException();
        }

        public MsalTokenResponse ParseSuccesfullWamResponse(WebTokenResponse webTokenResponse)
        {
            throw new NotImplementedException();
        }
    }


}
