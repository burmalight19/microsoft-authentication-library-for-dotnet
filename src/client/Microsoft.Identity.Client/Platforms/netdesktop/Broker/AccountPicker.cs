﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Instance;
using Microsoft.Identity.Client.Internal.Requests;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;

namespace Microsoft.Identity.Client.Platforms.netdesktop.Broker
{
    internal class AccountPicker 
    {
        private readonly IntPtr _parentHandle;
        private readonly ICoreLogger _logger;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly Authority _authority;

        private volatile WebAccountProvider _provider;

        public AuthenticationRequestParameters temporaryRequestParams { get; internal set; }

        public AccountPicker(IntPtr parentHandle, ICoreLogger logger, SynchronizationContext synchronizationContext, Authority authority)
        {
            _parentHandle = parentHandle;
            _logger = logger;
            _synchronizationContext = synchronizationContext;
            _authority = authority;
        }

        public async Task<WebAccountProvider> DetermineAccountInteractivelyAsync()
        {
            WebAccountProvider result = null;
            var sendAuthorizeRequestWithTcs = new Action<object>(async (tcs) =>
            {
                try
                {
                    result = await ShowPickerAsync().ConfigureAwait(true);
                    ((TaskCompletionSource<object>)tcs).TrySetResult(null);
                }
                catch (Exception e)
                {
                    // Need to catch the exception here and put on the TCS which is the task we are waiting on so that
                    // the exception comming out of Authenticate is correctly thrown.
                    ((TaskCompletionSource<object>)tcs).TrySetException(e);
                }
            });

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
            {
                if (_synchronizationContext != null)
                {
                    var tcs = new TaskCompletionSource<object>();
                    _synchronizationContext.Post(new SendOrPostCallback(sendAuthorizeRequestWithTcs), tcs);
                    await tcs.Task.ConfigureAwait(true);
                }
                else
                {
                    throw new InvalidOperationException(); // TODO: syncronization context null?
                }
            }

            return result;
        }

        private async Task<WebAccountProvider> ShowPickerAsync()
        {
            AccountsSettingsPane retaccountPane = null;
            try
            {
                retaccountPane = AccountsSettingsPaneInterop.GetForWindow(_parentHandle);
                retaccountPane.AccountCommandsRequested += Authenticator_AccountCommandsRequested;
                await AccountsSettingsPaneInterop.ShowAddAccountForWindowAsync(_parentHandle);

              
                return _provider;
            }
            catch (Exception e)
            {
                _logger.ErrorPii(e);
                throw;
            }
            finally
            {
                if (retaccountPane != null)
                {
                    retaccountPane.AccountCommandsRequested -= Authenticator_AccountCommandsRequested;
                }
            }
        }

        private async void Authenticator_AccountCommandsRequested(
            AccountsSettingsPane sender,
            AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            AccountsSettingsPaneEventDeferral deferral = null;
            try
            {
                deferral = e.GetDeferral();

                if (string.Equals("common", _authority.TenantId))
                {
                    _logger.Verbose("Displaying selector for common");
                    e.WebAccountProviderCommands.Add(
                        new WebAccountProviderCommand(
                            await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers"),
                            WebAccountProviderCommandInvoked));

                    e.WebAccountProviderCommands.Add(
                        new WebAccountProviderCommand(
                            await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "organizations"),
                            WebAccountProviderCommandInvoked));
                }
                else if (string.Equals("organizations", _authority.TenantId))
                {
                    _logger.Verbose("Displaying selector for organizations");

                    e.WebAccountProviderCommands.Add(
                       new WebAccountProviderCommand(
                           await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "organizations"),
                           WebAccountProviderCommandInvoked));
                }
                else if (string.Equals("consumers", _authority.TenantId))
                {
                    _logger.Verbose("Displaying selector for consumers");

                    e.WebAccountProviderCommands.Add(
                      new WebAccountProviderCommand(
                          await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers"),
                          WebAccountProviderCommandInvoked));
                }
                else
                {
                    _logger.Verbose("Displaying selector for tenanted authority");

                    e.WebAccountProviderCommands.Add(
                        new WebAccountProviderCommand(
                            await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", _authority.AuthorityInfo.CanonicalAuthority),
                        WebAccountProviderCommandInvoked));
                }

                e.HeaderText = "Please select an account to log in with"; // TODO: this is English only, try removing it
            }
            finally
            {
                deferral?.Complete();
            }
        }

        private void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
        {
            _provider = command.WebAccountProvider;
        }
    }
}
