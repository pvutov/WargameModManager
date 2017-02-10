﻿using System;
using System.Net;
using System.IO;

namespace WargameModManager {
    /// <summary>
    /// Responsible for updating mods. A lot of this code is a duplicate of 
    /// ModManagerUpdater, but the main difference is that this doesn't use
    /// a separate .exe to overwrite files.
    /// </summary>
    class ModUpdater {
        private Version currentVersion;
        private string modDir;
        private String downloadDir;
        private String zipPath;
        private String responseJson;
        private String latestVersion;
        private String downloadUrl;
        private String patchNotes;

        public ModUpdater(ModUpdateInfo modInfo) {
            modDir = modInfo.getModDir();
            currentVersion = modInfo.getVersion();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(modInfo.getUrl());

            // specify API version to use for stability
            request.Accept = "application/vnd.github.v3.raw+json";
            request.UserAgent = "pvutov/WargameModManagerMods";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                responseJson = reader.ReadToEnd();
            }

            latestVersion = getStringJsonField("tag_name", responseJson);
            patchNotes = getStringJsonField("body", responseJson);
            downloadUrl = getStringJsonField("browser_download_url", responseJson);

            DirectoryInfo di = Directory.CreateDirectory(Path.GetTempPath() + "WargameModManagerMods");
            downloadDir = Path.Combine(Path.GetTempPath(), "WargameModManagerMods");
            zipPath = Path.Combine(downloadDir, "WargameModManagerUpdate.zip");

            // make sure download dir is empty
            foreach (FileInfo file in di.GetFiles()) {
                file.Delete();
            }
        }

        private string getStringJsonField(string field, string json) {
            string formattedFieldName = "\"" + field + "\":";

            string result = "";

            try {
                result = json.Substring(json.IndexOf(formattedFieldName) + formattedFieldName.Length);

                result = result.Split('"')[1];
            }
            catch (Exception e) when (e is ArgumentOutOfRangeException || e is IndexOutOfRangeException) {
                Program.warning("JSON field not found.");
            }

            return result;
        }

        public bool checkForUpdates() {
            Version latestVer;
            try {
                latestVer = new Version(this.latestVersion);
            }
            catch (FormatException) {
                Program.warning("Version of latest git release could not be parsed.");
                return false;
            }

            return latestVer > currentVersion;
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
                    Directory.Delete(modDir, true);
                    Directory.CreateDirectory(modDir);
                    PathFinder.directoryCopy(downloadDir, modDir, true);
                    Directory.Delete(downloadDir, true);
                };

                wc.DownloadFileAsync(new Uri(downloadUrl), zipPath);
            }
        }

        public string getPatchNotes() {
            return patchNotes;
        }
    }
}
