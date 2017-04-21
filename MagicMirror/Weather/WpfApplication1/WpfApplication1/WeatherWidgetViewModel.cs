using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MagicMirror.Weather
{
   public enum ContentState { Undefined, Weather, Temp, FeelsLike, Humidity, Wind, Precip, Visibility}


    public class WeatherWidgetViewModel: INotifyPropertyChanged
    {
        private Timer timer;
        private HttpClientHandler handler;
        private ContentState contentstate = ContentState.Undefined;
        public WeatherWidgetViewModel()
        {
            ApiKey = "28ab95f87f611899";
            BaseAddress = new Uri("http://api.wunderground.com/");
            BaseUriFormatString = "api/{0}/conditions/q/{1}/{2}.json";
            StateAbbrev = "pa";
            City = "martinsburg";
            isDataAvailable = false;

            ExecuteGetWeatherCommand = new ExecuteCommand(this);
            timer = new Timer(CycleNextDisplayText, null, 1000, 7000 );

            handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
        }

        #region Required Input Values
        private string stateAbbrev;
        public string StateAbbrev
        {
            get { return stateAbbrev; }
            set
            {
                stateAbbrev = value;
                RaisePropertyChanged("StateAbbrev");
            }
        }

        private string city;
        public string City
        {
            get { return city; }
            set
            {
                city = value;
                RaisePropertyChanged("City");
            }
        }

        private string apiKey;
        public string ApiKey
        {
            get { return apiKey; }
            set
            {
                apiKey = value;
                RaisePropertyChanged("ApiKey");
            }
        }

        private Uri baseaddress;
        public Uri BaseAddress
        {
            get { return baseaddress; }
            set
            {
                baseaddress = value;
                RaisePropertyChanged("BaseAddress");
            }
        }

        private string baseUriFormatString;
        public string BaseUriFormatString
        {
            get { return baseUriFormatString; }
            set
            {
                baseUriFormatString = value;
                RaisePropertyChanged("BaseUriFormatString");
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
                //var obj = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
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
            set { lastUpdated = value; RaisePropertyChanged("LastUpdated"); }
        }

        private string location;
        public string Location
        {
            get { return location; }
            set { location = value; RaisePropertyChanged("Location"); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged("Status"); }
        }

        private Image statusimage;
        public Image StatusImage
        {
            get { return statusimage; }
            set { statusimage = value; RaisePropertyChanged("StatusImage"); }
        }

        private string temperature;
        public string Temperature
        {
            get { return temperature; }
            set { temperature = value; RaisePropertyChanged("Temperature"); }
        }

        private bool isDataAvailable;
        public bool IsDataAvailable
        {
            get { return isDataAvailable; }
            set { isDataAvailable = value; RaisePropertyChanged("IsDataAvailable"); }
        }

        #endregion

        private WeatherCondition weathercondition;
        public WeatherCondition WeatherConditions
        {
            get { return weathercondition; }
            set
            {
                weathercondition = value;
                RaisePropertyChanged("WeatherConditions");
                IsDataAvailable = (weathercondition != null);  }
        }

        private string lastDisplayProperty { get; set; }
        public void CycleNextDisplayText(object state)
        {
            if (weathercondition == null)
            {
                RollingContent = "";
                ExecuteGetWeatherCommand.Execute(null);
            }
            else
            {
                ContentState nextstate = this.contentstate.Next();
                switch (nextstate)
                {
                    case ContentState.FeelsLike:
                        RollingContent = String.Format("Feels Like {0}.", weathercondition.feelslike_string);
                        break;
                    case ContentState.Temp:
                        RollingContent = String.Format("Temperature {0}.", weathercondition.temperature_string);
                        break;
                    case ContentState.Weather:
                        RollingContent = weathercondition.weather;
                        break;
                    case ContentState.Wind:
                        RollingContent = String.Format("Wind {0} {1} from the {2}", weathercondition.wind_string, weathercondition.wind_mph, weathercondition.wind_dir);
                        break;
                    case ContentState.Precip:
                        RollingContent = String.Format("Precipitation {0}", weathercondition.precip_today_in);
                        break;
                    case ContentState.Humidity:
                        RollingContent = String.Format("Humidity {0}", weathercondition.relative_humidity);
                        break;
                    case ContentState.Visibility:
                        RollingContent = String.Format("Humidity {0}", weathercondition.visibility_mi);
                        break;
                    default:
                        rollingContent = "";
                        break;
                }
                contentstate = nextstate;
            }
        }


        /// <summary>
        /// Command object for executing GetWeather method on the viewmodel
        /// </summary>
        public ICommand ExecuteGetWeatherCommand { get; set; }

        private object rollingContent;
        public object RollingContent
        {
            get { return rollingContent; }
            set { rollingContent = value; RaisePropertyChanged("RollingContent"); }
        }


        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(String propertyName)
        {

            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        
    }

    public static class ContentStateEnumExtension
    {
        public static ContentState Next(this ContentState myEnum)
        {
            switch (myEnum)
            {
                case ContentState.Undefined:
                    return ContentState.Weather;
                case ContentState.Weather:
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
                    return ContentState.Weather;
            }
        }
    }


    public class ResponseHeader
    {
        public ResponseHeader()
        {
            features = new Features();
        }

        public string version { get; set; }
        public string termsofService { get; set; }
        
        public Features features { get; set; }
    }

    public class Features
    {
        public string conditions { get; set; }
    }

    public class ObservationResponse
    {
        public ObservationResponse()
        {
            response = new ResponseHeader();
            current_observation = new WeatherCondition();
        }

        public ResponseHeader response { get; set; }
        public WeatherCondition current_observation { get; set; }
    }

    public class WUImage
    {
        public string url { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(title))
                return title;
            else
                return base.ToString();
        }
    }

    public class DisplayLocation
    {
        public string full { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string state_name { get; set; }
        public string country { get; set; }
        public string country_iso3166 { get; set; }
        public string zip { get; set; }
        public string magic { get; set; }
        public string wmo { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string elevation { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(full))
                return full;
            else
                return base.ToString();
        }
    }

    public class ObservationLocation
    {
        public string full { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string country_iso3166 { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string elevation { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(full))
                return full;
            else
                return base.ToString();
        }
    }

    public class Estimated
    {

    }

    public class WeatherCondition
    {
        public WeatherCondition()
        {
            image = new WUImage();
            display_location = new DisplayLocation();
            observation_location = new ObservationLocation();
            estimated = new Estimated();
        }
        public WUImage image { get; set; }
        public DisplayLocation display_location { get; set; }
        public ObservationLocation observation_location { get; set; }
        public Estimated estimated { get; set; }
        public string station_id { get; set; }
        public string observation_time { get; set; }
        public string observation_time_rfc822 { get; set; }
        public string observation_epoch { get; set; }
        public string local_time_rfc822 { get; set; }
        public string local_epoch { get; set; }
        public string local_tz_short { get; set; }
        public string local_tz_long { get; set; }
        public string local_tz_offset { get; set; }
        public string weather { get; set; }
        public string temperature_string { get; set; }
        public string temp_f { get; set; }
        public string temp_c { get; set; }
        public string relative_humidity { get; set; }
        public string wind_string { get; set; }
        public string wind_dir { get; set; }
        public string wind_degrees { get; set; }
        public string wind_mph { get; set; }
        public string wind_gust_mph { get; set; }
        public string wind_kph { get; set; }
        public string wind_gust_kph { get; set; }
        public string pressure_mb { get; set; }
        public string pressure_in { get; set; }
        public string pressure_trend { get; set; }
        public string dewpoint_string { get; set; }
        public string dewpoint_f { get; set; }
        public string dewpoint_c { get; set; }
        public string heat_index_string { get; set; }
        public string heat_index_f { get; set; }
        public string heat_index_c { get; set; }
        public string windchill_string { get; set; }
        public string windchill_f { get; set; }
        public string windchill_c { get; set; }
        public string feelslike_string { get; set; }
        public string feelslike_f { get; set; }
        public string feelslike_c { get; set; }
        public string visibility_mi { get; set; }
        public string visibility_km { get; set; }
        public string solarradiation { get; set; }
        public string UV { get; set; }
        public string precip_1hr_string { get; set; }
        public string precip_1hr_in { get; set; }
        public string precip_1hr_metric { get; set; }
        public string precip_today_string { get; set; }
        public string precip_today_in { get; set; }
        public string precip_today_metric { get; set; }
        public string icon { get; set; }
        public string icon_url { get; set; }
        public string forecast_url { get; set; }
        public string history_url { get; set; }
        public string ob_url { get; set; }

        public string nowcast { get; set; }
    }

    public class ExecuteCommand : ICommand
    {
        WeatherWidgetViewModel viewmodel;

        public ExecuteCommand(WeatherWidgetViewModel viewmodel)
        {
            this.viewmodel = viewmodel;
        }

        public bool CanExecute(object parameter)
        {
            return !(String.IsNullOrEmpty(viewmodel.ApiKey) && String.IsNullOrEmpty(viewmodel.BaseUriFormatString) &&
                String.IsNullOrEmpty(viewmodel.City) && String.IsNullOrEmpty(viewmodel.StateAbbrev) && viewmodel.BaseAddress == null);
        }

        public async void Execute(object parameter)
        {
            viewmodel.WeatherConditions = await viewmodel.GetWeatherCondition();
            viewmodel.LastUpdated = viewmodel.WeatherConditions.observation_time;
            viewmodel.Location = viewmodel.WeatherConditions.display_location.full;
            viewmodel.Status = viewmodel.WeatherConditions.icon;
            viewmodel.Temperature = Math.Round(Convert.ToDecimal(viewmodel.WeatherConditions.temp_f)).ToString();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                BitmapImage btm = new BitmapImage(new Uri(viewmodel.WeatherConditions.icon_url, UriKind.Absolute));
                Image img = new Image();
                img.Source = btm;
                img.Stretch = Stretch.Uniform;
                viewmodel.StatusImage = img;
            });
            viewmodel.CycleNextDisplayText(null);

        }

        public event EventHandler CanExecuteChanged;
        protected void RaiseCanExecuteChanged()
        {
            //C# 6.0, otherwise assign the handler to variable and do null check
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }


    }
}
