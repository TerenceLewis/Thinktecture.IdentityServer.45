using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IdentityModel.Services;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Repositories;
using Thinktecture.IdentityServer.TokenService;

namespace Thinktecture.IdentityServer.Protocols.WSFederation
{
    //[ClaimsAuthorize(Constants.Actions.Issue, Constants.Resources.WSFederation)]
    public class WSFedController : ApiController
    {     
        [Import]
        public IConfigurationRepository ConfigurationRepository { get; set; }

        public WSFedController()
        {
            Container.Current.SatisfyImportsOnce(this);
        }

        public WSFedController(IConfigurationRepository configurationRepository)
        {
            ConfigurationRepository = configurationRepository;
        }

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            Tracing.Verbose("WS-Federation endpoint called.");

            if (!ConfigurationRepository.Endpoints.WSFederation)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var message = WSFederationMessage.CreateFromUri(request.RequestUri);

            // sign in 
            var signinMessage = message as SignInRequestMessage;
            if (signinMessage != null)
            {
                return ProcessWSFederationSignIn(signinMessage, ClaimsPrincipal.Current);
            }

            //// sign out
            //var signoutMessage = message as SignOutRequestMessage;
            //if (signoutMessage != null)
            //{
            //    return ProcessWSFederationSignOut(signoutMessage);
            //}

            //return View("Error");

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
        }

        #region Helper
        private HttpResponseMessage ProcessWSFederationSignIn(SignInRequestMessage message, ClaimsPrincipal principal)
        {
            // issue token and create ws-fed response
            var response = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(
                message,
                principal as ClaimsPrincipal,
                TokenServiceConfiguration.Current.CreateSecurityTokenService());

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            // set cookie for single-sign-out
            new SignInSessionsManager2(null, httpResponse, ConfigurationRepository.Configuration.MaximumTokenLifetime)
                .AddRealm(response.BaseUri.AbsoluteUri);

            httpResponse.Content = new StringContent(response.WriteFormPost());
            return httpResponse;
        }

        //private ActionResult ProcessWSFederationSignOut(SignOutRequestMessage message)
        //{
        //    FederatedAuthentication.SessionAuthenticationModule.SignOut();

        //    // check for return url
        //    if (!string.IsNullOrWhiteSpace(message.Reply))
        //    {
        //        ViewBag.ReturnUrl = message.Reply;
        //    }

        //    // check for existing sign in sessions
        //    var mgr = new SignInSessionsManager(HttpContext);
        //    var realms = mgr.GetRealms();
        //    mgr.ClearRealms();
            
        //    return View("Signout", realms);
        //}
        #endregion
    }
}
