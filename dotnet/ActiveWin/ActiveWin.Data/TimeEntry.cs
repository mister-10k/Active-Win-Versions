using System;
using System.Collections.Generic;

#nullable disable

namespace ActiveWin.Data
{
    public partial class TimeEntry
    {
        public TimeEntry()
        {
            TimeEntryHistories = new HashSet<TimeEntryHistory>();
        }

        public long Id { get; set; }
        public string Application { get; set; }
        public string Platform { get; set; }
        public string Company { get; set; }
        public string IconPath { get; set; }
        public string BundleId { get; set; }
        public double TotalTimeSpent { get; set; }
        public string CreatedAt { get; set; }

        public virtual ICollection<TimeEntryHistory> TimeEntryHistories { get; set; }
    }
}
