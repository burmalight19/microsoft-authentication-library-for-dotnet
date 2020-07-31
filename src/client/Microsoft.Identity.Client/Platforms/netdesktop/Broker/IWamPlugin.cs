using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Client.OAuth2;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace Microsoft.Identity.Client.Platforms.netdesktop.Broker
{
    internal interface IWamPlugin
    {
        Task<IEnumerable<IAccount>> GetAccountsAsync(string clientID);
        
        Task<WebAccount> FindWamAccountForMsalAccountAsync(
            WebAccountProvider provider, 
            IAccount account,   
            string loginHint, // TODO: ATS always requires an account
            string clientId);

        WebTokenRequest CreateWebTokenRequest(
            WebAccountProvider provider, 
            bool isInteractive,
            bool isAccountInWam,
            AuthenticationRequestParameters authenticationRequestParameters);

        MsalTokenResponse ParseSuccesfullWamResponse(WebTokenResponse webTokenResponse);

        string MapTokenRequestError(WebTokenRequestStatus status, uint errorCode, bool isInteractive);
    }
}
