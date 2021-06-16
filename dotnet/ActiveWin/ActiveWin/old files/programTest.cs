
//using ActiveWin.Data;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;

//namespace ActiveWin
//{
//  class ProgramTest
//  {

//    [DllImport("user32.dll")]
//    static extern IntPtr GetForegroundWindow();
//    [DllImport("user32.dll")]
//    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint procId);
//    private static string currWindow = "";
//    private static string csTimeEntriesFileName = "csTimeEntries.json";
//    private static string logsFielName = "logs.json";
//    private static string csTimeEntriesLockFileName = "csTimeEntriesLock.txt";
//    private static string iconFolder = "Icons\\";
//    private static List<TimeEntryModel> InMemoryTimeEntries = new List<TimeEntryModel> { };
//    private static List<TimeEntryHistoryModel> InMemoryimeHistories = new List<TimeEntryHistoryModel> { };


//    static void Main(string[] args)
//    {
//      if (args.Length > 0)
//      {
//        StartActiveWin(args[0]);
//        //StartTrackingAppsTimeHistory(args[0]);

//        string val = Console.ReadLine();
//      }

//    }

//    private static void StartActiveWin(string baseDirectory)
//    {
//      var task = Task.Run(async () => {
//        var stopWatch = new Stopwatch();
//        var activeWinDL = new ActiveWinDL();
//        var startTime = DateTime.Now;

//        while (true)
//        {
//          stopWatch.Restart();
//          await Task.Delay(1000);

//          try
//          {
//            var locked = File.ReadAllText(baseDirectory + csTimeEntriesLockFileName);
//            while (locked == "true")
//            {
//              await Task.Delay(1000);
//              Console.WriteLine("waiting");
//              locked = File.ReadAllText(baseDirectory + csTimeEntriesLockFileName);
//            }

//            var handle = GetForegroundWindow();

//            uint pid;
//            GetWindowThreadProcessId(handle, out pid);
//            var process = Process.GetProcessById((Int32)pid);
//            var fileName = process.MainModule.FileName;
//            var myFileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);

//            currWindow = myFileVersionInfo.FileDescription;
//            var company = myFileVersionInfo.CompanyName;

//            var iconPath = baseDirectory + iconFolder + currWindow + " - " + company + ".jpg";
//            if (!File.Exists(iconPath))
//            {
//              var icon = Icon.ExtractAssociatedIcon(fileName);
//              var bitmap = icon.ToBitmap();
//              bitmap.Save(iconPath);
//            }


//            var timeEntry = activeWinDL.TimeEntryExists("Google Chrome", "Google LLC");
//            var endTime = DateTime.Now;
//            var elapsedSeconds = (endTime - startTime).TotalSeconds;

//            if (timeEntry != null)
//            {
//              timeEntry.TotalTimeSpent += elapsedSeconds;

//              activeWinDL.UpdateTimeEntryTotalTimeSpent(timeEntry);
//              activeWinDL.AddTimeEntryHistory(timeEntry, endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"));
//              // InMemoryimeHistories.Add(new TimeEntryHistoryModel
//              // {
//              //   Application = "Google Chrome",
//              //   Platform = "Windows",
//              //   Company = "Google LLC",
//              //   CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"),
//              //   TotalTimeSpent = timeEntry.TotalTimeSpent,
//              // });
//            }
//            else
//            {
//              var newEntry = new TimeEntryModel
//              {
//                Application = "Google Chrome",
//                Platform = "Windows",
//                Company = "Google LLC",
//                IconPath = iconPath,
//                TimeHistory = new List<TimeEntryHistoryModel> { new TimeEntryHistoryModel { TotalTimeSpent = elapsedSeconds, CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") } },
//                CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"),
//                TotalTimeSpent = elapsedSeconds,
//              };
//              // InMemoryTimeEntries.Add(newEntry);
//              activeWinDL.AddTimeEntry(newEntry);
//            }
//            startTime = endTime;
//            //string json = "";
//            //using (StreamReader r = new StreamReader(baseDirectory + csTimeEntriesFileName))
//            //{
//            //  json = r.ReadToEnd();
//            //  json = json == "" || json == "\n" ? "[]" : json;
//            //  var timeEntries = JsonConvert.DeserializeObject<List<TimeEntryModel>>(json);

//            //  // var entry = timeEntries.FirstOrDefault(x => x.Application == currWindow && x.Company == company && x.CreatedAt.StartsWith(DateTime.Now.ToString("yyyy-MM-dd")));
//            //  var entry = timeEntries.FirstOrDefault(x => x.Application == "Google Chrome" && x.Company == "Google LLC" && x.CreatedAt.StartsWith(DateTime.Now.ToString("yyyy-MM-dd")));

//            //  //var elapsedSeconds = (stopWatch.ElapsedMilliseconds / (double)1000);
//            //  var endTime = DateTime.Now;
//            //  var elapsedSeconds = (endTime - startTime).TotalSeconds;

