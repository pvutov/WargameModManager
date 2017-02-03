using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Updater {
    public partial class Form1 : Form {
        private String wargameDir;
        public Form1(String wargameDir, String patchNotes) {
            InitializeComponent();
            this.wargameDir = wargameDir;
            if (wargameDir.Last() != '\\') {
                this.wargameDir += '\\';
            }
            changeListLabel.Text = patchNotes.Replace(@"\r\n", Environment.NewLine);
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void updateButton_Click(object sender, EventArgs e) {
            String thisFile = Assembly.GetEntryAssembly().Location;
            foreach (String f in Directory.GetFiles(Path.GetDirectoryName(thisFile))) {
                if (f != thisFile) {
                    try {
                        File.Copy(f, wargameDir + Path.GetFileName(f), true);
                    }
                    catch (IOException) {
                        Program.warning("Could not write to file " + f 
                            + "\nMaybe it is in use?" );
                    }
                }
            }

            if (File.Exists(wargameDir + "Wargame3.exe")) {
                Process.Start(wargameDir + "Wargame3.exe");
            }
            Application.Exit();
        }
    }
}
