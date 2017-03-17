using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System;

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

        string WebRequest(string method, string url, string postData, Dictionary<string, string> headers)
        {

            HttpWebRequest Request = null;
            StreamWriter requestWriter = null;

            Request = (HttpWebRequest)HttpWebRequest.Create(url);
            Request.Method = method.ToString();

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> h in headers)
                {
                    Request.Headers.Add(h.Key, h.Value);
                }
            }


            if (method == "POST" || method == "DELETE")
            {
                Request.ContentType = "application/x-www-form-urlencoded";

                // POST the data.
                requestWriter = new StreamWriter(Request.GetRequestStream());
                try
                {
                    requestWriter.Write(postData);
                }
                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }
            }

            return (new StreamReader(Request.GetResponse().GetResponseStream())).ReadToEnd();
        }

        public string GetTrumpsFeed()
        {
            return WebRequest("GET", "https://api.twitter.com/1.1/search/tweets.json?q=@realDonaldTrump", "", new Dictionary<string, string> { { "Authorization", "Bearer " + GetBearerToken().access_token } });
        }

        string GetAccessToken()
        {
            var enc = System.Text.Encoding.UTF8;
            return Convert.ToBase64String(enc.GetBytes(WebUtility.UrlEncode(ConsumerKey) + ":" + WebUtility.UrlEncode(ConsumerSecret)));
        }

        TokenBearerResponse GetBearerToken()
        {
            var res = WebRequest("POST", "https://api.twitter.com/oauth2/token", "grant_type=client_credentials", new Dictionary<string, string> { { "Authorization", "Basic " + GetAccessToken() } });
            return JsonConvert.DeserializeObject<TokenBearerResponse>(res);
        }

    }
}
