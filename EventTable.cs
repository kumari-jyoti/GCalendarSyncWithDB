using System;
using System.Collections.Generic;

namespace GCalendarSyncWithDB
{
    public partial class EventTable
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
