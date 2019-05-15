using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MagicMirror.Weather2
{
    public sealed partial class Weather2 : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly byte TEMPERATURE_ROUNDING_DIGITS = 0;
        private readonly TimeSpan WEATHER_UPDATE_INTERVAL = TimeSpan.FromMinutes(10);
        private readonly TimeSpan FORECAST_UPDATE_INTERVAL = TimeSpan.FromHours(3);

        private DispatcherTimer _timer;

        private int _currentDay;
        private DisplayRotation _currentRotation;
        private CurrentWeather _currentWeather;
        private DateTime _nextWeatherUpdateTime;
        private DateTime _nextForecastUpdateTime;

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

        public Weather2()
        {
            this.DataContext = this;
            this.InitializeComponent();

            _currentDay = -1;
            _currentRotation = DisplayRotation.Wind;
            WeatherDesc = "Loading...";

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _nextWeatherUpdateTime = DateTime.UtcNow;

            // The forecast API updates every 3 hours starting at midnight UTC
            int hoursUntilUpdate = FORECAST_UPDATE_INTERVAL.Hours - (DateTime.UtcNow.Hour % FORECAST_UPDATE_INTERVAL.Hours);

            // Update is this hour
            if (hoursUntilUpdate == 3)
                hoursUntilUpdate = 0;
            _nextForecastUpdateTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour + hoursUntilUpdate - FORECAST_UPDATE_INTERVAL.Hours, 15, 0);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();

            try
            {
                if (DateTime.UtcNow >= _nextForecastUpdateTime)
                {
                    UpdateForecastData();
                    _nextForecastUpdateTime = _nextForecastUpdateTime.Add(FORECAST_UPDATE_INTERVAL);
                }

                if (DateTime.UtcNow >= _nextWeatherUpdateTime)
                {
                    UpdateCurrentData();
                    _nextWeatherUpdateTime = _nextWeatherUpdateTime.Add(WEATHER_UPDATE_INTERVAL);
                }

                UpdateRotatingDisplay(_currentWeather);
            }
            catch (Exception ex)
            {
                WeatherDesc = $"Error reading weather data: {ex.Message}";
                WeatherIcon = null;
            }

            UpdateBindingProperties();

            _timer.Start();
        }

        private void UpdateCurrentData()
        {
            _currentWeather = OWMAPIInterface.GetCurrentWeather();

            Temperature = _currentWeather.main.temp;
        }

        private void UpdateForecastData()
        {
            ForecastWeather weatherData = OWMAPIInterface.GetForcastWeather();

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
            if (_currentDay != DateTime.Now.Day)
            {
                TemperatureLow = double.MaxValue;
                TemperatureHigh = double.MinValue;

                _currentDay = DateTime.Now.Day;
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
            switch (_currentRotation)
            {
                case DisplayRotation.Clouds:
                    RotatingDisplay = $"{weatherData.clouds.all}% cloudy";
                    _currentRotation = DisplayRotation.Humidity;
                    break;
                case DisplayRotation.Humidity:
                    RotatingDisplay = $"{weatherData.main.humidity}% humidity";
                    _currentRotation = DisplayRotation.Wind;
                    break;
                case DisplayRotation.Wind:
                    RotatingDisplay = $"{weatherData.wind.speed} mph wind";
                    _currentRotation = DisplayRotation.Clouds;
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
