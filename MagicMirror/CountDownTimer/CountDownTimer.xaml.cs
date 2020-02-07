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
            _workdayStart = new TimeSpan(8, 0, 0);
            _workdayEnd = new TimeSpan(17, 0, 0);
            _workdayLength = _workdayEnd - _workdayStart;
            //_vacationAndHolidays = new List<DateTime>()
            //{
            //    new DateTime(2020, 3, 6),
            //};

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _updateTimer.Tick += TimerTick;
            _updateTimer.Start();
        }

        private void TimerTick(object sender, object e)
        {
            OnPropertyChanged(() => Duration);
            OnPropertyChanged(() => WorkingDaysDuration);
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

        private TimeSpan _workdayStart;
        private TimeSpan _workdayEnd;
        private TimeSpan _workdayLength;
        private List<DateTime> _vacationAndHolidays;
        public string WorkingDaysDuration
        {
            get
            {
                if (_endDate < DateTime.Now)
                    return "";

                var duration = GetBusinessTimespanBetween(DateTime.Now, _endDate, _workdayStart, _workdayEnd, _vacationAndHolidays);
                double fractionalDays = duration / _workdayLength;
                int days = (int)Math.Truncate(fractionalDays);
                TimeSpan partialDay = _workdayLength * (fractionalDays - days);
                string dayString = days > 1 ? $"{days} Working Days" : days > 0 ? "1 Working Day" : "Working Hours:";
                return $"{dayString} {partialDay:h':'mm':'ss}";
            }
        }

        private static TimeSpan GetBusinessTimespanBetween(
            DateTime start, DateTime end,
            TimeSpan workdayStartTime, TimeSpan workdayEndTime,
            List<DateTime> holidays = null)
        {
            if (end < start)
                throw new ArgumentException("start datetime must be before end datetime.");

            // Just create an empty list for easier coding.
            if (holidays == null) holidays = new List<DateTime>();

            if (holidays.Where(x => x.TimeOfDay.Ticks > 0).Any())
                throw new ArgumentException("holidays can not have a TimeOfDay, only the Date.");

            var nonWorkDays = new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday };

            var startTime = start.TimeOfDay;

            // If the start time is before the starting hours, set it to the starting hour.
            if (startTime < workdayStartTime) startTime = workdayStartTime;

            var timeBeforeEndOfWorkDay = workdayEndTime - startTime;

            // If it's after the end of the day, then this time lapse doesn't count.
            if (timeBeforeEndOfWorkDay.TotalSeconds < 0) timeBeforeEndOfWorkDay = new TimeSpan();
            // If start is during a non work day, it doesn't count.
            if (nonWorkDays.Contains(start.DayOfWeek)) timeBeforeEndOfWorkDay = new TimeSpan();
            else if (holidays.Contains(start.Date)) timeBeforeEndOfWorkDay = new TimeSpan();

            var endTime = end.TimeOfDay;

            // If the end time is after the ending hours, set it to the ending hour.
            if (endTime > workdayEndTime) endTime = workdayEndTime;

            var timeAfterStartOfWorkDay = endTime - workdayStartTime;

            // If it's before the start of the day, then this time lapse doesn't count.
            if (timeAfterStartOfWorkDay.TotalSeconds < 0) timeAfterStartOfWorkDay = new TimeSpan();
            // If end is during a non work day, it doesn't count.
            if (nonWorkDays.Contains(end.DayOfWeek)) timeAfterStartOfWorkDay = new TimeSpan();
            else if (holidays.Contains(end.Date)) timeAfterStartOfWorkDay = new TimeSpan();

            // Easy scenario if the times are during the day day.
            if (start.Date.CompareTo(end.Date) == 0)
            {
                if (nonWorkDays.Contains(start.DayOfWeek)) return new TimeSpan();
                else if (holidays.Contains(start.Date)) return new TimeSpan();
                return endTime - startTime;
            }
            else
            {
                var timeBetween = end - start;
                var daysBetween = (int)Math.Floor(timeBetween.TotalDays);
                var dailyWorkSeconds = (int)Math.Floor((workdayEndTime - workdayStartTime).TotalSeconds);

                var businessDaysBetween = 0;

                // Now the fun begins with calculating the actual Business days.
                if (daysBetween > 0)
                {
                    var nextStartDay = start.AddDays(1).Date;
                    var dayBeforeEnd = end.AddDays(-1).Date;
                    for (DateTime d = nextStartDay; d <= dayBeforeEnd; d = d.AddDays(1))
                    {
                        if (nonWorkDays.Contains(d.DayOfWeek)) continue;
                        else if (holidays.Contains(d.Date)) continue;
                        businessDaysBetween++;
                    }
                }

                var dailyWorkSecondsToAdd = dailyWorkSeconds * businessDaysBetween;

                var output = timeBeforeEndOfWorkDay + timeAfterStartOfWorkDay;
                output = output + new TimeSpan(0, 0, dailyWorkSecondsToAdd);

                return output;
            }
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(((MemberExpression)exp.Body).Member.Name));
        }
    }
}
