using HTTPSPlayground.API.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace HTTPSPlayground.API.Services
{
    public interface IEndpointDetailsService
    {
        ICollection<string> GentlyGetEndpointAddresses();
        EndpointDetailsViewModel HackEndpointDetails();
    }
    public class EndpointDetailsService : IEndpointDetailsService
    {
        private readonly IServer server;
        private readonly IConfiguration configuration;

        public EndpointDetailsService(IServer server, IConfiguration configuration)
        {
            // the recomended way
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        }

        // lifted shamelessly from https://github.com/aspnet/AspNetCore/blob/master/src/Hosting/Hosting/src/GenericHost/GenericWebHostedService.cs#L66
        public ICollection<string> GentlyGetEndpointAddresses()
        {
            IServerAddressesFeature serverAddressesFeature = server.Features?.Get<IServerAddressesFeature>();
            ICollection<string> addresses = serverAddressesFeature?.Addresses;
            if (addresses != null && !addresses.IsReadOnly && addresses.Count == 0)
            {
                string urls = configuration[WebHostDefaults.ServerUrlsKey];
                if (!string.IsNullOrEmpty(urls))
                {
                    serverAddressesFeature.PreferHostingUrls = ParseBool(configuration, WebHostDefaults.PreferHostingUrlsKey);

                    foreach (string value in urls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        addresses.Add(value);
                    }
                }
            }

            return addresses;
        }

        public static bool ParseBool(IConfiguration configuration, string key)
        {
            return string.Equals("true", configuration[key], StringComparison.OrdinalIgnoreCase)
                || string.Equals("1", configuration[key], StringComparison.OrdinalIgnoreCase);
        }

        // David Fowler would really like us not to do this
        // https://github.com/aspnet/KestrelHttpServer/issues/2306#issuecomment-364478486
        public EndpointDetailsViewModel HackEndpointDetails()
        {
            var model = new EndpointDetailsViewModel();
            try
            {
                KestrelServer kestrel = server as KestrelServer;
                if (kestrel == null)
                {
                    model.NotKestrel = true;
                    return model;
                }

                KestrelServerOptions options = kestrel.Options;

                // reflection voodoo
                Type kestrelServerOptionsType = typeof(KestrelServerOptions);
                PropertyInfo listenOptionsProp = kestrelServerOptionsType.GetProperty("ListenOptions", BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo isTlsProp = typeof(ListenOptions).GetProperty("IsTls", BindingFlags.Instance | BindingFlags.NonPublic);
                List<ListenOptions> listenOptions = (List<ListenOptions>)listenOptionsProp.GetValue(options);

                foreach (ListenOptions listenOption in listenOptions)
                {
                    bool isTls = (bool)isTlsProp.GetValue(listenOption);

                    // Grab all the details for this endpoint
                    EndpointDetail endpointDetail = new EndpointDetail
                    {
                        Address = listenOption.IPEndPoint.Address.ToString(),
                        Port = listenOption.IPEndPoint.Port,
                        IsHttps = isTls
                    };
                    model.EndpointDetails.Add(endpointDetail);

                    if (isTls)
                    {
                        // it appears all middleware is configured for all listenOptions even if they aren't https
                        endpointDetail.Certificate = GetCertificateFromOptions(listenOption);
                    }
                }

                // Reflect the dev cert
                model.IsDevCertLoaded = (bool)(kestrelServerOptionsType.GetProperty("IsDevCertLoaded", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(options));
                model.DefaultCertificate = kestrelServerOptionsType.GetProperty("DefaultCertificate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(options) as X509Certificate2;

            }
            catch (Exception ex)
            {
                // because this is hacky enough that it'll likely fall down easily
                model.Exception = ex.Message;
            }
            return model;
        }

        private static X509Certificate2 GetCertificateFromOptions(ListenOptions listenOption)
        {
            X509Certificate2 cert = null;

            FieldInfo middlewareProp = typeof(ListenOptions).GetField("_middleware", BindingFlags.Instance | BindingFlags.NonPublic);
            List<Func<ConnectionDelegate, ConnectionDelegate>> middleware = (List<Func<ConnectionDelegate, ConnectionDelegate>>)middlewareProp.GetValue(listenOption);
            if (middleware == null)
            {
                return cert;
            }

            foreach (var mid in middleware)
            {
                var target = mid.Target; // a generated type
                FieldInfo httpsOptionsProp = target.GetType().GetField("httpsOptions", BindingFlags.Instance | BindingFlags.Public);
                if (httpsOptionsProp == null)
                {
                    continue;
                }
                HttpsConnectionAdapterOptions httpsOptions = httpsOptionsProp.GetValue(target) as HttpsConnectionAdapterOptions;
                if (httpsOptions == null)
                {
                    continue;
                }
                cert = httpsOptions.ServerCertificate;
            }
            return cert;
        }

    }
}
