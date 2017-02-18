namespace WargameModManager {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.launchButton = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.modButtonsGroup = new System.Windows.Forms.GroupBox();
            this.modsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.modButtonsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // launchButton
            // 
            this.launchButton.Location = new System.Drawing.Point(12, 196);
            this.launchButton.Name = "launchButton";
            this.launchButton.Size = new System.Drawing.Size(127, 23);
            this.launchButton.TabIndex = 1;
            this.launchButton.Text = "Launch Wargame";
            this.launchButton.UseVisualStyleBackColor = true;
            this.launchButton.Click += new System.EventHandler(this.launchButton_Click);
            // 
            // updateButton
            // 
            this.updateButton.Location = new System.Drawing.Point(145, 196);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(127, 23);
            this.updateButton.TabIndex = 2;
            this.updateButton.Text = "Check for updates";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // modButtonsGroup
            // 
            this.modButtonsGroup.Controls.Add(this.modsLayoutPanel);
            this.modButtonsGroup.Location = new System.Drawing.Point(12, 12);
            this.modButtonsGroup.Name = "modButtonsGroup";
            this.modButtonsGroup.Padding = new System.Windows.Forms.Padding(0);
            this.modButtonsGroup.Size = new System.Drawing.Size(200, 178);
            this.modButtonsGroup.TabIndex = 3;
            this.modButtonsGroup.TabStop = false;
            this.modButtonsGroup.Text = "Mods available:";
            // 
            // modsLayoutPanel
            // 
            this.modsLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.modsLayoutPanel.Location = new System.Drawing.Point(7, 20);
            this.modsLayoutPanel.Name = "modsLayoutPanel";
            this.modsLayoutPanel.Size = new System.Drawing.Size(187, 152);
            this.modsLayoutPanel.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 231);
            this.Controls.Add(this.modButtonsGroup);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.launchButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Wargame Mod Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.modButtonsGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button launchButton;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.GroupBox modButtonsGroup;
        private System.Windows.Forms.FlowLayoutPanel modsLayoutPanel;
    }
}

