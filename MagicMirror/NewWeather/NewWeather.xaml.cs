using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MagicMirror.NewWeather
{
    public sealed partial class NewWeather : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly byte TEMPERATURE_ROUNDING_DIGITS = 0;
        private readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromHours(3);

        private DispatcherTimer timer;

        private int currentDay;
        private DisplayRotation currentRotation;
        private CurrentWeather currentWeather;
        private DateTime nextUpdateTime;

        private double _temperature;
        private double _temperatureLow;
        private double _temperatureHigh;

        public double Temperature
        {
            get { return _temperature; }
            set { _temperature = Math.Round(value, TEMPERATURE_ROUNDING_DIGITS); }
        }
        public double TemperatureLow
        {
            get { return _temperatureLow; }
            set { _temperatureLow = Math.Round(value, TEMPERATURE_ROUNDING_DIGITS); }
        }
        public double TemperatureHigh
        {
            get { return _temperatureHigh; }
            set { _temperatureHigh = Math.Round(value, TEMPERATURE_ROUNDING_DIGITS); }
        }
        public string WeatherDesc { get; set; }
        public ImageSource WeatherIcon { get; set; }
        public string RotatingDisplay { get; set; }

        public NewWeather()
        {
            this.DataContext = this;
            this.InitializeComponent();

            currentDay = -1;
            currentRotation = DisplayRotation.Wind;
            WeatherDesc = "Loading...";

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            // The API updates every 3 hours starting at 2am every morning (midnight UTC)
            int hoursUntilUpdate = UPDATE_INTERVAL.Hours - ((DateTime.Now.Hour - 2) % UPDATE_INTERVAL.Hours);

            // Update is this hour
            if (hoursUntilUpdate == 3)
                hoursUntilUpdate = 0;
            nextUpdateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + hoursUntilUpdate - 3, 15, 0);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();

            try
            {
                if (DateTime.Now >= nextUpdateTime)
                {
                    UpdateWeatherData();
                    nextUpdateTime = nextUpdateTime.Add(UPDATE_INTERVAL);
                }

                UpdateRotatingDisplay(currentWeather);
            }
            catch
            {
                WeatherDesc = "Error reading weather data";
                WeatherIcon = null;
            }

            UpdateBindingProperties();

            timer.Start();
        }

        private void UpdateWeatherData()
        {
            ForecastWeather weatherData = OpenWeatherMapAPIInterface.GetForcastWeather();
            currentWeather = weatherData.list[0];

            Temperature = weatherData.list[0].main.temp;

            UpdateWeatherDescription(weatherData);

            UpdateHighLowTemperatures(weatherData);
        }

        private void UpdateWeatherDescription(ForecastWeather weatherData)
        {
            string iconId;

            if (weatherData.list[0].weather[0].id == weatherData.list[2].weather[0].id)
            {
                WeatherDesc = weatherData.list[0].weather[0].description;
                iconId = weatherData.list[0].weather[0].icon;
            }
            else
            {
                WeatherDesc = weatherData.list[1].weather[0].description;
                iconId = weatherData.list[1].weather[0].icon;
            }

            BitmapImage btm = new BitmapImage(new Uri($"http://openweathermap.org/img/w/{iconId}.png"));
            WeatherIcon = btm;
        }

        private void UpdateHighLowTemperatures(ForecastWeather weatherData)
        {
            // New day, new high and low temperatures
            if (currentDay != DateTime.Now.Day)
            {
                TemperatureLow = double.MaxValue;
                TemperatureHigh = double.MinValue;

                currentDay = DateTime.Now.Day;
            }

            long tomorrowUnix = new DateTimeOffset(DateTime.Today.AddDays(1)).ToUnixTimeSeconds();

            foreach (var data in weatherData.list)
            {
                // Done when it reaches tomorrow
                if (data.dt >= tomorrowUnix)
                    break;

                TemperatureLow = Math.Min(TemperatureLow, data.main.temp_min);

                TemperatureHigh = Math.Max(TemperatureHigh, data.main.temp_max);
            }
        }

        private void UpdateRotatingDisplay(CurrentWeather weatherData)
        {
            switch (currentRotation)
            {
                case DisplayRotation.Clouds:
                    RotatingDisplay = $"{weatherData.clouds.all}% cloudy";
                    currentRotation = DisplayRotation.Humidity;
                    break;
                case DisplayRotation.Humidity:
                    RotatingDisplay = $"{weatherData.main.humidity}% humidity";
                    currentRotation = DisplayRotation.Wind;
                    break;
                case DisplayRotation.Wind:
                    RotatingDisplay = $"{weatherData.wind.speed} mph wind";
                    currentRotation = DisplayRotation.Clouds;
                    break;
                default:
                    break;
            }
        }

        private void UpdateBindingProperties()
        {
            OnPropertyChanged(() => Temperature);
            OnPropertyChanged(() => TemperatureLow);
            OnPropertyChanged(() => TemperatureHigh);
            OnPropertyChanged(() => WeatherDesc);
            OnPropertyChanged(() => WeatherIcon);
            OnPropertyChanged(() => RotatingDisplay);
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(((MemberExpression)exp.Body).Member.Name));
        }
    }

    public enum DisplayRotation
    {
        Wind,
        Clouds,
        Humidity
    }
}
