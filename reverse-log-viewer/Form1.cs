using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ReverseLogViewer
{
    public partial class Form1 : Form
    {
        private readonly string _logPath;
        private readonly FileSystemWatcher _watcher;
        private readonly Timer _refreshTimer;

        public Form1(string logPath)
        {
            InitializeComponent();

            _logPath = logPath;
            Text = $"Reverse Log Viewer - {Path.GetFileName(logPath)}";

            // Auto-refresh timer (every 2 seconds)
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 2000;
            _refreshTimer.Tick += (s, e) => LoadLog();
            _refreshTimer.Start();

            // File watcher for instant refresh
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(logPath))
            {
                Filter = Path.GetFileName(logPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Changed += (s, e) => LoadLog();
            _watcher.EnableRaisingEvents = true;

            LoadLog();
        }

        private void LoadLog()
        {
            try
            {
                if (!File.Exists(_logPath))
                {
                    textBox1.Text = "(Log file not found)";
                    return;
                }

                // Read lines safely even if file is locked
                string[] lines;
                using (var fs = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    lines = sr.ReadToEnd()
                              .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                }

                // Reverse order (newest first)
                var reversed = lines.Reverse().ToArray();

                // Only update UI if changed (prevents flicker)
                string newText = string.Join(Environment.NewLine, reversed);
                if (textBox1.Text != newText)
                    textBox1.Text = newText;
            }
            catch (Exception ex)
            {
                textBox1.Text = $"Error reading log:\r\n{ex.Message}";
            }
        }
    }
}
