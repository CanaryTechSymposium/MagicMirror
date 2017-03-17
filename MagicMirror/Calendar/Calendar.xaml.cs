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
        public ObservableCollection<Day> Days { get; set; }
        public Calendar()
        {
            this.InitializeComponent();
            this.DataContext = this;

            Days = new ObservableCollection<Day>();

            // Test data
            var start = new DateTime(2017, 3, 17, 11, 0, 0);
            AddNextEvent("Tech Symposium", start, start.AddHours(2));
            start = new DateTime(2017, 3, 17, 14, 0, 0);
            AddNextEvent("Clean up", start, start.AddMinutes(30));
            start = new DateTime(2017, 3, 18, 18, 0, 0);
            AddNextEvent("Part at Gary's", start, start.AddHours(3));
            start = new DateTime(2017, 3, 20, 12, 0, 0);
            AddNextEvent("Finish potatoe chips", start, start.AddHours(1));
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
