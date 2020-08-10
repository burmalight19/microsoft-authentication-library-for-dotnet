using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Identity.Client;
using Windows.Storage.Streams;

namespace NetDesktopWinForms
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();
        }

        public static readonly string UserCacheFile = 
            System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.user.json";


        private IPublicClientApplication CreatePca()
        {
            var pca = PublicClientApplicationBuilder
                .Create(this.clientIdCbx.Text)
                .WithAuthority(this.authorityCbx.Text)
                .WithBroker(this.useBrokerChk.Checked)
                .Build();
            
            BindCache(pca.UserTokenCache, UserCacheFile);
            return pca;
        }

        private static void BindCache(ITokenCache tokenCache, string file)
        {
            tokenCache.SetBeforeAccess(notificationArgs =>
            {
                notificationArgs.TokenCache.DeserializeMsalV3(File.Exists(file)
                    ? File.ReadAllBytes(UserCacheFile)
                    : null);
            });

            tokenCache.SetAfterAccess(notificationArgs =>
            {
                // if the access operation resulted in a cache update
                if (notificationArgs.HasStateChanged)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(file, notificationArgs.TokenCache.SerializeMsalV3());
                }
            });
        }

        private async void atsBtn_Click(object sender, EventArgs e)
        {

            try
            {
                var pca = CreatePca();
                var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
                string loginHint = GetLoginHint();
                IAccount account =
                    string.IsNullOrEmpty(loginHint) ? accounts.FirstOrDefault() : accounts.First(aa => aa.Username == loginHint);
                Log($"ATS for {account?.Username}");
                AuthenticationResult result = await pca.AcquireTokenSilent(GetScopes(), account)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LogResult(result);

            }
            catch (Exception ex)
            {
                Log("Exception: " + ex);
            }

        }

        private string[] GetScopes()
        {
            string[] result = null;
            cbxScopes.Invoke((MethodInvoker)delegate
            {
                result = cbxScopes.Text.Split(' ');
            });

            return result;
        }

        private void LogResult(AuthenticationResult ar)
        {
            string message =

                $"Account.Username {ar.Account.Username}" + Environment.NewLine +
                $"Account.HomeAccountId {ar.Account.HomeAccountId}" + Environment.NewLine +
                $"Account.Environment {ar.Account.Environment}" + Environment.NewLine +
                $"TenantId {ar.TenantId}" + Environment.NewLine +
                $"Expires {ar.ExpiresOn.ToLocalTime()} local time" + Environment.NewLine +
                $"Source {ar.AuthenticationResultMetadata.TokenSource}" + Environment.NewLine +
                $"Scopes {String.Join(" ", ar.Scopes)}" + Environment.NewLine +                
                $"AccessToken: {ar.AccessToken} " + Environment.NewLine +
                $"IdToken {ar.IdToken}" + Environment.NewLine;

            Log(message);
                
        }

        private void Log(string message)
        {
            resultTbx.Invoke((MethodInvoker)delegate
            {
               resultTbx.AppendText(message + Environment.NewLine);
           });
        }

        private async void atiBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var pca = CreatePca();

                AuthenticationResult result = await pca.AcquireTokenInteractive(GetScopes())
                    .WithPrompt(GetPrompt())
                    .WithLoginHint(GetLoginHint())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LogResult(result);

            }
            catch (Exception ex)
            {
                Log("Exception: " + ex);
            }
        }

        private string GetLoginHint()
        {
            string loginHint = null;
            loginHintTxt.Invoke((MethodInvoker)delegate
            {
                loginHint = loginHintTxt.Text;
            });

            return loginHint;
        }

        private Prompt GetPrompt()
        {
            string prompt = null;
            promptCbx.Invoke((MethodInvoker)delegate
            {
                prompt = promptCbx.Text;
            });

            if (string.IsNullOrEmpty(prompt))
                return Prompt.SelectAccount;


            switch (prompt)
            {

                case "select_account":
                    return Prompt.SelectAccount;
                case "force_login":
                    return Prompt.ForceLogin;
                case "no_prompt":
                    return Prompt.NoPrompt;
                case "consent":
                    return Prompt.Consent;
                case "never":
                    return Prompt.Never;


                default:
                    throw new NotImplementedException();
            }
        }

        private async void accBtn_Click(object sender, EventArgs e)
        {
            var pca = CreatePca();
            var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
            string msg = string.Join(
                " ",
                accounts.Select(acc => $"{acc.Username} {acc.Environment} {acc.HomeAccountId.TenantId}"));
            Log(msg);
        }

        private async void atsAtiBtn_Click(object sender, EventArgs e)
        {
            var pca = CreatePca();
            var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);

            string loginHint = GetLoginHint();
            IAccount account =
                string.IsNullOrEmpty(loginHint) ? accounts.FirstOrDefault() : accounts.First(aa => aa.Username == loginHint);

            try
            {
                Log($"ATS for {account?.Username}");
                AuthenticationResult result = await pca.AcquireTokenSilent(GetScopes(), account)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LogResult(result);

            }
            catch (MsalUiRequiredException ex)
            {
                Log("UI required Exception!");
                try
                {
                    AuthenticationResult result2 = await pca.AcquireTokenInteractive(GetScopes())
                        .WithAccount(account)
                        .WithPrompt(GetPrompt())
                        .WithLoginHint(GetLoginHint())
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                    LogResult(result2);

                }
                catch(Exception ex3)
                {
                    Log("Exception: " + ex3);
                }

            }
            catch (Exception ex2)
            {
                Log("Exception: " + ex2);
            }
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            resultTbx.Text = "";
        }

        private async void btnClearCache_Click(object sender, EventArgs e)
        {
            var pca = CreatePca();
            foreach (var acc in (await pca.GetAccountsAsync().ConfigureAwait(false)))
            {
                await pca.RemoveAsync(acc).ConfigureAwait(false);
            }
        }
    }
}
