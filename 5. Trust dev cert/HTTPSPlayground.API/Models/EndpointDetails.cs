using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace HTTPSPlayground.API.Models
{
    public class EndpointDetailsViewModel
    {
        public List<EndpointDetail> EndpointDetails { get; set; } = new List<EndpointDetail>();
        public bool IsDevCertLoaded { get; set; }
        public X509Certificate2 DefaultCertificate { get; set; }
        public bool NotKestrel { get; set; }
        public string Exception { get; set; }
    }
    public class EndpointDetail
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public bool IsHttps { get; set; }
        public X509Certificate2 Certificate { get; set; }
    }
}
