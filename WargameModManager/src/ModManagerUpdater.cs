using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace WargameModManager {
    class ModManagerUpdater {
        private const string MODMANAGER_LATEST_RELEASE = @"https://api.github.com/repos/pvutov/WargameModManager/releases/latest";

        private String downloadDir;
        private String zipPath;
        private String responseJson;
        private String latestVersion;
        private String downloadUrl;
        private String patchNotes;


        public ModManagerUpdater() {
            // Override the .NET 4.5 defaults since github does not support them: 
            // https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MODMANAGER_LATEST_RELEASE);

            // specify API version to use for stability
            request.Accept = "application/vnd.github.v3.raw+json";
            request.UserAgent = "pvutov/WargameModManager";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                responseJson = reader.ReadToEnd();
            }

            latestVersion = Utility.getStringJsonField("tag_name", responseJson);
            patchNotes = Utility.getStringJsonField("body", responseJson);
            downloadUrl = Utility.getStringJsonField("browser_download_url", responseJson);
                        
            downloadDir = Path.Combine(Path.GetTempPath(), "WargameModManager");
            DirectoryInfo di = new DirectoryInfo(downloadDir);
            // remove old files if program didn't clean up last time
            if (di.Exists) {
                di.Delete(true);
            }
            di.Create();
            zipPath = Path.Combine(downloadDir, "WargameModManagerUpdate.zip");
        }

        public bool checkForUpdates() {
            Version currentVer = Assembly.GetEntryAssembly().GetName().Version;
            Version latestVer;
            try {
                latestVer = new Version(this.latestVersion);
            }
            catch (FormatException) {
                Program.warning("Version of latest git release could not be parsed.");
                return false;
            }

            return latestVer > currentVer;
        }

        public void applyUpdate(Action<int> reportProgress) {
            using (WebClient wc = new WebClient()) {
                wc.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                    reportProgress(e.ProgressPercentage);
                };

                // after download
                wc.DownloadFileCompleted += delegate (object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, downloadDir);
                    File.Delete(zipPath);
                    String updaterDir = Path.Combine(downloadDir, "Updater.exe");
                    try {
                        Process.Start(updaterDir, "\"" +
                            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                            + "\" " + patchNotes);
                    }
                    catch (System.ComponentModel.Win32Exception w) {
                        Program.warning(updaterDir + " not found." + w.Message + w.ErrorCode.ToString() + w.NativeErrorCode.ToString());
                    }
                };

                wc.DownloadFileAsync(new Uri(downloadUrl), zipPath);
            }
        }
    }
}
