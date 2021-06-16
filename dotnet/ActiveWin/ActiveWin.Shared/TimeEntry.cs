using System.Collections.Generic;

namespace ActiveWin
{
  public class TimeEntryModel
  {
    public long Id { get; set; }
    public string Application { get; set;}
    public string Platform { get; set;}
    public string BundleId { get; set; }
    public string Company { get; set;}
    public string IconPath { get; set; }
    public List<TimeEntryHistoryModel> TimeHistory { get; set;}
    public string CreatedAt { get; set;}
    public double TotalTimeSpent { get; set;}
  }

  public class TimeEntryHistoryModel
  {
    public string Application { get; set;}
    public string Platform { get; set;}
    public string Company { get; set;}
    public double TotalTimeSpent { get; set; }
    public string CreatedAt { get; set; }
  }
}
