using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Calendar
{
    public interface ICalendarEventInterface
    {
        void SetCurrentEvents(List<CalendarEvent> events);
    }

    public class CalendarEvent
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Description { get; set; }
        public bool IsAllDay { get; set; }
    }
}
