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
using Microsoft.Identity.Client;

namespace NetDesktopWinForms
{
    public partial class Form1 : Form
    {
        private static string[] s_scopes = new string[] { "User.Read" };
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
                var account = accounts.FirstOrDefault();
                Log($"ATS for {account?.Username}");
                AuthenticationResult result = await pca.AcquireTokenSilent(s_scopes, account)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LogResult(result);

            }
            catch (Exception ex)
            {
                Log("Exception: " + ex);
            }

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
               resultTbx.Text += message + Environment.NewLine;
           });
        }

        private async void atiBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var pca = CreatePca();

                AuthenticationResult result = await pca.AcquireTokenInteractive(s_scopes)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LogResult(result);

            }
            catch (Exception ex)
            {
                Log("Exception: " + ex);
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
            var account = accounts.FirstOrDefault();

            try
            {
                Log($"ATS for {account?.Username}");
                AuthenticationResult result = await pca.AcquireTokenSilent(s_scopes, account)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LogResult(result);

            }
            catch (MsalUiRequiredException ex)
            {
                Log("UI required Exception!");
                try
                {
                    AuthenticationResult result2 = await pca.AcquireTokenInteractive(s_scopes)
                        .WithAccount(account)
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
    }
}
