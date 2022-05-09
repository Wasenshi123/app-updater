using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Updater.Utils;

namespace Updater
{
    public class CustomHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var httpMessageHandler = base.CreateMessageHandler();

            // By default, Flurl creates HttpClientHandlers as message handlers.
            // Confirm this is what it did, and then attach a custom behavior.
            if (httpMessageHandler is HttpClientHandler httpClientHandler)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            else
            {
                Logger.LogError($"HttpMessageHandler is type {httpMessageHandler.GetType().Name}. Cannot set custom certification validation callback.");
            }

            return httpMessageHandler;
        }

    }
}
