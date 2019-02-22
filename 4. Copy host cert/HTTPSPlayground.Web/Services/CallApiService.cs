using HTTPSPlayground.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HTTPSPlayground.Web.Services
{
	public interface ICallApiService
	{
		Task<ApiResultModel> MakeCall(string url);
	}
	public class CallApiService : ICallApiService
	{

		public async Task<ApiResultModel> MakeCall(string url)
		{
			ApiResultModel model = new ApiResultModel {
				Url = url
			};

			try {
				HttpClientHandler handler = new HttpClientHandler {
					UseDefaultCredentials = true,
					ServerCertificateCustomValidationCallback = (sender, cert, chain, error) => {
						// cert is disposed as soon as this method is done, so grab everything we want while we have it
						model.CertThumbprint = cert?.Thumbprint;
						model.CertSubject = cert?.Subject;
						model.CertError = error;
						return error == SslPolicyErrors.None; // <-- fail request on cert error
					}
				};
				HttpClient client = new HttpClient(handler);
				HttpResponseMessage response = await client.GetAsync(url);
				model.StatusCode = (int)response.StatusCode;
				//TODO: in production code: response.EnsureSuccessStatusCode();
				string body = await response.Content.ReadAsStringAsync();
				model.Response = JsonConvert.DeserializeObject<List<string>>(body);
			} catch (/*HttpRequestException*/Exception ex) {
				model.Exception = $"{ex.Message}, {ex.InnerException?.Message}";
			}

			return model;
		}

		public string GetSha2Thumbprint(X509Certificate2 cert)
		{
			byte[] hashBytes;
			using (var hasher = new SHA256Managed()) {
				hashBytes = hasher.ComputeHash(cert.RawData);
			}
			return hashBytes.Aggregate("", (str, hashByte) => str + hashByte.ToString("x2"));
		}

	}
}
