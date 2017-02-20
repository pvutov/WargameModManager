﻿using System;
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

            DirectoryInfo di = Directory.CreateDirectory(Path.GetTempPath() + "WargameModManager");
            downloadDir = Path.Combine(Path.GetTempPath(), "WargameModManager");
            zipPath = Path.Combine(downloadDir, "WargameModManagerUpdate.zip");

            // make sure download dir is empty
            foreach (FileInfo file in di.GetFiles()) {
                file.Delete();
            }
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
