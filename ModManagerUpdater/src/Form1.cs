using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Updater {
    public partial class Form1 : Form {
        private String programDir;
        public Form1(String programDir, String patchNotes) {
            InitializeComponent();
            this.programDir = programDir;
            if (programDir.Last() != '\\') {
                this.programDir += '\\';
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
                        File.Copy(f, programDir + Path.GetFileName(f), true);
                    }
                    catch (IOException) {
                        Program.warning("Could not write to file " + f 
                            + "\nMaybe it is in use?" );
                    }
                }
            }

            if (File.Exists(programDir + "WargameModManager.exe")) {
                Process.Start(programDir + "WargameModManager.exe");
            }
            Application.Exit();
        }
    }
}
