using System;
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
            HttpWebRequest request = null;
            try {
                request = (HttpWebRequest)WebRequest.Create(modInfo.getUrl());
            }
            catch (UriFormatException e) {
                Program.warning("A mod can't be updated because releaseRepo is invalid. To fix this, open "
                    + Path.Combine(modDir, "UPDATE") 
                    + " and delete or adjust the releaseRepo line."
                    + Environment.NewLine + Environment.NewLine + "Detailed Exception: " 
                    + Environment.NewLine + e.ToString());

                
                
                // Allow checkForUpdates() to do nothing:
                latestVersion = currentVersion.ToString();
                return;
            }

            // specify API version to use for stability
            request.Accept = "application/vnd.github.v3.raw+json";
            request.UserAgent = "pvutov/WargameModManagerMods";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                responseJson = reader.ReadToEnd();
            }

            latestVersion = Utility.getStringJsonField("tag_name", responseJson);
            patchNotes = Utility.getStringJsonField("body", responseJson);
            downloadUrl = Utility.getStringJsonField("browser_download_url", responseJson);

            
            downloadDir = Path.Combine(Path.GetTempPath(), "WargameModManagerMods");
            DirectoryInfo di = new DirectoryInfo(downloadDir);
            // remove old files if program didn't clean up last time
            if (di.Exists) {
                di.Delete(true);
            }
            di.Create();

            zipPath = Path.Combine(downloadDir, "WargameModManagerUpdate.zip");

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

        /// <summary>
        /// Download and install the update.
        /// </summary>
        /// <param name="reportProgress"> A callback for reporting download progress. </param>
        /// <param name="done"> A callback used to report when the entire update process has completed. </param>
        public void applyUpdate(Action<int> reportProgress, Action done) {
            using (WebClient wc = new WebClient()) {
                wc.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                    reportProgress(e.ProgressPercentage);
                };

                // after download
                wc.DownloadFileCompleted += delegate (object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, downloadDir);
                    File.Delete(zipPath);
                    DeleteDirectory(modDir);
                    Directory.CreateDirectory(modDir);
                    PathFinder.directoryCopy(downloadDir, modDir, true);
                    DeleteDirectory(downloadDir);
                    done();
                };

                wc.DownloadFileAsync(new Uri(downloadUrl), zipPath);
            }
        }

        public string getPatchNotes() {
            return patchNotes;
        }

        /// <summary>
        /// Directory.Delete(path, true) fails if a subdirectory
        /// is open in windows explorer, because the file handles
        /// are not released fast enough. This is a fix for that problem.
        /// </summary>
        public static void DeleteDirectory(string path) {
            foreach (string directory in Directory.GetDirectories(path)) {
                DeleteDirectory(directory);
            }

            try {
                Directory.Delete(path, true);
            }
            catch (IOException) {
                System.Threading.Thread.Sleep(100);
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException) {
                System.Threading.Thread.Sleep(100);
                Directory.Delete(path, true);
            }
        }
    }
}
