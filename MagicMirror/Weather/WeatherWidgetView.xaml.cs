using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MagicMirror.Weather
{
    public sealed partial class WeatherWidgetView : UserControl, INotifyPropertyChanged
    {
        public WeatherWidgetView()
        {
            this.InitializeComponent();
            this.DataContext = this;

            //ViewModel = new WeatherWidgetViewModel();
            var weatherCredintials = MagicMirror.Utilities.CredintialStore.GetCredintials("weather");
            ApiKey = weatherCredintials.ID;
            BaseAddress = new Uri("http://api.wunderground.com/");
            BaseUriFormatString = "api/{0}/conditions/q/{1}/{2}.json";
            StateAbbrev = "pa";
            City = "martinsburg";
            isDataAvailable = false;

            handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            this.Loaded += WeatherWidgetView_Loaded;
        }

        private void WeatherWidgetView_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Tick += CycleNextDisplayText1;
            timer.Interval = new TimeSpan(1);
            timer.Start();
        }

        private DispatcherTimer timer;
        private HttpClientHandler handler;
        private ContentState contentstate = ContentState.Undefined;
        private DateTime nextWeatherUpdateTime = DateTime.MinValue;
        private TimeSpan nextWeatherUpdateInterval = new TimeSpan(0, 5, 0);

        private void CycleNextDisplayText1(object sender, object e)
        {
            if (DateTime.Now > nextWeatherUpdateTime)
            {
                RollingContent = "";
                timer.Stop();
                GetWeather();
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Start();

                nextWeatherUpdateTime = DateTime.Now + nextWeatherUpdateInterval;
            }

            if (weathercondition == null || weathercondition.temperature_string == null)
                return;

            ContentState nextstate = this.contentstate;
            switch (nextstate)
            {
                case ContentState.FeelsLike:
                    RollingContent = String.Format("Feels like {0}", weathercondition.feelslike_string);
                    break;
                case ContentState.Temp:
                    RollingContent = String.Format("Temperature: {0}", weathercondition.temperature_string);
                    break;
                case ContentState.Pressure:
                    RollingContent = string.Format("Pressure: {0} in", weathercondition.pressure_in);
                    break;
                case ContentState.Wind:
                    RollingContent = String.Format("Wind {0}{1}", weathercondition.wind_string[0].ToString().ToLower(), weathercondition.wind_string.Substring(1));
                    break;
                case ContentState.Precip:
                    RollingContent = String.Format("Precipitation: {0} in", weathercondition.precip_today_in);
                    break;
                case ContentState.Humidity:
                    RollingContent = String.Format("Humidity: {0}", weathercondition.relative_humidity);
                    break;
                case ContentState.Visibility:
                    RollingContent = String.Format("Visibility: {0} mi", weathercondition.visibility_mi);
                    break;
                default:
                    rollingContent = "";
                    break;
            }
            contentstate = nextstate.Next();
        }

        public async void GetWeather()
        {
            try
            {
                WeatherConditions = await GetWeatherCondition();
                LastUpdated = string.Format("Last udpate: {0}", DateTime.Parse(WeatherConditions.observation_time_rfc822));
                Location = WeatherConditions.display_location.full;
                Status = WeatherConditions.weather;
                Temperature = Math.Round(Convert.ToDecimal(WeatherConditions.temp_f)).ToString();

                BitmapImage btm = new BitmapImage(new Uri($"ms-appx:///Assets/{MapWeatherUndergroundIconNames(WeatherConditions.icon)}"));
                //BitmapImage btm = new BitmapImage(new Uri(string.Format("https://icons.wxug.com/i/c/k/{0}.gif", WeatherConditions.icon), UriKind.Absolute));
                StatusImage = btm;
            }
            catch
            {
                LastUpdated = DateTime.Now.ToString();
                Status = "Error reading weather data";
            }

        }

        #region Required Input Values
        private string stateAbbrev;
        public string StateAbbrev
        {
            get { return stateAbbrev; }
            set
            {
                stateAbbrev = value;
            }
        }

        private string city;
        public string City
        {
            get { return city; }
            set
            {
                city = value;
            }
        }

        private string apiKey;
        public string ApiKey
        {
            get { return apiKey; }
            set
            {
                apiKey = value;
            }
        }

        private Uri baseaddress;
        public Uri BaseAddress
        {
            get { return baseaddress; }
            set
            {
                baseaddress = value;
            }
        }

        private string baseUriFormatString;
        public string BaseUriFormatString
        {
            get { return baseUriFormatString; }
            set
            {
                baseUriFormatString = value;
            }
        }

        #endregion

        public async Task<WeatherCondition> GetWeatherCondition()
        {
            WeatherCondition condition = null;
            string uri = string.Format(BaseUriFormatString, ApiKey, StateAbbrev, City);
            var client = new HttpClient(handler, true);
            client.BaseAddress = BaseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                ObservationResponse obj = await response.Content.ReadAsAsync<ObservationResponse>();
                condition = obj.current_observation;
            }
            return condition;
        }

        #region Output for View Display
        private string lastUpdated;
        public string LastUpdated
        {
            get { return lastUpdated; }
            set { lastUpdated = value; OnPropertyChanged(() => LastUpdated); }
        }

        private string location;
        public string Location
        {
            get { return location; }
            set { location = value; OnPropertyChanged(() => Location); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged(() => Status); }
        }

        private ImageSource statusimage;
        public ImageSource StatusImage
        {
            get { return statusimage; }
            set { statusimage = value; OnPropertyChanged(() => StatusImage); }
        }

        private string temperature;
        public string Temperature
        {
            get { return temperature; }
            set { temperature = value; OnPropertyChanged(() => Temperature); }
        }

        private bool isDataAvailable;
        public bool IsDataAvailable
        {
            get { return isDataAvailable; }
            set { isDataAvailable = value; OnPropertyChanged(() => IsDataAvailable); }
        }

        #endregion

        private WeatherCondition weathercondition;
        public WeatherCondition WeatherConditions
        {
            get { return weathercondition; }
            set
            {
                weathercondition = value;
                OnPropertyChanged(() => WeatherConditions);
                IsDataAvailable = (weathercondition != null);
            }
        }

        private string lastDisplayProperty { get; set; }

        private object rollingContent;
        public object RollingContent
        {
            get { return rollingContent; }
            set { rollingContent = value; OnPropertyChanged(() => RollingContent); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(((MemberExpression)exp.Body).Member.Name));
        }

        private string MapWeatherUndergroundIconNames(string icon)
        {
            switch (icon.ToLower())
            {
                case "sunny":
                case "clear":
                    return "Sunny.png";
                case "cloudy":
                    return "Cloudy.png";
                case "flurries":
                    return "Flurries.png";
                case "hazy":
                    return "";
                case "partlycloudy":
                case "mostlycloudy":
                case "partlysunny":
                    return "Mostly_Cloudy.png";
                case "mostlysunny":
                    return "Mostly_Sunny.png";
                case "rain":
                    return "Rain.png";
                case "sleet":
                    return "Freezing_Rain.png";
                case "snow":
                    return "Snow.png";
                case "thunderstorm":
                    return "Thunderstorms.png";
                default:
                    return string.Empty;
            }
        }
    }

    public enum ContentState { Undefined, Pressure, Temp, FeelsLike, Humidity, Wind, Precip, Visibility }

    public static class ContentStateEnumExtension
    {
        public static ContentState Next(this ContentState myEnum)
        {
            switch (myEnum)
            {
                case ContentState.Undefined:
                    return ContentState.Pressure;
                case ContentState.Pressure:
                    return ContentState.Temp;
                case ContentState.Temp:
                    return ContentState.FeelsLike;
                case ContentState.FeelsLike:
                    return ContentState.Humidity;
                case ContentState.Humidity:
                    return ContentState.Wind;
                case ContentState.Wind:
                    return ContentState.Precip;
                case ContentState.Precip:
                    return ContentState.Visibility;
                default:
                    return ContentState.Pressure;
            }
        }
    }
}
