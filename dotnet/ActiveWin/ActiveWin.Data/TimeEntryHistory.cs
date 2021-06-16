using System;
using System.Collections.Generic;

#nullable disable

namespace ActiveWin.Data
{
    public partial class TimeEntryHistory
    {
        public long Id { get; set; }
        public long TimeEntryId { get; set; }
        public double TotalTimeSpent { get; set; }
        public string CreatedAt { get; set; }

        public virtual TimeEntry TimeEntry { get; set; }
    }
}
