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
    public partial class Calendar : UserControl, INotifyPropertyChanged, ICalendarEventInterface
    {
        private MagicMirror.Calendar.ExchangeProvider.ExchangeCalendarProvider _calendarProvider;

        public ObservableCollection<Day> Days { get; set; }
        public Calendar()
        {
            this.InitializeComponent();
            this.DataContext = this;

            Days = new ObservableCollection<Day>();

            _calendarProvider = new MagicMirror.Calendar.ExchangeProvider.ExchangeCalendarProvider(this, new TimeSpan(0, 0, 15));
        }

        public void SetCurrentEvents(List<CalendarEvent> events)
        {
            Days.Clear();

            List<Event> expandedEvents = new List<Event>();

            // Expand out multiday events.
            foreach (var item in events)
            {
                for(var date = item.Start; date < item.End; date = date.Date.AddDays(1))
                {
                    var ev = new Event();
                    ev.Description = item.Description;
                    ev.Start = new DateTime(Math.Max(item.Start.Ticks, date.Ticks));
                    ev.End = new DateTime(Math.Min(item.End.Ticks, date.AddDays(1).Ticks));
                    ev.IsAllDay = item.IsAllDay || (ev.Start == ev.Start.Date && ev.End >= ev.Start.Date.AddDays(1));

                    expandedEvents.Add(ev);
                }
            }

            foreach (var item in expandedEvents.OrderBy(p => p.Start))
            {
                if (item.End < DateTime.Now)
                    continue;

                AddNextEvent(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddNextEvent(Event ev)
        {
            // Is this event the same day as previous event or a new day?
            var day = Days.LastOrDefault();
            if (day == null || day.Date != ev.Start.Date)
            {
                day = new Day(ev.Start);
                Days.Add(day);
            }

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
                if (IsAllDay)
                    return "All Day";

                return Start.ToString("t");
            }
        }
        public DateTime End { get; set; }
        public string Description { get; set; }
        public bool IsAllDay { get; set; }
    }
}
