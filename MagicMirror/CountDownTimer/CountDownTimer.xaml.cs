using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

namespace MagicMirror.CountDownTimer
{
    public sealed partial class CountDownTimer : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _updateTimer;

        public CountDownTimer()
        {
            this.DataContext = this;

            this.InitializeComponent();

            var countdownSettings = MagicMirror.Utilities.CredintialStore.GetCredintials("countdowntimer");
            Heading = countdownSettings.ID;
            _endDate = DateTime.Parse(countdownSettings.Secret);

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _updateTimer.Tick += TimerTick;
            _updateTimer.Start();
        }

        private void TimerTick(object sender, object e)
        {
            OnPropertyChanged(() => Duration);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Heading { get; set; }

        private DateTime _endDate;
        public string Duration
        {
            get
            {
                var duration = _endDate - DateTime.Now;
                string format = string.Empty;
                if (duration.Ticks < 0)
                {
                    format += "'-'";
                    duration = TimeSpan.FromTicks(-duration.Ticks);
                }

                if (duration.Days > 0)
                    format += "d' '";
                if (duration.Days > 1)
                    format += "'Days '";
                else
                    format += "'Day '";

                format += "h':'mm':'ss";
                return duration.ToString(format);
            }
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(((MemberExpression)exp.Body).Member.Name));
        }
    }
}
