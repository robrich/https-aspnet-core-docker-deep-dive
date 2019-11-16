using System.Collections.Generic;
using System.Net.Security;

namespace HTTPSPlayground.Web.Models
{
    public class ApiResultModel
    {
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public List<string> Response { get; set; }
        public string CertThumbprint { get; set; }
        public string CertSubject { get; set; }
        public SslPolicyErrors CertError { get; set; } = SslPolicyErrors.None;
        public string Exception { get; set; }
    }
}
