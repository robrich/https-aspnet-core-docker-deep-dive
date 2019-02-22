using HTTPSPlayground.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Https.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace HTTPSPlayground.Web.Services
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
			if (addresses != null && !addresses.IsReadOnly && addresses.Count == 0) {
				string urls = configuration[WebHostDefaults.ServerUrlsKey];
				if (!string.IsNullOrEmpty(urls)) {
					serverAddressesFeature.PreferHostingUrls = WebHostUtilities.ParseBool(configuration, WebHostDefaults.PreferHostingUrlsKey);

					foreach (string value in urls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
						addresses.Add(value);
					}
				}
			}

			return addresses;
		}

		// David Fouler would really like us not to do this
		// https://github.com/aspnet/KestrelHttpServer/issues/2306#issuecomment-364478486
		public EndpointDetailsViewModel HackEndpointDetails()
		{
			var model = new EndpointDetailsViewModel();
			try {
				KestrelServer kestrel = server as KestrelServer;
				if (kestrel == null) {
					model.NotKestrel = true;
					return model;
				}

				KestrelServerOptions options = kestrel.Options;
				// reflect out the ListenOptions array
				Type kestrelServerOptionsType = typeof(KestrelServerOptions);
				PropertyInfo listenOptionsProp = kestrelServerOptionsType.GetProperty("ListenOptions", BindingFlags.Instance | BindingFlags.NonPublic);
				List<ListenOptions> listenOptions = (List<ListenOptions>)listenOptionsProp.GetValue(options);

				foreach (ListenOptions listenOption in listenOptions) {
					if (listenOption.ConnectionAdapters?.Count > 0) {
						foreach (IConnectionAdapter connectionAdapter in listenOption.ConnectionAdapters) {

							// Grab all the details for this endpoint
							EndpointDetail endpointDetail = new EndpointDetail {
								Address = listenOption.IPEndPoint.Address.ToString(),
								Port = listenOption.IPEndPoint.Port,
								IsHttps = connectionAdapter.IsHttps
							};
							if (connectionAdapter is HttpsConnectionAdapter) {
								endpointDetail.Certificate = typeof(HttpsConnectionAdapter).GetField("_serverCertificate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(connectionAdapter) as X509Certificate2;
							}

							model.EndpointDetails.Add(endpointDetail);
						}
					} else {
						model.EndpointDetails.Add(new EndpointDetail {
							Address = listenOption.IPEndPoint.Address.ToString(),
							Port = listenOption.IPEndPoint.Port,
							IsHttps = false
						});
					}
				}

				// Reflect the dev cert
				model.IsDevCertLoaded = (bool)(kestrelServerOptionsType.GetProperty("IsDevCertLoaded", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(options));
				model.DefaultCertificate = kestrelServerOptionsType.GetProperty("DefaultCertificate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(options) as X509Certificate2;

			} catch (Exception ex) {
				// because this is hacky enough that it'll likely fall down easily
				model.Exception = ex.Message;
			}
			return model;
		}

	}
}
