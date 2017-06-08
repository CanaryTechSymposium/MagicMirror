using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagicMirror.ThirdParty
{
    public class TwitterAPI
    {
        string ConsumerKey = "";
        string ConsumerSecret = "";

        // constructor
        public TwitterAPI(string key, string secret)
        {
            ConsumerKey = key;
            ConsumerSecret = secret;
        }

        public  async Task<List<Tweet>> GetTrumpsFeed()
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetBearerToken().Result.access_token );

                HttpResponseMessage response;

                response = await client.GetAsync("https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=@realDonaldTrump&count=15");

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Tweet>>(json);
            }
        }

        string GetAccessToken()
        {
            var enc = System.Text.Encoding.UTF8;
            return Convert.ToBase64String(enc.GetBytes(WebUtility.UrlEncode(ConsumerKey) + ":" + WebUtility.UrlEncode(ConsumerSecret)));
        }

       public async Task<TokenBearerResponse> GetBearerToken()
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + GetAccessToken());

                Dictionary<string, string> formVars = new Dictionary<string, string>();
                formVars.Add("grant_type", "client_credentials");

                HttpContent content = new FormUrlEncodedContent(formVars);
                HttpResponseMessage response;

                response = await client.PostAsync("https://api.twitter.com/oauth2/token", content);

                return JsonConvert.DeserializeObject<TokenBearerResponse>(await response.Content.ReadAsStringAsync());
            }  
        }

    }
}