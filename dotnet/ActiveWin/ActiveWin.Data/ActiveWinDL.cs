using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ActiveWin.Data
{
    public class ActiveWinDL
    {

      public bool AddTimeEntryHistory(TimeEntryModel timeEntry, string createdAt) {

        var savedChanges = false;

        try
        {
          using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
          {

            var newTimeEntryHistory = new TimeEntryHistory
            {
              TimeEntryId = timeEntry.Id,
              CreatedAt = createdAt,
              TotalTimeSpent = timeEntry.TotalTimeSpent,
            };
            dbContext.TimeEntryHistories.Add(newTimeEntryHistory);

            savedChanges = dbContext.SaveChanges() > 0;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          throw ex;
        }

        return savedChanges;
      }

      public bool AddMultipleTimeEntryHistories(List<TimeEntryHistoryModel> inMemoryimeHistories) {

        var changes = 0;

        try
        {
          using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
          {

            foreach(var timeHistory in inMemoryimeHistories)
            {
              var today = DateTime.Now.ToString("yyyy-MM-dd");
              var dbTimeEntry = dbContext.TimeEntries.FirstOrDefault(x => x.Application == timeHistory.Application && x.Company == timeHistory.Company && x.CreatedAt.StartsWith(today));
              if (dbTimeEntry != null)
              {
                var newTimeEntryHistory = new TimeEntryHistory
                {
                  TimeEntryId = dbTimeEntry.Id,
                  CreatedAt = timeHistory.CreatedAt,
                  TotalTimeSpent = timeHistory.TotalTimeSpent,
                };
                dbContext.TimeEntryHistories.Add(newTimeEntryHistory);
              }
            }

            changes = dbContext.SaveChanges();
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          throw ex;
        }

        return changes > 0;
      }

      public bool UpdateTimeEntryTotalTimeSpent(TimeEntryModel timeEntry)
      {
          var savedChanges = false;

          try
          {
            using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
            {
              var today = DateTime.Now.ToString("yyyy-MM-dd");
              TimeEntry dbTimeEntry = null;

              if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
              {
                dbTimeEntry = dbContext.TimeEntries.FirstOrDefault(x => x.Application == timeEntry.Application && x.Company == timeEntry.Company && x.CreatedAt.StartsWith(today));
              }
              else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
              {
                dbTimeEntry = dbContext.TimeEntries.FirstOrDefault(x => x.BundleId == timeEntry.BundleId && x.CreatedAt.StartsWith(today));
              }

              if (dbTimeEntry != null)
              {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                  dbTimeEntry.Application = timeEntry.Application;
                }
                dbTimeEntry.TotalTimeSpent = timeEntry.TotalTimeSpent;
              }
              savedChanges = dbContext.SaveChanges() > 0;
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex);
            throw ex;
          }

          return savedChanges;
      }

      public TimeEntryModel TimeEntryExistsMac(string bundleId)
      {
          TimeEntryModel timeEntry = null;
          try
          {
            using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
            {
              var today = DateTime.Now.ToString("yyyy-MM-dd");

              var dbTimeEntry = dbContext.TimeEntries.FirstOrDefault(x => x.BundleId == bundleId && x.CreatedAt.StartsWith(today));
              if (dbTimeEntry != null)
              {
                timeEntry = new TimeEntryModel
                {
                  Id = dbTimeEntry.Id,
                  Application = dbTimeEntry.Application,
                  Platform = dbTimeEntry.Platform,
                  BundleId = dbTimeEntry.BundleId,
                  IconPath = dbTimeEntry.IconPath,
                  CreatedAt = dbTimeEntry.CreatedAt,
                  TotalTimeSpent = dbTimeEntry.TotalTimeSpent
                };
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex);
            throw ex;
          }
          return timeEntry;
      }

      public TimeEntryModel TimeEntryExistsWindows(string application, string company)
      {
          TimeEntryModel timeEntry = null;
          try
          {
            using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
            {
              var today = DateTime.Now.ToString("yyyy-MM-dd");

              var dbTimeEntry = dbContext.TimeEntries.FirstOrDefault(x => x.Application == application && x.Company == company && x.CreatedAt.StartsWith(today));
              if (dbTimeEntry != null)
              {
                timeEntry = new TimeEntryModel {
                  Id = dbTimeEntry.Id,
                  Application = dbTimeEntry.Application,
                  Platform = dbTimeEntry.Platform,
                  Company = dbTimeEntry.Company,
                  IconPath = dbTimeEntry.IconPath,
                  CreatedAt = dbTimeEntry.CreatedAt,
                  TotalTimeSpent = dbTimeEntry.TotalTimeSpent
                };
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex);
            throw ex;
          }
          return timeEntry;
      }

      public bool AddMultipleTimeEntries(List<TimeEntryModel> inMemoryTimeEntries)
      {
        var changes = 0;

        try
        {
          using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
          {

            foreach(var timeEntry in inMemoryTimeEntries)
            {
              var newTimeEntry = new TimeEntry
              {
                Application = timeEntry.Application,
                Platform = timeEntry.Platform,
                Company = timeEntry.Company,
                IconPath = timeEntry.IconPath,
                CreatedAt = timeEntry.CreatedAt,
                TotalTimeSpent = timeEntry.TotalTimeSpent,
              };
              dbContext.TimeEntries.Add(newTimeEntry);

              if (dbContext.SaveChanges() > 0)
              {
                changes++;
                var newTimeEntryHistory = new TimeEntryHistory
                {
                  TimeEntryId = newTimeEntry.Id,
                  TotalTimeSpent = timeEntry.TotalTimeSpent,
                  CreatedAt = timeEntry.CreatedAt
                };
                dbContext.TimeEntryHistories.Add(newTimeEntryHistory);
                if (dbContext.SaveChanges() > 0) changes++;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          throw ex;
        }
        return changes > 0;
      }

      public bool AddUpdateMultipleTimeEntries(List<TimeEntryModel> inMemoryTimeEntries)
      {
        var changes = 0;

        try
        {
          using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
          {

            foreach(var timeEntry in inMemoryTimeEntries)
            {
              var today = DateTime.Now.ToString("yyyy-MM-dd");
              var dbTimeEntry = dbContext.TimeEntries.FirstOrDefault(x => x.Application == timeEntry.Application && x.Company == timeEntry.Company && x.CreatedAt.StartsWith(today));

              if (dbTimeEntry != null)
              {
                 if (UpdateTimeEntryTotalTimeSpent(timeEntry))
                 {
                   changes++;
                 }
              }
              else
              {
                  var newTimeEntry = new TimeEntry
                  {
                    Application = timeEntry.Application,
                    Platform = timeEntry.Platform,
                    Company = timeEntry.Company,
                    IconPath = timeEntry.IconPath,
                    CreatedAt = timeEntry.CreatedAt,
                    TotalTimeSpent = timeEntry.TotalTimeSpent,
                  };
                  dbContext.TimeEntries.Add(newTimeEntry);

                  if (dbContext.SaveChanges() > 0)
                  {
                    changes++;
                    var newTimeEntryHistory = new TimeEntryHistory
                    {
                      TimeEntryId = newTimeEntry.Id,
                      TotalTimeSpent = timeEntry.TotalTimeSpent,
                      CreatedAt = timeEntry.CreatedAt
                    };
                    dbContext.TimeEntryHistories.Add(newTimeEntryHistory);
                    if (dbContext.SaveChanges() > 0)
                    {
                      changes++;
                    }
                  }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          throw ex;
        }
        return changes > 0;
      }

      public bool AddTimeEntry(TimeEntryModel timeEntry)
      {

        var savedChanges = false;

        try
        {
          using (ActiveWinDBContext dbContext = new ActiveWinDBContext())
          {
            var changes = 0;

            var newTimeEntry = new TimeEntry
            {
              Application = timeEntry.Application,
              Platform = timeEntry.Platform,
              BundleId = timeEntry.BundleId,
              Company = timeEntry.Company,
              IconPath = timeEntry.IconPath,
              CreatedAt = timeEntry.CreatedAt,
              TotalTimeSpent = timeEntry.TotalTimeSpent,
            };
            dbContext.TimeEntries.Add(newTimeEntry);

           changes = dbContext.SaveChanges();

            var newTimeEntryHistory = new TimeEntryHistory
            {
              TimeEntryId = newTimeEntry.Id,
              TotalTimeSpent = timeEntry.TotalTimeSpent,
              CreatedAt = timeEntry.CreatedAt
            };
            dbContext.TimeEntryHistories.Add(newTimeEntryHistory);

            changes += dbContext.SaveChanges();
            savedChanges = changes == 2;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          throw ex;
        }

        return savedChanges;
      }
    }
}
