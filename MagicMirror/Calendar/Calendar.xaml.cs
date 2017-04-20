using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MagicMirror.Calendar
{
    public partial class Calendar : UserControl, INotifyPropertyChanged
    {
        private ICalendarEventInterface _calendarProvider;

        private DispatcherTimer _updateTimer;
        private TimeSpan _updateRate;

        public ObservableCollection<Day> Days { get; set; }
        public Calendar()
        {
            this.InitializeComponent();
            this.DataContext = this;

            Days = new ObservableCollection<Day>();

            _calendarProvider = new ExchangeCalendarProvider();

            _updateTimer = null;
            _updateRate = new TimeSpan(0, 0, 15);

            StartTimer();
        }

        public void StartTimer()
        {
            if (_updateTimer != null)
                return;

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _updateTimer.Tick += TimerTick;
            _updateTimer.Start();
        }

        public void StopTimer()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer = null;
            }
        }

        private void TimerTick(object sender, object e)
        {
            _updateTimer.Stop();

            CheckForChanges();

            // restart _updateTimer if it hasn't been stopped
            if (_updateTimer != null)
            {
                _updateTimer.Interval = _updateRate;
                _updateTimer.Start();
            }
        }

        private void CheckForChanges()
        {
            var events = _calendarProvider.GetCurrentEvents();

            Days.Clear();

            foreach (var item in events.OrderBy(p=>p.Start))
            {
                AddNextEvent(item.Description, item.Start, item.End);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddNextEvent(string description, DateTime start, DateTime end)
        {
            // Is this event the same day as previous event or a new day?
            var day = Days.LastOrDefault();
            if (day == null || day.Date != start.Date)
            {
                day = new Day(start);
                Days.Add(day);
            }

            var ev = new Event();
            ev.Description = description;
            ev.Start = start;
            ev.End = end;
            day.Events.Add(ev);
        }
    }

    public class Day
    {
        public DateTime Date { get; set; }
        public string DayName
        {
            get
            {
                if (Date == DateTime.Now.Date)
                    return "Today";
                else if (Date == DateTime.Now.Date.AddDays(1))
                    return "Tomorrow";
                else
                    return Date.ToString("MMMM d, yyyy");
            }
        }
        public List<Event> Events { get; set; }

        public Day(DateTime date)
        {
            Date = date.Date;
            Events = new List<Event>();
        }
    }

    public class Event
    {
        public DateTime Start { get; set; }
        public string StartDisplay
        {
            get
            {
                return Start.ToString("t");
            }
        }
        public DateTime End { get; set; }
        public string Description { get; set; }
    }
}
