using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using file_mover_log;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

namespace ReverseLogViewer
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref POINT lParam);


        private const int EM_GETSCROLLPOS = 0x0400 + 221;
        private const int EM_SETSCROLLPOS = 0x0400 + 222;

        private string _logPath;
        private FileSystemWatcher _watcher;
        private readonly Timer _refreshTimer;
        private readonly Timer _debounceTimer = new Timer();
        private bool _pendingRefresh = false;
        private Font _boldRichTextBoxFont;



        public DataFilter FilterData { get; set; } = new DataFilter();

        private POINT GetScrollPos()
        {
            POINT p = new POINT();
            SendMessage(richTextBox1.Handle, EM_GETSCROLLPOS, 0, ref p);
            return p;
        }

        private void SetScrollPos(POINT p)
        {
            SendMessage(richTextBox1.Handle, EM_SETSCROLLPOS, 0, ref p);
        }

        private LogEntryType SelectedFilter
            => (LogEntryType)ctlFilterCombo.SelectedValue;

        private IEnumerable<LogEntry> ApplyFilter(IEnumerable<LogEntry> entries)
        {
            if (SelectedFilter == LogEntryType.All)
            {
                return entries;
            }

            return entries.Where(e => e.EntryType == SelectedFilter);
        }

        private void AppendColoredLine(LogEntry entry)
        {
            // 1. Determine color for the EntryType label
            Color typeColor;

            switch (entry.EntryType)
            {
                case LogEntryType.Moved:
                    typeColor = Color.MediumSeaGreen;
                    break;

                case LogEntryType.MovedVersioned:
                    typeColor = Color.OliveDrab;
                    break;

                case LogEntryType.Warning:
                    typeColor = Color.Goldenrod;
                    break;

                case LogEntryType.Error:
                    typeColor = Color.IndianRed;
                    break;

                case LogEntryType.InternalError:
                    typeColor = Color.HotPink;
                    break;

                case LogEntryType.Undefined:
                    typeColor = Color.DimGray;
                    break;

                default:
                    typeColor = Color.Black;
                    break;
            }

            string src = ShortenPath(entry.Source);
            string dst = ShortenPath(entry.Destination);

            BeginEntryBlock();

            // 2. Timestamp (neutral)
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.SelectionFont = richTextBox1.Font;
            richTextBox1.AppendText($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}  ");

            // 3. EntryType (colored)
            richTextBox1.SelectionColor = typeColor;
            richTextBox1.SelectionFont = richTextBox1.Font;
            richTextBox1.AppendText($"{entry.EntryType}  ");

            // 4. Source path (bold)
            if (!string.IsNullOrWhiteSpace(entry.Source))
            {
                richTextBox1.SelectionColor = Color.DarkGray;
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.AppendText("From '");

                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.SelectionFont = _boldRichTextBoxFont;
                richTextBox1.AppendText(src);

                richTextBox1.SelectionColor = Color.DarkGray;
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.AppendText("' ");
            }

            // 5. Destination path (bold)
            if (!string.IsNullOrWhiteSpace(entry.Destination))
            {
                richTextBox1.SelectionColor = Color.DarkGray;
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.AppendText("To '");

                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.SelectionFont = _boldRichTextBoxFont;
                richTextBox1.AppendText(dst);

                richTextBox1.SelectionColor = Color.DarkGray;
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.AppendText("' ");
            }

            // 6. Message (neutral or error-highlighted)
            if (!string.IsNullOrWhiteSpace(entry.Message))
            {
                bool isError = entry.EntryType == LogEntryType.Error ||
                               entry.EntryType == LogEntryType.InternalError;

                richTextBox1.SelectionColor = isError ? Color.IndianRed : Color.Black;
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.AppendText(entry.Message);
            }

            // 7. Exception (always red)
            if (!string.IsNullOrWhiteSpace(entry.Exception))
            {
                richTextBox1.SelectionColor = Color.IndianRed;
                richTextBox1.SelectionFont = richTextBox1.Font;
                richTextBox1.AppendText("  EX: " + entry.Exception);
            }

            // 8. End the line
            richTextBox1.AppendText(Environment.NewLine);

            // Reset formatting
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            richTextBox1.SelectionFont = richTextBox1.Font;
        }

        public Form1() : this(null)
        {
        }


        public Form1(string logPath)
        {
            InitializeComponent();
            
            bool isDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (!isDesignMode)
            {
                ctlFilterCombo.DataSource = FilterData.Filters;
                ctlFilterCombo.DisplayMember = "Key";
                ctlFilterCombo.ValueMember = "Value";
                ctlFilterCombo.SelectedIndex = 0;
                ctlFilterCombo.SelectedIndexChanged += (s, e) => LoadLog();

                _logPath = logPath;
                _boldRichTextBoxFont = new Font(richTextBox1.Font, FontStyle.Bold);

                // Auto-refresh timer (every 2 seconds)
                _refreshTimer = new Timer
                {
                    Interval = 2000
                };
                _refreshTimer.Tick += (s, e) => LoadLog();
                _refreshTimer.Start();
                _refreshTimer.Enabled = false;

                _debounceTimer.Interval = 200; // 200ms debounce
                _debounceTimer.Tick += (s, e) =>
                {
                    _debounceTimer.Stop();
                    if (_pendingRefresh)
                    {
                        _pendingRefresh = false;
                        LoadLog();
                    }
                };


                // Only create FileSystemWatcher if a valid file was passed
                if (!string.IsNullOrEmpty(_logPath) && File.Exists(_logPath))
                {
                    WireUpRefreshWatcher();
                    // Load initial content
                    LoadLog();
                }
                else
                {
                    // No file → viewer starts empty
                    richTextBox1.Clear();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _watcher?.Dispose();
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();
        }

        private void WireUpRefreshWatcher()
        {
            Text = $"Reverse Log Viewer - {ShortenPath(_logPath)}";

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_logPath))
            {
                Filter = Path.GetFileName(_logPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Changed += (s, e) =>
            {
                _pendingRefresh = true;
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };

            _watcher.EnableRaisingEvents = true;
        }

        private IEnumerable<LogEntry> ParseJsonLines(string[] lines)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                LogEntry entry;

                try
                {
                    entry = LogEntry.FromJson(line);
                }
                catch
                {
                    entry = new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        EntryType = LogEntryType.InternalError,
                        Message = "Malformed JSON log entry",
                        Exception = line
                    };
                }

                if (entry != null)
                {
                    yield return entry;
                }
            }
        }
              
        private int GetDistanceFromBottom(int visibleLineCount)
        {
            int totalLines = richTextBox1.GetLineFromCharIndex(
                richTextBox1.TextLength) + 1;
            int firstVisibleLine = richTextBox1.GetLineFromCharIndex(
                richTextBox1.GetCharIndexFromPosition(new Point(0, 0)));
            return Math.Max(0, totalLines - firstVisibleLine - visibleLineCount);
        }


        private void LoadLog()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)LoadLog);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(_logPath) || !File.Exists(_logPath))
                {
                    richTextBox1.Text = "(Log file not found)";
                    return;
                }

                // Read file safely
                string[] lines;
                using (var fs = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    lines = sr.ReadToEnd()
                              .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                }

                // Parse JSON
                var entries = ParseJsonLines(lines).Reverse();

                // Apply filter
                entries = ApplyFilter(entries);

                int visibleLines = richTextBox1.ClientSize.Height / richTextBox1.Font.Height;

                // Capture scroll distance from bottom BEFORE clearing
                int distanceFromBottom = GetDistanceFromBottom(visibleLines);

                bool preserve = chkPreserveScrollLocation.Checked;

                // Clear + redraw
                richTextBox1.Clear();

                foreach (var entry in entries)
                {
                    AppendColoredLine(entry);
                }

                // Compute new line metrics AFTER redraw
                int newTotalLines = richTextBox1.GetLineFromCharIndex(
                    richTextBox1.TextLength) + 1;

                // Determine scroll position
                if (newTotalLines <= visibleLines)
                {
                    // All records fit on screen → show from top
                    richTextBox1.SelectionStart = 0;
                    richTextBox1.ScrollToCaret();
                    SetScrollPos(new POINT() { X = 0, Y = 0 });
                }
                else if (preserve && distanceFromBottom > 0)
                {
                    // Preserve distance from bottom (for file updates)
                    int newFirstVisibleLine = Math.Max(0, newTotalLines - visibleLines - distanceFromBottom);
                    int charIndex = richTextBox1.GetFirstCharIndexFromLine(newFirstVisibleLine);
                    richTextBox1.SelectionStart = charIndex;
                    richTextBox1.ScrollToCaret();
                }
                else
                {
                    // Default: scroll to top
                    richTextBox1.SelectionStart = 0;
                    richTextBox1.ScrollToCaret();
                    SetScrollPos(new POINT() { X = 0, Y = 0 });
                }
            }
            catch (Exception ex)
            {
                richTextBox1.Text = $"Error reading log:\r\n{ex.Message}";
            }
        }

        private bool _alternate = false;

        private void BeginEntryBlock()
        {
            _alternate = !_alternate;
            richTextBox1.SelectionBackColor = _alternate
                ? Color.FromArgb(245, 245, 245)   // light gray
                : Color.White;
        }

        private string ShortenPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            return chkShowFullPaths.Checked
                ? path
                : Path.GetFileName(path);
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Reverse Log (*.rlog)|*.rlog|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _logPath = dlg.FileName;
                    if (!string.IsNullOrEmpty(_logPath) && File.Exists(_logPath))
                    {
                        WireUpRefreshWatcher();
                        // Load initial content
                        LoadLog();
                        SetScrollPos(new POINT() { X = 0, Y = 0 });
                    }
                    else
                    {
                        // No file → viewer starts empty
                        richTextBox1.Clear();
                    }
                }
            }
        }

        private void chkShowFullPaths_CheckedChanged(object sender, EventArgs e)
        {
            Text = ShortenPath(_logPath);
            LoadLog();
        }
    }
}
