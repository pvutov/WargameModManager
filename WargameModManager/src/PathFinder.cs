using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WargameModManager {
    class PathFinder {
        private String ini_path = AppDomain.CurrentDomain.BaseDirectory + "settings.ini";
        private string wargameDir;
        private string searchDir;

        public PathFinder() {

            if (File.Exists(ini_path)) {
                if (!tryReadIni(out wargameDir)) {
                    wargameDir = askUserForWargameDir();
                }
            }
            else {
                wargameDir = askUserForWargameDir();
            }

            searchDir = Path.Combine(wargameDir, "Data", "WARGAME", "PC");
        }

        private string askUserForWargameDir() {
            return askUserForWargameDir("");
        }

        public string askUserForWargameDir(string error) {
            DialogResult res;
            String folderPath;

            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                fbd.Description = error + "Where is your wargame folder?"
                    + " The path will be something like \n"
                    + @"C:\SteamLibrary\SteamApps\common\Wargame Red Dragon";
                fbd.ShowNewFolderButton = false;
                res = fbd.ShowDialog();
                folderPath = fbd.SelectedPath;
            }

            if (res != DialogResult.OK) {
                Application.Exit();
                Environment.Exit(0);
                return "";
            }
            else {
                String exe = Path.Combine(folderPath, "Wargame3.exe");

                if (File.Exists(exe)) {

                    // Save dir for next time:
                    string[] lines = { "dir:" + folderPath };
                    File.WriteAllLines(ini_path, lines);

                    return folderPath;
                }

                // Files not found, try again..
                return askUserForWargameDir(exe + " not found. \n");
            }
        }

        private bool tryReadIni(out string result) {

            string[] lines = System.IO.File.ReadAllLines(ini_path);
            foreach (string line in lines) {
                if (line.StartsWith("dir:")) {
                    result = line.Substring("dir:".Length);
                    return true;
                }
            }

            result = "";
            return false;
        }

        public String getWargameDir() {
            return wargameDir;
        }

        public String getModsDir() {
            return Path.Combine(getThisDir(), "mods");
        }

        public String getThisDir() {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        /// <summary>
        /// The vanilla folder stores the original wargame files that are affected by modding.
        /// For a file that's about to be replaced by a modded version,
        /// check if it has already been backed up in the vanilla folder
        /// and back it up if not.
        /// </summary>
        /// <param name="fileBeingReplaced"></param>
        public void saveVanillaFile(String fileBeingReplaced) {
            string vanillaDir = Path.Combine(getModsDir(), "vanilla");
            string suffix = fileBeingReplaced.Substring(searchDir.Length + Path.PathSeparator.ToString().Length);
            string vanillaFile = Path.Combine(vanillaDir, suffix);

            if (!File.Exists(vanillaFile)) {
                Directory.CreateDirectory(Path.GetDirectoryName(vanillaFile));
                try {
                    File.Copy(fileBeingReplaced, vanillaFile, false);
                }
                catch (IOException) {
                    Program.warning(vanillaFile + " exists even though we established that it doesn't exist."
                        + " This should never happen.");
                }
            }
        }

        public String getRealExe() {
            return Path.Combine(getWargameDir(), "Wargame3.exe");
        }

        private String findNewest(String filename) {
            // TODO
            string result = null;
            DirectoryInfo di = new DirectoryInfo(searchDir);

            // Order from newest
            var ordered = di.GetDirectories().OrderByDescending(x => x.Name);
            foreach (DirectoryInfo innerDi in ordered) {
                if (tryFindNewestRecursive(filename, innerDi, out result)) {
                    return result;
                }
            }

            if (result == null) {
                Program.warning("File not found: " + filename);
            }
            return result;
        }

        private bool tryFindNewestRecursive(String filename, DirectoryInfo di, out string result) {
            result = null;
            string possibleFilePath = Path.Combine(di.FullName, filename);

            // If a file is found, we're done.
            if (File.Exists(possibleFilePath)) {
                result = possibleFilePath;
                return true;
            }

            // If a file doesn't exist in this directory, recurse on subdirs:
            var ordered = di.GetDirectories().OrderByDescending(x => x.Name);
            foreach (DirectoryInfo innerDi in ordered) {
                if (tryFindNewestRecursive(filename, innerDi, out result)) {
                    return true;
                }
            }

            return false;
        }

        public String[] getModList() {
            return Directory.GetDirectories(getModsDir())
                .Select(d => new DirectoryInfo(d).Name).ToArray();
        }

        public void activateMod(String modName) {
            if (modName != "vanilla") {
                foreach (string modFile in Directory.GetFiles(getModFilesDir(modName))) {
                    string wrdFile = findNewest(Path.GetFileName(modFile));

                    saveVanillaFile(wrdFile);

                    File.Copy(modFile, wrdFile, true);
                }
            }
            else {
                string vanillaDir = Path.Combine(getModsDir(), "vanilla");
                directoryCopy(vanillaDir, searchDir, true);
            }
        }

        private String getModFilesDir(String modName) {
            return Path.Combine(getModsDir(), modName, "bin");
        }

        //private String getModUpdateFile(String modName) {
        //    return Path.Combine(getModsDir(), modName, "UPDATE");
        //}

        // source: https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void directoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    directoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
