using System.Collections.Generic;

namespace MagicMirror.NewWeather
{
    public class ForecastWeather
    {
        public List<CurrentWeather> list;
    }

    public class CurrentWeather
    {
        public long dt;

        public List<WeatherType> weather;

        public WeatherMain main;

        public WeatherClouds clouds;

        public WeatherWind wind;
    }

    public class WeatherMain
    {
        public double temp;

        public double temp_min;

        public double temp_max;

        public double humidity;
    }

    public class WeatherType
    {
        public string id;

        public string description;

        public string icon;
    }

    public class WeatherClouds
    {
        public double all;
    }

    public class WeatherWind
    {
        public double speed;
    }
}
