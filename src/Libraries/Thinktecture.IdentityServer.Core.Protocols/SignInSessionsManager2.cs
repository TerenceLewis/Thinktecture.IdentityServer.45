/*
 * Copyright (c) Dominick Baier.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Thinktecture.IdentityServer.Protocols
{
    public class SignInSessionsManager2
    {
        private const string COOKIENAME = ".idsrvsso";
        
        HttpRequestMessage _request;
        HttpResponseMessage _response;
        int _maximumCookieLifetime;

        public SignInSessionsManager2(HttpRequestMessage request) : this(request, null, 24)
        { }

        public SignInSessionsManager2(HttpResponseMessage response) : this(null, response, 24)
        { }


        public SignInSessionsManager2(HttpRequestMessage request, HttpResponseMessage response, int maximumCookieLifetime)
        {
            _request = request;
            _response = response;
            _maximumCookieLifetime = maximumCookieLifetime;
        }

        public void AddRealm(string realm)
        {
            var realms = ReadCookie();
            if (!realms.Contains(realm.ToLowerInvariant()))
            {
                realms.Add(realm.ToLowerInvariant());
                WriteCookie(realms);
            }
        }

        public List<string> GetRealms()
        {
            return ReadCookie();
        }

        public void ClearRealms()
        {
            //var cookie = (from c in _request.Headers.GetCookies()
            //              where c.Cookies.First().Name == COOKIENAME
            //              select c.Cookies.First())
            //             .FirstOrDefault();

            //if (cookie != null)
            //{
            //    cookie.Value = "";
            //    cookie.Expires = new DateTime(2000, 1, 1);
            //    _context.Response.SetCookie(cookie);
            //}
        }

        private List<string> ReadCookie()
        {
            var cookie = (from c in _request.Headers.GetCookies()
                          where c.Cookies.First().Name == COOKIENAME
                          select c.Cookies.First())
                         .FirstOrDefault();

            if (cookie == null)
            {
                return new List<string>();
            }

            return cookie.Value.Split('|').ToList();
        }

        private void WriteCookie(List<string> realms)
        {
            if (realms.Count == 0)
            {
                ClearRealms();
                return;
            }

            var realmString = string.Join("|", realms).ToLowerInvariant();

            var cookie = new CookieHeaderValue(COOKIENAME, realmString)
            {
                Expires = DateTime.Now.AddHours(_maximumCookieLifetime),
                HttpOnly = true,
                Secure = true
            };

            _response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
        }
    }
}
