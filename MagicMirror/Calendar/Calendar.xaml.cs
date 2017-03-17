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
            var day = new Day();
            day.DayName = "Today";
            var ev = new Event();
            ev.Description = "Tech Symposium";
            ev.Start = new DateTime(2017, 3, 17, 11, 0, 0);
            ev.End = ev.Start.AddHours(2);
            day.Events.Add(ev);
            ev = new Event();
            ev.Description = "Clean up";
            ev.Start = new DateTime(2017, 3, 17, 14, 0, 0);
            ev.End = ev.Start.AddMinutes(30);
            day.Events.Add(ev);
            Days.Add(day);

            day = new Day();
            day.DayName = "Tomorrow";
            ev = new Event();
            ev.Description = "Part at Gary's";
            ev.Start = new DateTime(2017, 3, 18, 18, 0, 0);
            ev.End = ev.Start.AddHours(3);
            day.Events.Add(ev);
            Days.Add(day);

            day = new Day();
            day.DayName = "March 20, 2017";
            ev = new Event();
            ev.Description = "Finish potatoe chips";
            ev.Start = new DateTime(2017, 3, 20, 12, 0, 0);
            ev.End = ev.Start.AddHours(1);
            day.Events.Add(ev);
            Days.Add(day);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Day
    {
        public string DayName { get; set; }
        public List<Event> Events { get; set; }

        public Day()
        {
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
