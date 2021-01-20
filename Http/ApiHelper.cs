using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrintNodeNet.Http
{
    internal class PrintNodeApiClient
    {
        private const string BaseUri = "https://api.printnode.com";
        private readonly HttpClient _client;

        private static readonly JsonSerializerSettings DefaultSerializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public PrintNodeApiClient(PrintNodeDelegatedClientContext clientContext = null)
        {
            _client = BuildHttpClient(clientContext);
        }

        internal async Task<string> Get(string relativeUri, PrintNodeRequestOptions options)
        {
            SetAuthenticationHeader(_client, options);

            var result = await _client.GetAsync(BaseUri + relativeUri, CancellationToken.None);

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception(result.StatusCode.ToString());
            }

            return await result.Content.ReadAsStringAsync();
        }

        internal async Task<string> Post<T>(string relativeUri, T parameters, PrintNodeRequestOptions options)
        {
            SetAuthenticationHeader(_client, options);

            var json = JsonConvert.SerializeObject(parameters, DefaultSerializationSettings);

            var response = await _client.PostAsync(BaseUri + relativeUri, new StringContent(json, Encoding.UTF8, "application/json"), CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new PrintNodeException(response);
            }

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> Patch<T>(string relativeUri, T parameters, PrintNodeRequestOptions options, Dictionary<string, string> headers)
        {
            SetAuthenticationHeader(_client, options);

            var json = JsonConvert.SerializeObject(parameters, DefaultSerializationSettings);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), BaseUri + relativeUri) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new PrintNodeException(response);
            }

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> Delete(string relativeUri, PrintNodeRequestOptions options, Dictionary<string, string> headers)
        {
            SetAuthenticationHeader(_client, options);

            var request = new HttpRequestMessage(new HttpMethod("DELETE"), BaseUri + relativeUri);

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new PrintNodeException(response);
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static void SetAuthenticationHeader(HttpClient client, PrintNodeRequestOptions options)
        {
            var apiKey = options?.ApiKey ?? PrintNodeConfiguration.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("PrintNode API key not set! Please go to printnode.com and request an API key, and follow the instructions for configuring PrintNode.Net");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey)));
        }

        private HttpClient BuildHttpClient(PrintNodeDelegatedClientContext clientContext, Dictionary<string, string> headers = null)
        {
            headers = headers ?? new Dictionary<string, string>();
            var http = new HttpClient();
            
            http.DefaultRequestHeaders.Add("Accept-Version", "~3");

            if (clientContext != null)
            {
                var headerName = "";

                switch (clientContext.AuthenticationMode)
                {
                    case PrintNodeDelegatedClientContextAuthenticationMode.Id:
                        headerName = "X-Child-Account-By-Id";
                        break;
                    case PrintNodeDelegatedClientContextAuthenticationMode.Email:
                        headerName = "X-Child-Account-By-Email";
                        break;
                    case PrintNodeDelegatedClientContextAuthenticationMode.CreatorRef:
                        headerName = "X-Child-Account-By-CreatorRef";
                        break;
                }

                http.DefaultRequestHeaders.Add(headerName, clientContext.AuthenticationValue);
            }

            foreach (var kv in headers)
            {
                http.DefaultRequestHeaders.Add(kv.Key, kv.Value);
            }

            return http;
        }
    }
}
