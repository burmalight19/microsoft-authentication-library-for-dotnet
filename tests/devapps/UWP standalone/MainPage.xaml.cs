using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading;
using System.Collections.ObjectModel;
using Windows.Security.Authentication.Web;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP_standalone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly string s_clientID = "81179aff-797c-49c3-94bd-04ff14feec7d";
        private static readonly string s_authority = "https://login.microsoftonline.com/common/";
        private static readonly IEnumerable<string> s_scopes = new[] { "user.read" };
        private const string CacheFileName = "msal_user_cache.json";


        public MainPage()
        {
            InitializeComponent();

            // redirect URI required for WAM!
            string sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host;
            string redirectUri = $"ms-appx-web://Microsoft.AAD.BrokerPlugin/{sid}";
        }



        private IPublicClientApplication CreatePublicClient()
        {
            return PublicClientApplicationBuilder.Create(s_clientID)
                .WithAuthority(s_authority)
                .WithBroker(chkUseBroker.IsChecked.Value)
                .WithLogging((x, y, z) => Debug.WriteLine($"{x} {y}"), LogLevel.Verbose, true)
                .Build();
        }

        private async void AcquireTokenIWA_ClickAsync(object sender, RoutedEventArgs e)
        {
            var pca = CreatePublicClient();
            AuthenticationResult result = null;
            try
            {
                result = await pca.AcquireTokenByIntegratedWindowsAuth(s_scopes).ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await DisplayErrorAsync(ex).ConfigureAwait(false);
                return;
            }

            await DisplayResultAsync(result).ConfigureAwait(false);
        }


        private async void GetAccountsAsync(object sender, RoutedEventArgs e)
        {
            var pca = CreatePublicClient();
            IEnumerable<IAccount> accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
            foreach (IAccount account in accounts)
            {
                await DisplayMessageAsync($"{account.Username} .... from {account.Environment}").ConfigureAwait(false);
            }
        }

        private async void ClearCacheAsync(object sender, RoutedEventArgs e)
        {
            var pca = CreatePublicClient();

            IEnumerable<IAccount> accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
            foreach (IAccount account in accounts)
            {
                await pca.RemoveAsync(account).ConfigureAwait(false);
            }
        }


        private async void ATS_ClickAsync(object sender, RoutedEventArgs e)
        {
            var pca = CreatePublicClient();
            var upnPrefix = tbxUpn.Text;

            IEnumerable<IAccount> accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
            var acc = accounts.SingleOrDefault(a => a.Username.StartsWith(upnPrefix));

            AuthenticationResult result = null;
            try
            {
                result = await pca
                    .AcquireTokenSilent(s_scopes, acc)
                    .ExecuteAsync(CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await DisplayErrorAsync(ex).ConfigureAwait(false);
                return;
            }

            await DisplayResultAsync(result).ConfigureAwait(false);

        }

        private async void ATI_ClickAsync(object sender, RoutedEventArgs e)
        {
            var pca = CreatePublicClient();
            var upnPrefix = tbxUpn.Text;

            IEnumerable<IAccount> accounts = await pca.GetAccountsAsync().ConfigureAwait(true); // stay on UI thread
            var acc = accounts.SingleOrDefault(a => a.Username.StartsWith(upnPrefix));

            try
            {
                var result = await pca.AcquireTokenInteractive(s_scopes)
                    .WithAccount(acc)
                    .ExecuteAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                await DisplayResultAsync(result).ConfigureAwait(false);


            }
            catch (Exception ex)
            {
                await DisplayErrorAsync(ex).ConfigureAwait(false);
                return;
            }

        }

        private async Task DisplayErrorAsync(Exception ex)
        {
            await DisplayMessageAsync(ex.Message).ConfigureAwait(false);
        }

        private async Task DisplayResultAsync(AuthenticationResult result)
        {
            await DisplayMessageAsync("Signed in User - " + result.Account.Username + "\nAccessToken: \n" + result.AccessToken).ConfigureAwait(false);
        }


        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       AccessToken.Text = message;
                   });
        }
    }
}
