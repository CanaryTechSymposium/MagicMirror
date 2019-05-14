using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace MagicMirror.NewWeather
{
    public static class OpenWeatherMapAPIInterface
    {
        private static readonly string CITY_ID = "5200055";

        private static readonly string UNITS = "imperial";

        public static CurrentWeather GetCurrentWeather()
        {
            return GetWeather("weather?").ReadAsAsync<CurrentWeather>().Result;
        }

        public static ForecastWeather GetForcastWeather()
        {
            return GetWeather("forecast?").ReadAsAsync<ForecastWeather>().Result;
        }

        private static HttpContent GetWeather(string requestType)
        {
            string appID = MagicMirror.Utilities.CredintialStore.GetCredintials("newWeather").ID;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");

            return client.GetAsync(requestType + $"id={CITY_ID}&units={UNITS}&APPID={appID}").Result.Content;
        }
    }
}
