/* eslint global-require: off, no-console: off */

/**
 * This module executes inside of electron's main process. You can start
 * electron renderer process from here and communicate with the other processes
 * through IPC.
 *
 * When running `yarn build` or `yarn build:main`, this file is compiled to
 * `./src/main.prod.js` using webpack. This gives us some performance wins.
 */
 import 'core-js/stable';
 import 'regenerator-runtime/runtime';
 import path from 'path';
 import { app, BrowserWindow, shell, globalShortcut, Tray, Menu, screen } from 'electron';
 import { autoUpdater } from 'electron-updater';
 import log from 'electron-log';
 import MenuBuilder from './menu';
import { DataShareService } from './services/data-share.service';
import { ChildProcess } from 'child_process';

 const { spawn } = require('child_process');
 let activeWinInstance:ChildProcess, isQuitting: boolean = false, updateAvailable: boolean = false;
 const iconPath = process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true' ? path.join(__dirname, '../assets/logo-laps-tray-win.png') : path.join(process.resourcesPath, "assets/logo-laps-tray-win.png");
 const args = process.argv.slice(1);
 const hidden = args.some(val => val === '--hidden');


 let mainWindow: BrowserWindow | null = null;

 // Setup Auto Launcher
app.setLoginItemSettings({
  openAtLogin: true,
  path: app.getPath('exe'),
  args: [`"--hidden"`]
});

 if (process.env.NODE_ENV === 'production') {
   const sourceMapSupport = require('source-map-support');
   sourceMapSupport.install();
 }

 if (process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true') {
   require('electron-debug')();
 }

 const startActiveWin = () => {
  if (process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true') {
    // if (process.platform === "darwin") { // mac os doesn't kill child processes, so this has to be setup for workaround
    //   spawn('dotnet', ['build', path.join(__dirname, 'data/ActiveWin/ActiveWin/ActiveWin.csproj'), 'p:UseRazorBuildServer=false', '-p:UseSharedCompilation=false']);
    //   spawn('dotnet', ['build-server', 'shutdown']);
    //   activeWinInstance = spawn('dotnet', ['run', '--no-build', '--project', path.join(__dirname, 'data/ActiveWin/ActiveWin/ActiveWin.csproj'), path.join(__dirname, 'data/ActiveWin/')]);
    // } else {
    //   activeWinInstance = spawn('dotnet', ['run', '--project', path.join(__dirname, 'data/ActiveWin/ActiveWin/ActiveWin.csproj'), path.join(__dirname, 'data/ActiveWin/')]);
    // }

    activeWinInstance = spawn('python', [path.join(__dirname, 'data/ActiveWin/program.py')]);

    (activeWinInstance as any).stdout.on('data', function(data:any) {
        console.log(data.toString());
    });
    setInterval(() =>  reRunActiveWinIfInactive(), 3000);
  } else {
    activeWinInstance = spawn(path.join(process.resourcesPath, "data/ActiveWin/LapsHelper.exe"), [path.join(process.resourcesPath, "data/ActiveWin/")]);

    (activeWinInstance as any).stdout.on('data', function(data:any) {
      console.log(data.toString());
    });
    setInterval(() =>  reRunActiveWinIfInactive(), 3000);
  }
}

const reRunActiveWinIfInactive = () => {
  if (process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true') {
    if (!activeWinInstance || !require('is-running')(activeWinInstance.pid)) {
      if (process.platform === "darwin") { // mac os doesn't kill child processes, so this has to be setup for workaround
        spawn('dotnet', ['build', path.join(__dirname, 'data/ActiveWin/ActiveWin/ActiveWin.csproj'), 'p:UseRazorBuildServer=false', '-p:UseSharedCompilation=false']);
        spawn('dotnet', ['build-server', 'shutdown']);
        activeWinInstance = spawn('dotnet', ['run', '--no-build', '--project', path.join(__dirname, 'data/ActiveWin/ActiveWin/ActiveWin.csproj'), path.join(__dirname, 'data/ActiveWin/')]);
      } else {
        activeWinInstance = spawn('dotnet', ['run', '--project', path.join(__dirname, 'data/ActiveWin/ActiveWin/ActiveWin.csproj'), path.join(__dirname, 'data/ActiveWin/')]);
      }

      (activeWinInstance as any).stdout.on('data', function(data:any) {
          console.log(data.toString());
      });
    }
  } else {
    if (!activeWinInstance || !require('is-running')(activeWinInstance.pid)) {
      activeWinInstance = spawn(path.join(process.resourcesPath, "data/ActiveWin/LapsHelper.exe"), [path.join(process.resourcesPath, "data/ActiveWin/")]);

      (activeWinInstance as any).stdout.on('data', function(data:any) {
        console.log(data.toString());
      });
    }
  }
}

const startAssetLineChartCacheRefresher = () => {
  if (process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true') {
    setInterval(async () => {
        DataShareService.refreshCache(path.join(__dirname, 'data/ActiveWin/'));
    }, 600000); // 10 mins
  } else {
    setInterval(async () => {
      DataShareService.refreshCache(path.join(process.resourcesPath, "data/ActiveWin/"));
    }, 600000); // 10 mins
  }
}

