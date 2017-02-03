using System;
using System.Windows.Forms;

namespace WargameModManager {
    public static class Program {
        // Where to report bugs?
        public const string CONTACT_STRING = "\nTo report, PM throwaway on forums.eugensystems.com";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String [] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args));
        }

        /// <summary>
        ///  Display a warning string in cases of unexpected behavior.
        /// </summary>
        public static void warning(String warningText) {
            MessageBox.Show(warningText + Program.CONTACT_STRING, "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
