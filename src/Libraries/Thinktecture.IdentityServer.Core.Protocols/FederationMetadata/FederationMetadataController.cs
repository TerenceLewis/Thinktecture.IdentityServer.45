using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Thinktecture.IdentityServer.Repositories;

namespace Thinktecture.IdentityServer.Protocols.FederationMetadata
{
    public class FederationMetadataController : ApiController
    {
        [Import]
        public IConfigurationRepository ConfigurationRepository { get; set; }

        //[Import]
        //public ICacheRepository CacheRepository { get; set; }

        public FederationMetadataController()
        {
            Container.Current.SatisfyImportsOnce(this);
        }

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            if (ConfigurationRepository.Endpoints.FederationMetadata)
            {
                var path = request.RequestUri.AbsolutePath;
                var appPath = path.Substring(0, path.Length - "FederationMetadata/2007-06/FederationMetadata.xml".Length);

                var endpoints = Endpoints.Create(
                    request.Headers.Host,
                    appPath,
                    ConfigurationRepository.Endpoints.HttpPort,
                    ConfigurationRepository.Endpoints.HttpsPort);

                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(new WSFederationMetadataGenerator(endpoints).Generate(), Encoding.UTF8, "application/xml");

                return resp;
            }
            else
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }
    }
}