//            //  if (entry != null)
//            //  {
//            //    entry.TotalTimeSpent += elapsedSeconds;
//            //    // entry.TimeHistory.Add(new TimeHistory { TotalTimeSpent = entry.TotalTimeSpent, CreatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") });
//            //    entry.TimeHistory.Add(new TimeHistory { TotalTimeSpent = entry.TotalTimeSpent, CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") });
//            //  }
//            //  else
//            //  {
//            //    //var newEntry = new TimeEntry
//            //    //{
//            //    //  Application = currWindow,
//            //    //  Platform = "Windows",
//            //    //  Company = company,
//            //    //  IconPath = iconPath,
//            //    //  TimeHistory = new List<TimeHistory> { new TimeHistory { TotalTimeSpent = elapsedSeconds, CreatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") } },
//            //    //  CreatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"),
//            //    //  TotalTimeSpent = elapsedSeconds,
//            //    //};
//            //    var newEntry = new TimeEntryModel
//            //    {
//            //      Application = "Google Chrome",
//            //      Platform = "Windows",
//            //      Company = "Google LLC",
//            //      IconPath = iconPath,
//            //      TimeHistory = new List<TimeHistory> { new TimeHistory { TotalTimeSpent = elapsedSeconds, CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") } },
//            //      CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"),
//            //      TotalTimeSpent = elapsedSeconds,
//            //    };
//            //    activeWinDL.AddTimeEntry(newEntry);
//            //    // timeEntries.Add(newEntry);
//            //  }

//            //  // json = JsonConvert.SerializeObject(timeEntries);
//            //}

//            //if (json != "")
//            //  File.WriteAllText(baseDirectory + csTimeEntriesFileName, json);

//          }
//          catch (Exception e)
//          {
//            var json = "";
//            using (StreamReader r = new StreamReader(baseDirectory + logsFielName))
//            {
//              json = r.ReadToEnd();
//              json = json == "" || json == "\n" ? "[]" : json;
//              var logs = JsonConvert.DeserializeObject<List<string>>(json);

//              logs.Add(DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss") + " => Error Type: " + e.GetType().Name + " Message: " + e.Message);

//              json = JsonConvert.SerializeObject(logs);
//            }

//            if (json != "")
//              File.WriteAllText(baseDirectory + logsFielName, json);
//          }

//          Console.WriteLine(currWindow);
//        }
//      });
//    }

//    private static void StartActiveWinDBUpdater()
//    {
//      var activeWinDL = new ActiveWinDL();
//      var task = Task.Run(async () =>
//      {
//        while (true)
//        {
//          await Task.Delay(1000);

//          if (activeWinDL.AddUpdateMultipleTimeEntries(InMemoryTimeEntries))
//          {
//            InMemoryTimeEntries = new List<TimeEntryModel> {};
//          };
//          if (activeWinDL.AddMultipleTimeEntryHistories(InMemoryimeHistories))
//          {
//            InMemoryimeHistories = InMemoryimeHistories = new List<TimeEntryHistoryModel> { };
//          }
//        }

//      });
//      }

//    private static void StartTrackingAppsTimeHistory(string baseDirectory)
//    {
//      var task = Task.Run(async () => {
//        while(true)
//        {
//          await Task.Delay(60000);

//          try
//          {
//            var locked = File.ReadAllText(baseDirectory + csTimeEntriesLockFileName);
//            while (locked == "true")
//            {
//              await Task.Delay(1000);
//              locked = File.ReadAllText(baseDirectory + csTimeEntriesLockFileName);
//            }

//            File.WriteAllText(baseDirectory + csTimeEntriesLockFileName, "true");

//            var json = "";
//            using (StreamReader r = new StreamReader(baseDirectory + csTimeEntriesFileName))
//            {
//              json = r.ReadToEnd();
//              var timeEntries = JsonConvert.DeserializeObject<List<TimeEntryModel>>(json);

//              foreach(var timeEntry in timeEntries)
//              {
//                if (timeEntry.CreatedAt.StartsWith(DateTime.Now.ToString("yyyy-MM-dd")))
//                {
//                  var newTimeHistory = new TimeEntryHistoryModel
//                  {
//                    TotalTimeSpent = timeEntry.TotalTimeSpent,
//                    CreatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss")
//                  };

//                  timeEntry.TimeHistory.Add(newTimeHistory);
//                }
//              }

//              json = JsonConvert.SerializeObject(timeEntries);
//            }

//            if (json != "")
//              File.WriteAllText(baseDirectory + csTimeEntriesFileName, json);

//          }
//          catch (Exception e)
//          {
//            Console.WriteLine(e.Message);
//          }
//          finally
//          {
//            File.WriteAllText(baseDirectory + csTimeEntriesLockFileName, "false");
//          }

//        }
//      });
//    }
//  }
//}