const setupAutoUpdater = () => {
  // Auto update listeners
  autoUpdater.on('checking-for-update', () => {});
  autoUpdater.on('update-available', (info) => {});
  autoUpdater.on('update-not-available', (info) => {});
  autoUpdater.on('error', (err) => {});
  autoUpdater.on('download-progress', (progressObj) => {});
  autoUpdater.on('update-downloaded', (info) => { updateAvailable = true;});
  autoUpdater.checkForUpdates();
  setInterval(() =>  autoUpdater.checkForUpdates(), 300000);
  log.transports.file.level = 'info';
  autoUpdater.logger = log;
}

let tray = null;
const setupTray = () => {
  tray = new Tray(iconPath);
      const contextMenu = Menu.buildFromTemplate([
      {
        label: 'Quit                   Ctrl+Q', click:  () => { // 10 tabs
          isQuitting = true;
          if (mainWindow) {
            mainWindow.close();
          }

          app.quit();
        }
      }
    ]);
    tray.setToolTip('Laps');
    tray.setContextMenu(contextMenu);
    tray.on('click', () => {
      if (mainWindow) {
        mainWindow.show();
      } else {
        createWindow();
      }
    });
}

const start = () => {
  if (process.env.NODE_ENV === 'production') { setupAutoUpdater(); }
  startActiveWin();
  startAssetLineChartCacheRefresher();
  setupTray();
  if (!hidden) { createWindow(); }
}

 const installExtensions = async () => {
   const installer = require('electron-devtools-installer');
   const forceDownload = !!process.env.UPGRADE_EXTENSIONS;
   const extensions = ['REACT_DEVELOPER_TOOLS'];

   return installer
     .default(
       extensions.map((name) => installer[name]),
       forceDownload
     )
     .catch(console.log);
 };

 const createWindow = async () => {
   if (
     process.env.NODE_ENV === 'development' ||
     process.env.DEBUG_PROD === 'true'
   ) {
     await installExtensions();
   }

   // https://stackoverflow.com/questions/60106922/electron-non-context-aware-native-module-in-renderer
   app.allowRendererProcessReuse = false;

   const RESOURCES_PATH = app.isPackaged
     ? path.join(process.resourcesPath, 'assets')
     : path.join(__dirname, '../assets');

   const getAssetPath = (...paths: string[]): string => {
     return path.join(RESOURCES_PATH, ...paths);
   };
   const { width, height } = screen.getPrimaryDisplay().workAreaSize;

   mainWindow = new BrowserWindow({
     show: false,
     width: width,
     height: height,
     icon: getAssetPath('icon.png'),
     webPreferences: {
       nodeIntegration: true,
     },
   });

   mainWindow.loadURL(`file://${__dirname}/index.html`);
   console.log(`\n\n\nfile://${__dirname}/index.html\n\n\n`);

  //  globalShortcut.register('CommandOrControl+R', () => {
  //   // throttle refresh
  //   mainWindow?.webContents.send('refresh', path.join(process.resourcesPath, "data/ActiveWin/Laps.exe") + "          " + path.join(process.resourcesPath, "data/ActiveWin/"));
  // });

   // @TODO: Use 'ready-to-show' event
   //        https://github.com/electron/electron/blob/master/docs/api/browser-window.md#using-ready-to-show-event
   mainWindow.webContents.on('did-finish-load', () => {
     if (!mainWindow) {
       throw new Error('"mainWindow" is not defined');
     }
     if (process.env.START_MINIMIZED) {
       mainWindow.minimize();
     } else {
       mainWindow.show();
       mainWindow.focus();
     }
   });

   mainWindow.on('closed', () => {
     mainWindow = null;
   });

   mainWindow.on('close', (event) => {
      if (!isQuitting) {
        event.preventDefault();
        mainWindow?.hide();
      }
      return false;
  });

   const menuBuilder = new MenuBuilder(mainWindow);
   menuBuilder.buildMenu();

   // Open urls in the user's browser
   mainWindow.webContents.on('new-window', (event, url) => {
     event.preventDefault();
     shell.openExternal(url);
   });
 };

 /**
  * Add event listeners...
  */

try {
  const gotTheLock  = app.requestSingleInstanceLock();
  if (!gotTheLock) {
    app.quit();
  } else {
    // This method will be called when Electron has finished
    // initialization and is ready to create browser windows.
    // Some APIs can only be used after this event occurs.
    app.on('ready', start);

    app.on('second-instance', (event, commandLine, workingDirectory) => {
      // Someone tried to run a second instance, we should focus our window.
      if (mainWindow) {
        mainWindow.show();
      } else {
        createWindow();
      }
    });

    app.on('window-all-closed', () => {
      // Respect the OSX convention of having the application in memory even
      // after all windows have been closed
      if (process.platform !== 'darwin') {
        app.quit();
      }
    });

    //  app.whenReady().then(createWindow).catch(console.log);

    app.on('activate', () => {
      // On macOS it's common to re-create a window in the app when the
      // dock icon is clicked and there are no other windows open.
      if (mainWindow === null) createWindow();
    });

    app.on('quit', () => {
      if (process.platform === "darwin" && (process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true')) { // mac os doesn't kill child processes, so this has to be setup for workaround
        if (activeWinInstance) { // due to mac os
          activeWinInstance.kill();
        }
      }
      if (updateAvailable) {
        autoUpdater.quitAndInstall(true, false);
      }
    });
  }
} catch (e) {
  log.warn(e);
  // Catch Error
  // throw e;
}
