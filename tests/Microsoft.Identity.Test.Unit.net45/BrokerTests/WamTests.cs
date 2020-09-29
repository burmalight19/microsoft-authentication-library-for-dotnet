using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal.Broker;
using Microsoft.Identity.Client.OAuth2;
using Microsoft.Identity.Client.Utils;
using Microsoft.Identity.Test.Common.Core.Helpers;
using Microsoft.Identity.Test.Common.Core.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Microsoft.Identity.Test.Unit.BrokerTests
{
#if DESKTOP
    [TestClass]
    public class WamTests : TestBase
    {
        [TestMethod]
        public async Task WAM_AccountIdWriteback_Async()
        {
            // Arrange
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddInstanceDiscoveryMockHandler();

                var mockBroker = Substitute.For<IBroker>();
                mockBroker.IsBrokerInstalledAndInvokable().Returns(true);
                
                var msalTokenResponse = CreateMsalTokenResponseFromWam("wam1");
                mockBroker.AcquireTokenInteractiveAsync(null, null).ReturnsForAnyArgs(Task.FromResult(msalTokenResponse));

                var pca = PublicClientApplicationBuilder.Create(TestConstants.ClientId)
                    .WithBroker(true)
                    .WithHttpManager(httpManager)                    
                    .BuildConcrete();

                pca.ServiceBundle.PlatformProxy.SetBrokerForTest(mockBroker);

                // Act
                await pca.AcquireTokenInteractive(TestConstants.s_scope).ExecuteAsync().ConfigureAwait(false);
                var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);

                // Assert
                var wamAccountIds = (accounts.Single() as IAccountInternal).WamAccountIds;
                Assert.AreEqual(1, wamAccountIds.Count);
                Assert.AreEqual("wam1", wamAccountIds[TestConstants.ClientId]);

                var pca2 = PublicClientApplicationBuilder.Create(TestConstants.ClientId2)
                    .WithBroker(true)
                    .WithHttpManager(httpManager)
                    .BuildConcrete();
                pca2.ServiceBundle.PlatformProxy.SetBrokerForTest(mockBroker);
                var accounts2 = await pca2.GetAccountsAsync().ConfigureAwait(false);
                Assert.IsFalse(accounts2.Any());
            }
        }

      

        private static MsalTokenResponse CreateMsalTokenResponseFromWam(string wamAccountId)
        {
            return new MsalTokenResponse
            {
                IdToken = MockHelpers.CreateIdToken(TestConstants.UniqueId, TestConstants.DisplayableId),
                AccessToken = "access-token",
                ClientInfo = MockHelpers.CreateClientInfo(),
                ExpiresIn = 3599,
                CorrelationId = "correlation-id",
                RefreshToken = null, // brokers don't return RT
                Scope = TestConstants.s_scope.AsSingleString(),
                TokenType = "Bearer", 
                WamAccountId = wamAccountId,
            };
        }
    }
#endif
}
