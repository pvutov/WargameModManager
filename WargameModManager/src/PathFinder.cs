using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WargameModManager {
    class PathFinder {
        private String ini_path = AppDomain.CurrentDomain.BaseDirectory + "settings.ini";
        private readonly String WARGAME_ID_FOLDER = "251060";
        private string wargameDir;
        private bool _autoUpdate = false;
        public bool autoUpdate {
            get { return _autoUpdate; }
        }

        private bool _manageProfiles = false;
        public bool manageProfiles {
            get { return _manageProfiles; }
        }

        private DirectoryInfo _steamUsersDir;

        /// <summary>
        /// The folder all wargame updates go into. 
        /// Probably ..DirOfExe/Data/WARGAME/PC
        /// </summary>
        private string searchDir;

        public PathFinder() {

            if (File.Exists(ini_path)) {
                if (!tryReadIni(out wargameDir)) {
                    wargameDir = askUserForWargameDir();
                    _manageProfiles = askUserWhetherToManageProfiles();
                    createIni();
                }
            }
            else {
                wargameDir = askUserForWargameDir();
                _manageProfiles = askUserWhetherToManageProfiles();
                createIni();
            }

            searchDir = Path.Combine(wargameDir, "Data", "WARGAME", "PC");

            // Get steam users dir from the registry
            if (_manageProfiles) {
                RegistryKey regKey = Registry.CurrentUser;
                regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

                String steamDir = regKey.GetValue("SteamPath").ToString();
                _steamUsersDir = new DirectoryInfo(Path.Combine(steamDir, "userdata"));
            }
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
                    return folderPath;
                }

                // Files not found, try again..
                return askUserForWargameDir(exe + " not found. \n");
            }
        }

        /// <summary>
        /// Show the user a prompt explaining profile management and ask whether to enable it.
        /// </summary>
        /// <returns></returns>
        public bool askUserWhetherToManageProfiles() {
            String text = "Your wargame profile contains your level, your winrate,"
                + " and your decks. Do you want to use mod-specific profiles?"
                + Environment.NewLine + "If you enable this, "
                + "your current profile will be copied, but any future changes to your decks will only show up when playing the mod you made them in."
                + Environment.NewLine + "(If you're not sure, I recommend 'yes')";
            return DialogResult.Yes == MessageBox.Show(text, "Question",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Write settings to file.
        /// </summary>
        private void createIni() {
            // Save dir for next time:
            string[] lines = { "dir:" + wargameDir,
                // dont autoupdate on first run, but set it for future
                "autoupdate:true",
                "manageprofiles:" + _manageProfiles.ToString().ToLower()
            };
            File.WriteAllLines(ini_path, lines);
        }

        private bool tryReadIni(out string result) {
            result = "";

            string[] lines = File.ReadAllLines(ini_path);
            foreach (string line in lines) {
                if (line == "autoupdate:true") {
                    _autoUpdate = true;
                }

                if (line == "manageprofiles:true") {
                    _manageProfiles = true;
                }

                if (line.StartsWith("dir:")) {
                    result = line.Substring("dir:".Length);
                }
            }

            return result != "";
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

        private String findNewestFolder(String filename) {
            string result = null;
            DirectoryInfo di = new DirectoryInfo(searchDir);

            // Order from newest
            var ordered = di.GetDirectories().OrderByDescending(x => x.Name);
            foreach (DirectoryInfo innerDi in ordered) {
                if (tryFindNewestFolderRecursively(filename, innerDi, out result)) {
                    return result;
                }
            }

            if (result == null) {
                Program.warning("File not found: " + filename);
            }
            return result;
        }

        private bool tryFindNewestFolderRecursively(String filename, DirectoryInfo di, out string result) {
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
                if (tryFindNewestFolderRecursively(filename, innerDi, out result)) {
                    return true;
                }
            }

            return false;
        }

        public String[] getModList() {
            return Directory.GetDirectories(getModsDir())
                .Select(d => new DirectoryInfo(d).Name).ToArray();
        }

        /// <summary>
        /// Place the game files of the selected mod in the game dir, and change the player profile if profile management is enabled.
        /// </summary>
        /// <param name="modName"></param>
        public void activateMod(String modName) {
            // files
            if (modName != "vanilla") {
                string modDir = getModFilesDir(modName);

                foreach (string versionFolder in Directory.GetDirectories(modDir)) {
                    string versionFolderShort = new DirectoryInfo(versionFolder).Name;
                    string src = Path.Combine(modDir, versionFolderShort);
                    string dst = Path.Combine(searchDir, versionFolderShort);
                    string vanillaDir = Path.Combine(getModsDir(), "vanilla");

                    directoryCopyWithVanillaBackup(src, dst, vanillaDir, true);
                }

                foreach (string modFile in Directory.GetFiles(modDir)) {
                    string wrdFile = findNewestFolder(Path.GetFileName(modFile));

                    saveVanillaFile(wrdFile);

                    File.Copy(modFile, wrdFile, true);
                }
            }
            else {
                string vanillaDir = Path.Combine(getModsDir(), "vanilla");
                directoryCopy(vanillaDir, searchDir, true);
            }

            // profiles
            if (_manageProfiles) {
                foreach (DirectoryInfo userFolder in _steamUsersDir.GetDirectories()) {
                    DirectoryInfo wargameProfileDir = new DirectoryInfo(Path.Combine(userFolder.FullName, WARGAME_ID_FOLDER, "remote"));
                    if (wargameProfileDir.Exists) {
                        String extension = ".wargameprofile";
                        String activeProfile = Path.Combine(wargameProfileDir.FullName, "PROFILE" + extension);

                        // On first launch, create backup dir and save vanilla profile
                        String backupDirName = "modmanager";
                        String backupDir = Path.Combine(wargameProfileDir.FullName, backupDirName);
                        if (!Directory.Exists(backupDir)) {
                            Directory.CreateDirectory(backupDir);
                        }

                        // On first mod launch, create a profile for that mod from the last profile used. 
                        String modProfile = Path.Combine(backupDir, modName + extension);
                        if (!File.Exists(modProfile)) {
                            File.Copy(activeProfile, modProfile, false);
                        }

                        // Find out what mod was used in the previous launch
                        String lastModUsedFile = Path.Combine(backupDir, "LAST");
                        String lastModUsed = "vanilla";
                        if (File.Exists(lastModUsedFile)) {
                            lastModUsed = File.ReadAllLines(lastModUsedFile)[0];
                        }

                        // Save user progress from the previous session
                        String lastModUsedProfile = Path.Combine(backupDir, lastModUsed + extension);
                        File.Copy(activeProfile, lastModUsedProfile, true);

                        // Write what mod this session used, and place the appropriate profile.
                        File.WriteAllText(lastModUsedFile, modName);
                        File.Copy(modProfile, activeProfile, true);
                    }
                }
            }
        }

        private String getModFilesDir(String modName) {
            return Path.Combine(getModsDir(), modName, "bin");
        }

        // source: https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        public static void directoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
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

        public void directoryCopyWithVanillaBackup(string sourceDirName, string destDirName, string vanillaDirName, bool copySubDirs) {
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
                
                saveVanillaFile(temppath);

                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    string vanPath = Path.Combine(vanillaDirName, subdir.Name);
                    directoryCopyWithVanillaBackup(subdir.FullName, temppath, vanPath, copySubDirs);
                }
            }
        }

        public ModUpdateInfo[] getModUpdateInfo () {
            List<ModUpdateInfo> result = new List<ModUpdateInfo>();
            string mods = getModsDir();

            foreach (string mod in Directory.GetDirectories(mods)) {
                string updateFile = Path.Combine(mod, "UPDATE");
                if (File.Exists(updateFile)) {
                    string version = null;
                    string url = null;

                    foreach (string line in File.ReadAllLines(updateFile)) {
                        if (line.StartsWith("releaseRepo:")) {
                            url = line.Substring("releaseRepo:".Length);
                        } else if (line.StartsWith("version:")) {
                            version = line.Substring("version:".Length);
                        }
                    }

                    if (version != null && url != null) {
                        result.Add(new ModUpdateInfo(new DirectoryInfo(mod).Name, version, url, mod));
                    }
                }
            }

            return result.ToArray();
        }
    }
}
