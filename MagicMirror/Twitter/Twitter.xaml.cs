using MagicMirror.ThirdParty;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MagicMirror.Twitter
{
    public sealed partial class Twitter : UserControl
    {
        public ObservableCollection<Tweet> Tweets { get; set; }
        DispatcherTimer timer = new DispatcherTimer();

        public Twitter()
        {
            this.InitializeComponent();
            this.DataContext = this;

            Tweets = new ObservableCollection<Tweet>();

            timer.Interval = new TimeSpan(1);
            timer.Tick += Timer_Tick;
            this.Loaded += Twitter_Loaded;
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();

            Tweets.Clear();

            var twitterCredintials = MagicMirror.Utilities.CredintialStore.GetCredintials("twitter");
            TwitterAPI api = new TwitterAPI(twitterCredintials.ID, twitterCredintials.Secret);
            foreach (Tweet t in api.GetTrumpsFeed())
            {
                t.text = System.Net.WebUtility.HtmlDecode(t.text);
                t.created_at = DateTime.ParseExact(t.created_at, "ddd MMM dd HH:mm:ss +ffff yyyy", System.Globalization.CultureInfo.CurrentCulture).ToLocalTime().ToString();
                Tweets.Add(t);
            }

            timer.Interval = new TimeSpan(0, 15, 0);
            timer.Start();
        }

        private void Twitter_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }
    }
}
