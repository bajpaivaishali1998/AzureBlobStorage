using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Globalization;
using Microsoft.OData.UriParser;

namespace GithubActionIntergrationApp.Token
{
    enum apiVar
    {
        SecClientId,
        SecClientSecret,
        SecOauthuserName,
        SecOauthuserPassword,
        SecExpiryTime,
        access_token,
        refresh_token
    }
    // SecOauthuserName, SecOauthuserPassword, SecClientSecret and SecClientId, SecAccessToken,SecRefreshToken, SecExpiryTime
    public class Formatter : IFormatProvider
    {

        public object GetFormat(Type formatType)
        {
            throw new NotImplementedException();
        }


        public static string ToString(string format, IFormatProvider formatProvider)
        {
            if (String.IsNullOrEmpty(format)) format = "G";


            switch (format.ToUpperInvariant())
            {
                case "G":
                case "C":
                    return "";
                case "F":
                    return "";
                case "K":
                    return "";
                default:
                    throw new FormatException("The format string is not supported.");
            }
        }
        
    }
    public class CheckAccess
    {
        public CheckAccess()
        {

            GetTokenHttpClient(true);
        }
        public System.Uri url
        {
            get { return new Uri(Environment.GetEnvironmentVariable("oauthUrl", EnvironmentVariableTarget.Process)); }
        }
        
