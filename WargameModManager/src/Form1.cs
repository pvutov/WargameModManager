﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WargameModManager {
    public partial class Form1 : Form {
        private String[] args;
        private PathFinder directories;

        public Form1(String[] args) {
            this.args = args;

            InitializeComponent();
            this.directories = new PathFinder();

            // Generate the mod selection buttons
            foreach (string modName in directories.getModList()) {
                RadioButton button = new RadioButton();
                button.Text = modName;

                button.Padding = new Padding(0);
                button.Size = new Size(button.Size.Width, 20);
                modsLayoutPanel.Controls.Add(button);
            }
        }

        private void launchButton_Click(object sender, EventArgs e) {
            // Swap in mods
            var selectedButton = modsLayoutPanel.Controls.OfType<RadioButton>()
                                      .FirstOrDefault(r => r.Checked);
            directories.activateMod(selectedButton.Text);

            // Start wargame
            var processStartInfo = new ProcessStartInfo {
                FileName = directories.getRealExe(),
                UseShellExecute = false,
                Arguments = String.Join(" ", args)
            };
            try {
                Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception w) {
                Program.warning(directories.getRealExe() + " not found." + w.Message + w.ErrorCode.ToString() + w.NativeErrorCode.ToString());
            }

            Application.Exit();
        }

        private void updateButton_Click(object sender, EventArgs e) {

            // CHECK FOR MOD UPDATES

            ModUpdateInfo[] infos = directories.getModUpdateInfo();
            foreach (ModUpdateInfo info in infos) {
                ModUpdater modUpdater = new ModUpdater(info);

                if (modUpdater.checkForUpdates()) {
                    // update
                    ProgressBar progressBar = new ProgressBar();
                    progressBar.Name = "downloadProgressBar";
                    progressBar.Maximum = 100;
                    progressBar.Scale(new SizeF(0.7f, 1f));

                    Panel container = new Panel();
                    container.Location = updateButton.Location;
                    container.Controls.Add(progressBar);

                    this.Controls.Remove(updateButton);
                    this.Controls.Add(container);
                    
                    string caption = "New version of " + info.getName() + " found!";
                    string message = "Changelog: \n" + modUpdater.getPatchNotes() + "\n\nApply update?'";

                    DialogResult decision = MessageBox.Show(message, caption,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (decision == DialogResult.Yes) {
                        modUpdater.applyUpdate((int val) => { progressBar.Value = val; });
                    }

                    // remove progress bar when it is done
                    var t = new Timer();
                    t.Interval = 2000;
                    t.Tick += (s, _) => {
                        if (progressBar.Value == progressBar.Maximum) {
                            this.Controls.Remove(container);
                            this.Controls.Add(updateButton);
                            t.Stop();
                        }
                    };
                    t.Start();
                }
            }


            // no mod updates were found, but mod manager can be updated
            ModManagerUpdater appUpdater = new ModManagerUpdater();

            if (appUpdater.checkForUpdates()) {
                // update
                ProgressBar progressBar = new ProgressBar();
                progressBar.Name = "downloadProgressBar";
                progressBar.Maximum = 100;
                progressBar.Scale(new SizeF(0.7f, 1f));

                Panel container = new Panel();
                container.Location = updateButton.Location;
                container.Controls.Add(progressBar);

                this.Controls.Remove(updateButton);
                this.Controls.Add(container);

                appUpdater.applyUpdate((int val) => { progressBar.Value = val; });

                // remove progress bar when it is done
                var t = new Timer();
                t.Interval = 10000;
                t.Tick += (s, _) => {
                    if (progressBar.Value == progressBar.Maximum) {
                        this.Controls.Remove(container);
                        this.Controls.Add(updateButton);
                        t.Stop();
                    }
                };
                t.Start();
            }
            else {
                // remove button, show and disappear text
                Label updateMessageLabel = new Label();
                updateMessageLabel.Name = "updateMessageLabel";
                updateMessageLabel.Text = "at newest version";
                updateMessageLabel.Location = updateButton.Location;
                updateMessageLabel.MaximumSize = new Size(updateButton.Size.Width, 0);
                updateMessageLabel.AutoSize = true;

                this.Controls.Remove(updateButton);
                this.Controls.Add(updateMessageLabel);

                var t = new Timer();
                t.Interval = 4000;
                t.Tick += (s, _) => {
                    this.Controls.Remove(updateMessageLabel);
                    t.Stop();
                };
                t.Start();
            }
        }
    }
}
