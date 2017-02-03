using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace WargameModManager {
    class ModManagerUpdater {
        private String api_url;
        private String downloadDir;
        private String zipPath;
        private String responseJson;
        private String latestVersion;
        private String downloadUrl;
        private String patchNotes;


        public ModManagerUpdater(String url) {
            api_url = url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(api_url);

            // specify API version to use for stability
            request.Accept = "application/vnd.github.v3.raw+json";
            request.UserAgent = "pvutov/WargameModManager";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                responseJson = reader.ReadToEnd();
            }

            latestVersion = getStringJsonField("tag_name", responseJson);
            patchNotes = getStringJsonField("body", responseJson);
            downloadUrl = getStringJsonField("browser_download_url", responseJson);

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
                        Process.Start(updaterDir,
                            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                            + " " + patchNotes);
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