        public void GetTokenHttpClient(bool IsFreshCall)
        {
            var baseUri = url;
            string paramStr = "";
            string encodedPair = "";
            
            if (IsFreshCall)
            {
                var clientId = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable(apiVar.SecClientId.ToString(), EnvironmentVariableTarget.Process));
                var clinetSecret = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable(apiVar.SecClientSecret.ToString(), EnvironmentVariableTarget.Process));
                var username = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable(apiVar.SecOauthuserName.ToString(), EnvironmentVariableTarget.Process));
                var password = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable(apiVar.SecOauthuserPassword.ToString(), EnvironmentVariableTarget.Process));
                encodedPair = Base64Encode(clientId + ":" + clinetSecret + ":" + username + ":" + password);
                paramStr = "grant_type=password&client_id=" + clientId + "&client_secret=" + clinetSecret + "&username=" + username + "&password=" + password;
            }
            else
            {
                var clientId = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable(apiVar.SecClientId.ToString(), EnvironmentVariableTarget.Process));
                var clinetSecret = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable(apiVar.SecClientSecret.ToString(), EnvironmentVariableTarget.Process));
                var encodedRefreshToken = Environment.GetEnvironmentVariable(apiVar.refresh_token.ToString(), EnvironmentVariableTarget.Process);
                encodedPair = Base64Encode(string.Format(CultureInfo.CreateSpecificCulture("en-US"), "{0}:{1}:{2}", clientId, clinetSecret, encodedRefreshToken));
                paramStr = "grant_type=refresh_token&client_id=" + clientId + "&client_secret=" + clinetSecret + "&refresh_token=" + encodedRefreshToken;
            }

            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseUri, "oauth_token.do"),
                Content = new StringContent(paramStr)
            };

            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("authorization", "Bearer " + encodedPair);

            using (var client = new HttpClient())
            {
                var bearerResult = client.SendAsync(requestToken);
                if (bearerResult.Result.IsSuccessStatusCode)
                {
                    var bearerData = bearerResult.Result.Content.ReadAsStringAsync();
                    JObject joResponse = JObject.Parse(bearerData.Result);
                    var seconds = Convert.ToDouble(joResponse["expires_in"], CultureInfo.CreateSpecificCulture("en-US"));
                    double expire = Convert.ToDouble(seconds);
                    //Environment.SetEnvironmentVariable("SecAccessToken", joResponse[Convert.ToString( apiVar.SecAccessToken)].ToString());
                    //Environment.SetEnvironmentVariable("SecRefreshToken", joResponse[Convert.ToString(apiVar.SecRefreshToken)].ToString());
                    //Environment.SetEnvironmentVariable("SecExpiryTime", DateTime.UtcNow.AddSeconds(expire).ToString("G", CultureInfo.CreateSpecificCulture("en-US")));


                    Environment.SetEnvironmentVariable("access_token", joResponse[apiVar.access_token.ToString()].ToString());
                    Environment.SetEnvironmentVariable("refresh_token", joResponse[apiVar.refresh_token.ToString()].ToString());
                    Environment.SetEnvironmentVariable("expiry_time", DateTime.UtcNow.AddSeconds(expire).ToString("G", CultureInfo.CreateSpecificCulture("en-US")));
                }
            }
            requestToken.Dispose();

        }
        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public JObject HTTPGetClient(System.Uri baseUri)
        {
            var token = Environment.GetEnvironmentVariable("access_token");
            string paramStr = "Bearer=" + token;

            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = baseUri,
                Content = new StringContent(paramStr)
            };

            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);
            requestToken.Dispose();
            using (var client = new HttpClient())
            {
                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = baseUri,
                };
                requestData.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);

                var results = client.SendAsync(requestData);
                requestData.Dispose();
                if (results.IsCompletedSuccessfully)
                {
                    var data = results.Result.Content.ReadAsStringAsync();
                    JObject Jobj = JObject.Parse(data.Result);
                    return Jobj;
                }
                else
                {
                    Task<string> data = results.Result.Content.ReadAsStringAsync();
                    JObject Jobj = JObject.Parse(data.Result);
                    return Jobj;
                }
            }
        }

        public JObject HTTPPostClient(System.Uri baseUri, string param)
        {
            var token = Environment.GetEnvironmentVariable("access_token");


            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = baseUri,
                Content = new StringContent(param)
            };

            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            using (var client = new HttpClient())
            {
                var results = client.SendAsync(requestToken);

                if (results.IsCompletedSuccessfully)
                {
                    var data = results.Result.Content.ReadAsStringAsync();
                    JObject Jobj = JObject.Parse(data.Result);
                    requestToken.Dispose();
                    return Jobj;
                }
                else
                {
                    Task<string> data = results.Result.Content.ReadAsStringAsync();
                    JObject Jobj = JObject.Parse(data.Result);
                    requestToken.Dispose();
                    return Jobj;
                }

            }
        }
        public JObject HTTPPatchtClient(System.Uri baseUri, string param)
        {
            var token = Environment.GetEnvironmentVariable("access_token");

            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Patch,
                RequestUri = baseUri,
                Content = new StringContent(param)
            };

            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            using (var client = new HttpClient())
            {

                var results = client.SendAsync(requestToken);

                if (results.IsCompletedSuccessfully)
                {
                    var data = results.Result.Content.ReadAsStringAsync();
                    JObject Jobj = JObject.Parse(data.Result);
                    requestToken.Dispose();
                    return Jobj;
                }
                else
                {
                    Task<string> data = results.Result.Content.ReadAsStringAsync();
                    JObject Jobj = JObject.Parse(data.Result);
                    requestToken.Dispose();
                    return Jobj;
                }

            }
        }
       public string ProgressHTTPPatchtClient(System.Uri baseUri, string param)
        {
            var token = Environment.GetEnvironmentVariable("access_token");

            var client = new RestClient( baseUri.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("Authorization", "Bearer "+ token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", param, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            string res = ((RestSharp.RestResponseBase)response).StatusCode.ToString();
            return res;
            
        }

        public string ServiceCafeHTTPPosttClient(System.Uri baseUri, string body)
        {
            var token = Environment.GetEnvironmentVariable("access_token");
            //  var client = new RestClient("https://servicecafedev.service-now.com/api/cmd/hibernation/Hibernation_Status");
            var client = new RestClient(baseUri.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
     
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            string res = ((RestSharp.RestResponseBase)response).StatusCode.ToString();
            return res;
            
        }

        public string ArtifactHTTPPatchtClient(System.Uri baseUri, string param, string RITM, string PlanType)
        {
            var token = Environment.GetEnvironmentVariable("access_token");
            var client = new RestClient(baseUri.ToString());
            client.Timeout = -1;
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("Workflow-Step", PlanType);
            request.AddHeader("RITM", RITM);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", param, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
           
            string res = ((RestSharp.RestResponseBase)response).StatusCode.ToString();
            return res;

        }
 
    }
}
