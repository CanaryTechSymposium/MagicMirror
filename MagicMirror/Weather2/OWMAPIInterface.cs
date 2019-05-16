using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace MagicMirror.Weather2
{
    public static class OWMAPIInterface
    {
        private static readonly string CITY_ID = "5200055";

        private static readonly string UNITS = "imperial";

        public static CurrentWeather GetCurrentWeather()
        {
            CurrentWeather cWeather = GetWeather("weather?").ReadAsAsync<CurrentWeather>().Result;

            if (cWeather.weather == null)
                throw new APIRequestFailedException("API call did not return the correct object");
            return cWeather;
        }

        public static ForecastWeather GetForcastWeather()
        {
            ForecastWeather fWeather = GetWeather("forecast?").ReadAsAsync<ForecastWeather>().Result;

            if (fWeather.list == null)
                throw new APIRequestFailedException("API call did not return the correct object");
            return fWeather;
        }

        private static HttpContent GetWeather(string requestType)
        {
            try
            {
                string appID = MagicMirror.Utilities.CredintialStore.GetCredintials("newWeather").ID;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");

                return client.GetAsync(requestType + $"id={CITY_ID}&units={UNITS}&APPID={appID}").Result.Content;
            }
            catch (Exception ex)
            {
                throw new APIRequestFailedException("Can't access weather API", ex);
            }
        }
    }

    public class APIRequestFailedException : Exception
    {
        public APIRequestFailedException(string message) : base(message) { }
        public APIRequestFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}