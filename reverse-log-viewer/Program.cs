using System;
using System.Windows.Forms;

namespace ReverseLogViewer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show("No log file provided.\n\nThis program must be launched by opening a .rlog file.",
                                "Reverse Log Viewer",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args[0]));
        }
    }
}
