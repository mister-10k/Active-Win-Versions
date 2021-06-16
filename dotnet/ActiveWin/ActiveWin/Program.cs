
using ActiveWin.Data;
using ActiveWin.Shared;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveWin
{
  class Program
  {

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint procId);
    private static string currWindow = "";
    private static string logsFilelName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ActiveWin\\logs.json" : "ActiveWin/logs.json";
    private static string iconFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ActiveWin\\Icons\\" : "ActiveWin/Icons/";
    private static bool systemLokced = false;


    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        AppConstants.BasePath = args[0];
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
          StartActiveWinWindows();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
          NSApplication.Init();
          StartActiveWinMac();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {

        }
        string val = Console.ReadLine();
      }

    }

    private static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
    {
      if (e.Reason == SessionSwitchReason.SessionLock)
      {
        systemLokced = true;
      }
      else if (e.Reason == SessionSwitchReason.SessionUnlock)
      {
        systemLokced = false;
      }
    }

    public static void hi()
    {
      Console.WriteLine("hi");
    }

    private static void StartActiveWinMac()
    {
      // set system lock event
      // SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);

      //NSWorkspace.Notifications.ObserveActiveSpaceDidChange((object sender, NSNotificationEventArgs e) =>
      //{
      //  Console.Write("Your Mac is getting sleepy\n");
      //});
      // var x = NSApplication.DidUpdateNotification.Description;
      //NSNotificationCenter.DefaultCenter.AddObserver(NSWorkspace.Notifications.ObserveScreensDidSleep.ToString(), (NSNotification notification) => { Console.WriteLine("zzzzzzzzzz"); });
      // NSDistributedNotificationCenter.DefaultCenter.AddObserver((NSNotification nsn) => { Console.WriteLine("hi"); }, null, NSKeyValueObservingOptions.New, null);
      // NSDistributedNotificationCenter.DefaultCenter.AddObserver(new NSObject(), "com.apple.screenIsLocked", NSKeyValueObservingOptions.New, null)
      //NSDistributedNotificationCenter.DefaultCenter.AddObserver("com.apple.screenIsLocked", NSKeyValueObservingOptions.Initial, (obj) => {
      //  Console.WriteLine("123");
      //});


      var activeWinDL = new ActiveWinDL();
        var startTime = DateTime.Now;

        while (true)
        {
          Thread.Sleep(1000);

          try
          {
            var activeApp = NSWorkspace.SharedWorkspace.ActiveApplication;
            currWindow = activeApp["NSApplicationName"].Description; 
            var bundleId = activeApp["NSApplicationBundleIdentifier"].Description;
            systemLokced = bundleId == "com.apple.loginwindow" ? true : false;
            
            var appPath = activeApp["NSApplicationPath"].Description;
            var indexOfLastForwardSlash = appPath.LastIndexOf("/");
            var indexOfDotApp = appPath.IndexOf(".app");
            if (indexOfLastForwardSlash != -1 && indexOfDotApp != -1)
            {
              currWindow = appPath.Substring(indexOfLastForwardSlash + 1, indexOfDotApp - (indexOfLastForwardSlash + 1));
            }

            var nsImage = NSWorkspace.SharedWorkspace.IconForFile(appPath);
            var image = NSImageToImage(nsImage);
            

            var iconSavePath = AppConstants.BasePath + iconFolder + bundleId + ".jpg";
            if (!File.Exists(iconSavePath))
            {
              image.Save(iconSavePath);
            }


            var timeEntry = activeWinDL.TimeEntryExistsMac(bundleId);
            var endTime = DateTime.Now;
            var elapsedSeconds = (endTime - startTime).TotalSeconds;

            if (!systemLokced)
            {
              if (timeEntry != null)
              {
                timeEntry.Application = currWindow;
                timeEntry.TotalTimeSpent += elapsedSeconds;
                activeWinDL.UpdateTimeEntryTotalTimeSpent(timeEntry);
                activeWinDL.AddTimeEntryHistory(timeEntry, endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"));
              }
              else
              {
                var newEntry = new TimeEntryModel
                {
                  Application = currWindow,
                  Platform = "macOS",
                  BundleId = bundleId,
                  IconPath = iconSavePath,
                  TimeHistory = new List<TimeEntryHistoryModel> { new TimeEntryHistoryModel { TotalTimeSpent = elapsedSeconds, CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") } },
                  CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"),
                  TotalTimeSpent = elapsedSeconds,
                };
                activeWinDL.AddTimeEntry(newEntry);
              }
            }

            startTime = endTime;

            Console.WriteLine(currWindow + " => " + bundleId);

          }
          catch (Exception e)
          {
            LogError(e);
          }
        }

    }

    private static void StartActiveWinWindows()
    {
      // set system lock event
      SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);

      var task = Task.Run(async () => {
        var activeWinDL = new ActiveWinDL();
        var startTime = DateTime.Now;

        while (true)
        {
          await Task.Delay(1000);

          try
          {
            var handle = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(handle, out pid);
            var process = Process.GetProcessById((Int32)pid);
            var fileName = process.MainModule.FileName;
            var myFileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);
            currWindow = myFileVersionInfo.FileDescription;
            var company = myFileVersionInfo.CompanyName;

            var iconPath = AppConstants.BasePath + iconFolder + currWindow + " - " + company + ".jpg";
            if (!File.Exists(iconPath))
            {
              var icon = Icon.ExtractAssociatedIcon(fileName);
              var bitmap = icon.ToBitmap();
              bitmap.Save(iconPath);
            }


            var timeEntry = activeWinDL.TimeEntryExistsWindows(currWindow, company);
            var endTime = DateTime.Now;
            var elapsedSeconds = (endTime - startTime).TotalSeconds;

            if (!systemLokced)
            {
              if (timeEntry != null)
              {
                timeEntry.TotalTimeSpent += elapsedSeconds;
                activeWinDL.UpdateTimeEntryTotalTimeSpent(timeEntry);
                activeWinDL.AddTimeEntryHistory(timeEntry, endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"));
              }
              else
              {
                var newEntry = new TimeEntryModel
                {
                  Application = currWindow,
                  Platform = "Windows",
                  Company = company,
                  IconPath = iconPath,
                  TimeHistory = new List<TimeEntryHistoryModel> { new TimeEntryHistoryModel { TotalTimeSpent = elapsedSeconds, CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") } },
                  CreatedAt = endTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff"),
                  TotalTimeSpent = elapsedSeconds,
                };
                activeWinDL.AddTimeEntry(newEntry);
              }
            }

            startTime = endTime;

          }
          catch (Exception e)
          {
            await LogError(e);
          }

          Console.WriteLine(currWindow);
        }
      });
    }

    private static async Task LogError(Exception e)
    {
      var json = "";
      using (StreamReader r = new StreamReader(AppConstants.BasePath + logsFilelName))
      {
        json = r.ReadToEnd();
        json = json == "" || json == "\n" ? "[]" : json;
        var logs = JsonConvert.DeserializeObject<List<string>>(json);

        logs.Add(DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss") + " => Error Type: " + e.GetType().Name + " Message: " + e.StackTrace);

        json = JsonConvert.SerializeObject(logs);
      }

      if (json != "")
        await File.WriteAllTextAsync(AppConstants.BasePath + logsFilelName, json);
    }

    private static Image NSImageToImage(NSImage img)
    {
      using (var imageData = img.AsTiff())
      {
        var imgRep = NSBitmapImageRep.ImageRepFromData(imageData) as NSBitmapImageRep;
        var imageProps = new NSDictionary();
        var data = imgRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, imageProps);
        return Image.FromStream(data.AsStream());
      }
    }
  }
}
