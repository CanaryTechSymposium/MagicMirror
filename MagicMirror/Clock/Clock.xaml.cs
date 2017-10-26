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

namespace MagicMirror.Clock
{
    public sealed partial class Clock : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _updateTimer;

        public Clock()
        {
            this.DataContext = this;

            this.InitializeComponent();

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _updateTimer.Tick += TimerTick;
            _updateTimer.Start();
        }

        private void TimerTick(object sender, object e)
        {
            OnPropertyChanged(() => Time);
            OnPropertyChanged(() => Date);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Time
        {
            get
            {
                return DateTime.Now.ToString("T");
            }
        }

        public string Date
        {
            get
            {
                return DateTime.Now.ToString("ddd, MMMM d");
            }
        }
        public void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(((MemberExpression)exp.Body).Member.Name));
        }
    }
}
