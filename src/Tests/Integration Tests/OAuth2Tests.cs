﻿/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityModel.Clients;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Tokens;

namespace Thinktecture.IdentityServer.Tests
{
    [TestClass]
    public class OAuth2Tests
    {
        string baseAddress = Constants.OAuth2.LocalBaseAddress;
        //string baseAddress = Constants.OAuth2.CloudBaseAddress;

        string scope = Constants.Realms.TestRP;

        [TestMethod]
        public void ValidUserNameCredential()
        {
            var client = new OAuth2Client(new Uri(baseAddress));

            var response = client.RequestAccessTokenUserName(
                Constants.Credentials.ValidUserName,
                Constants.Credentials.ValidPassword,
                scope);

            Assert.IsTrue(response != null, "response is null");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(response.AccessToken), "access token is null");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(response.TokenType), "token type is null");
            Assert.IsTrue(response.ExpiresIn > 0, "expiresIn is 0");

            Trace.WriteLine(response.AccessToken);
        }

        [TestMethod]
        public void ValidUserNameCredentialWithTokenValidation()
        {
            var client = new OAuth2Client(new Uri(baseAddress));

            var response = client.RequestAccessTokenUserName(
                Constants.Credentials.ValidUserName,
                Constants.Credentials.ValidPassword,
                scope);

            Assert.IsTrue(response != null, "response is null");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(response.AccessToken), "access token is null");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(response.TokenType), "token type is null");
            Assert.IsTrue(response.ExpiresIn > 0, "expiresIn is 0");

            Trace.WriteLine(response.AccessToken);

            var config = new SecurityTokenHandlerConfiguration();
            var registry = new WebTokenIssuerNameRegistry();
            registry.AddTrustedIssuer("http://identityserver45.thinktecture.com/trust/changethis", "http://identityserver45.thinktecture.com/trust/initial");
            config.IssuerNameRegistry = registry;

            var issuerResolver = new WebTokenIssuerTokenResolver();
            issuerResolver.AddSigningKey("http://identityserver45.thinktecture.com/trust/changethis", "3ihK5qGVhp8ptIk9+TDucXQW4Aaengg3d5m6gU8nzc8=");
            config.IssuerTokenResolver = issuerResolver;

            config.AudienceRestriction.AllowedAudienceUris.Add(new Uri(scope));

            var handler = new JsonWebTokenHandler();
            handler.Configuration = config;

            var jwt = handler.ReadToken(response.AccessToken);

            var id = handler.ValidateToken(jwt);
        }

        //[TestMethod]
        //public void ValidClientCertificateCredential()
        //{
        //    var client = new OAuth2Client(new Uri(baseAddress));

        //    var response = client.RequestAccessTokenCertificate(
        //        HttpClientFactory.GetValidClientCertificate(),
        //        scope);

        //    Assert.IsTrue(response != null, "response is null");
        //    Assert.IsTrue(!string.IsNullOrWhiteSpace(response.AccessToken), "access token is null");
        //    Assert.IsTrue(!string.IsNullOrWhiteSpace(response.TokenType), "token type is null");
        //    Assert.IsTrue(response.ExpiresIn > 0, "expiresIn is 0");

        //    Trace.WriteLine(response.AccessToken);
        //}

        [TestMethod]
        public void InvalidUserNameCredential()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { OAuth2Constants.GrantType, OAuth2Constants.Password },
                    { OAuth2Constants.UserName, Constants.Credentials.ValidUserName },
                    { OAuth2Constants.Password, "invalid" },
                    { OAuth2Constants.scope, scope }
                });

            var client = new HttpClient();
            var result = client.PostAsync(new Uri(baseAddress), form).Result;

            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.Unauthorized, result.StatusCode);            
        }

        [TestMethod]
        public void UnauthorizedUser()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { OAuth2Constants.GrantType, OAuth2Constants.Password },
                    { OAuth2Constants.UserName, Constants.Credentials.UnauthorizedUserName },
                    { OAuth2Constants.Password, Constants.Credentials.ValidUserName },
                    { OAuth2Constants.scope, scope }
                });

            var client = new HttpClient();
            var result = client.PostAsync(new Uri(baseAddress), form).Result;

            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [TestMethod]
        public void NoRealm()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { OAuth2Constants.GrantType, OAuth2Constants.Password },
                    { OAuth2Constants.UserName, Constants.Credentials.ValidUserName },
                    { OAuth2Constants.Password, Constants.Credentials.ValidUserName }
                });

            var client = new HttpClient();
            var result = client.PostAsync(new Uri(baseAddress), form).Result;

            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void MalformedRealm()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { OAuth2Constants.GrantType, OAuth2Constants.Password },
                    { OAuth2Constants.UserName, Constants.Credentials.ValidUserName },
                    { OAuth2Constants.Password, Constants.Credentials.ValidUserName },
                    { OAuth2Constants.scope, "invalid" }
                });

            var client = new HttpClient();
            var result = client.PostAsync(new Uri(baseAddress), form).Result;

            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void NoCredentials()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { OAuth2Constants.scope, scope }
                });

            var client = new HttpClient();
            var result = client.PostAsync(new Uri(baseAddress), form).Result;

            Assert.AreEqual<HttpStatusCode>(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}