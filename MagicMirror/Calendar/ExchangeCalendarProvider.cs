using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Calendar
{
    class ExchangeCalendarProvider : ICalendarEventInterface
    {
        public ExchangeCalendarProvider()
        {

        }

        public List<CalendarEvent> GetCurrentEvents()
        {
            List<CalendarEvent> events = new List<MagicMirror.Calendar.CalendarEvent>();

            // Test data
            ////var start = new DateTime(2017, 3, 17, 11, 0, 0);
            ////var start = DateTime.Today.AddHours(11);
            //var start = DateTime.Now;
            //events.Add(new CalendarEvent { Description = "Tech Symposium", Start = start, End = start.AddHours(2) });
            //start = start.AddHours(3);
            //events.Add(new CalendarEvent { Description = "Clean up", Start = start, End = start.AddMinutes(30) });
            //start = start.AddDays(1).AddHours(4);
            //events.Add(new CalendarEvent { Description = "Part at Gary's", Start = start, End = start.AddHours(3) });
            //start = start.AddDays(2).AddHours(-6);
            //events.Add(new CalendarEvent { Description = "Finish potatoe chips", Start = start, End = start.AddHours(1) });

            return events;
        }
    }
}
