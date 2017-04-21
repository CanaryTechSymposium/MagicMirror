using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MagicMirror.Weather
{
    public enum ContentState { Undefined, Weather, Temp, FeelsLike, Humidity, Wind, Precip, Visibility }


    public class WeatherWidgetViewModel : INotifyPropertyChanged
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
            timer = new Timer(CycleNextDisplayText, null, 1000, 7000);

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
                IsDataAvailable = (weathercondition != null);
            }
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
            
            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
                BitmapImage btm = new BitmapImage(new Uri(viewmodel.WeatherConditions.icon_url, UriKind.Absolute));
                Image img = new Image();
                img.Source = btm;
                img.Stretch = Stretch.Uniform;
                viewmodel.StatusImage = img;
            //});
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
