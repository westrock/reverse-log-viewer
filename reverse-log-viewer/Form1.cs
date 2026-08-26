using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace reverse_log_viewer
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref POINT lParam);

        private const int EM_GETSCROLLPOS = 0x0400 + 221;
        private const int EM_SETSCROLLPOS = 0x0400 + 222;

        private bool _fileDirty = true;
        private bool _filterDirty = true;
        private bool _displayDirty = true;
        private bool _alternate = false;
        private List<LogEntry> _allEntries = new List<LogEntry>();
        private List<LogEntry> _filteredEntries = new List<LogEntry>();
        private string _logPath;
        private FileSystemWatcher _watcher;
        private readonly Timer _refreshTimer;
        private readonly Timer _debounceTimer = new Timer();
        private bool _pendingRefresh = false;
        private Font _boldRichTextBoxFont;



        public DataFilter FilterData { get; set; } = new DataFilter();

        #region TextBox and Scrolling Helpers

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
        {
            get
            {
                return (ctlFilterCombo.SelectedValue == null)
                    ? LogEntryType.All
                    : (LogEntryType)ctlFilterCombo.SelectedValue;
            }
        }


        /// <summary>
        /// Renders the filtered log entries in the RichTextBox control.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Calculates the number of visible lines based on the RichTextBox dimensions and font height.</description></item>
        /// <item><description>Determines the current scroll position distance from the bottom.</description></item>
        /// <item><description>Clears the RichTextBox and re-populates it with all filtered entries using color formatting.</description></item>
        /// <item><description>Restores or adjusts the scroll position based on the <see cref="chkPreserveScrollLocation"/> checkbox state.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Scroll position behavior:
        /// <list type="bullet">
        /// <item><description>If new content fits entirely on screen, scrolls to the top.</description></item>
        /// <item><description>If scroll preservation is enabled and content exceeds visible area, maintains the distance from bottom.</description></item>
        /// <item><description>Otherwise, defaults to scrolling to the top.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="AppendColoredLine"/>
        /// <seealso cref="GetDistanceFromBottom"/>
        /// <seealso cref="SetScrollPos"/>
        private void RenderEntries()
        {
            int visibleLines = richTextBox1.ClientSize.Height / richTextBox1.Font.Height;
            int distanceFromBottom = GetDistanceFromBottom(visibleLines);
            bool preserve = chkPreserveScrollLocation.Checked;

            richTextBox1.Clear();

            foreach (LogEntry entry in _filteredEntries)
                AppendColoredLine(entry);

            int newTotalLines = richTextBox1.GetLineFromCharIndex(richTextBox1.TextLength) + 1;

            if (newTotalLines <= visibleLines)
            {
                richTextBox1.SelectionStart = 0;
                richTextBox1.ScrollToCaret();
                SetScrollPos(new POINT() { X = 0, Y = 0 });
            }
            else if (preserve && distanceFromBottom > 0)
            {
                int newFirstVisibleLine = Math.Max(0, newTotalLines - visibleLines - distanceFromBottom);
                int charIndex = richTextBox1.GetFirstCharIndexFromLine(newFirstVisibleLine);
                richTextBox1.SelectionStart = charIndex;
                richTextBox1.ScrollToCaret();
            }
            else
            {
                richTextBox1.SelectionStart = 0;
                richTextBox1.ScrollToCaret();
                SetScrollPos(new POINT() { X = 0, Y = 0 });
            }
        }



        /// <summary>
        /// Appends a single log entry to the RichTextBox with appropriate coloring and formatting based on its type and content.
        /// </summary>
        /// <param name="entry">The log entry to append.</param>
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

        /// <summary>
        /// Filters a collection of log entries based on their type.
        /// </summary>
        /// <param name="entries">The collection of log entries to filter.</param>
        /// <param name="filterBy">The type of log entries to include in the result.</param>
        /// <returns>An enumerable collection of filtered log entries.</returns>
        private IEnumerable<LogEntry> FilterBy(IEnumerable<LogEntry> entries, LogEntryType filterBy)
        {
            if (filterBy == LogEntryType.All)
                return entries;

            return entries.Where(e => e.EntryType == filterBy);
        }


        /// <summary>
        /// Refreshes the list of filtered log entries.
        /// </summary>
        private void RefreshFilteredList()
        {
            _filteredEntries = FilterBy(_allEntries, SelectedFilter).ToList();
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1() : this(null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class with a specified log file path.
        /// </summary>
        /// <param name="logPath">The path to the log file to view.</param>
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

                _logPath = logPath;
                _boldRichTextBoxFont = new Font(richTextBox1.Font, FontStyle.Bold);

                // Auto-refresh timer (every 1 second).
                // - The Tick handler invokes `LoadLog()` to refresh the viewer.
                // - The timer is `Start()`ed and then immediately `Enabled = false` so that
                //   the timer's native resources are allocated up-front but automatic
                //   refreshing remains disabled until explicitly enabled elsewhere.
                _refreshTimer = new Timer
                {
                    Interval = 1000
                };
                _refreshTimer.Tick += (s, e) => LoadLog();
                _refreshTimer.Start();
                _refreshTimer.Enabled = false;

                // Debounce timer used to coalesce rapid filesystem events.
                // - Interval: 200ms debounce window.
                // - When filesystem events are received the code sets `_pendingRefresh = true`
                //   and starts this timer. When the timer fires it stops itself and only
                //   calls `LoadLog()` if a refresh is still pending, preventing multiple
                //   rapid reloads for bursty change events.
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
                    // Load initial content
                    LoadLog();
                    WireUpRefreshWatcher();
                }
                else
                {
                    // No file → viewer starts empty
                    richTextBox1.Clear();
                }
            }
        }


        /// <summary>
        /// Sets up a FileSystemWatcher to monitor changes to the log file. When the file is modified, it marks the display, filter, and file as "dirty"
        /// and starts a debounce timer to coalesce rapid change events. Once the debounce timer elapses, it triggers a refresh of the log viewer by
        /// calling `LoadLog()`. This ensures that the viewer stays up-to-date with the latest log entries without overwhelming the UI with too many
        /// refreshes during rapid file changes.
        /// </summary>
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
                _displayDirty = true;
                _filterDirty = true;
                _fileDirty = true;
                _pendingRefresh = true;

                // Marshal timer operations to the UI thread
                BeginInvoke((Action)(() =>
                {
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }));
            };

            _watcher.EnableRaisingEvents = true;
        }


        /// <summary>
        /// Calculates the distance (in lines) from the bottom of the RichTextBox to the first visible line.
        /// This is used to determine if the view is scrolled to the bottom for auto-scrolling logic.
        /// </summary>
        /// <param name="visibleLineCount">The number of lines currently visible in the control's viewport.</param>
        /// <returns>The number of lines between the first visible line and the end of the document; 0 if the bottom is visible.</returns>
        private int GetDistanceFromBottom(int visibleLineCount)
        {
            int totalLines = richTextBox1.GetLineFromCharIndex(
                richTextBox1.TextLength) + 1;
            int firstVisibleLine = richTextBox1.GetLineFromCharIndex(
                richTextBox1.GetCharIndexFromPosition(new Point(0, 0)));
            return Math.Max(0, totalLines - firstVisibleLine - visibleLineCount);
        }


        /// <summary>
        /// Loads the log file, applies filtering, and renders the entries. This method is designed
        /// to be safe to call from any thread, and will marshal calls to the UI thread as needed.
        /// It also optimizes performance by only reloading the file or reapplying filters when necessary,
        /// based on internal "dirty" flags.
        /// </summary>
        private void LoadLog()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)LoadLog);
                return;
            }

            try
            {
                // Reload file only if needed
                if (_fileDirty)
                {
                    _allEntries = LogEntryHelpers.LoadFile(_logPath, LogEntryHelpers.ParseJsonLines);
                    _fileDirty = false;
                    _filterDirty = _displayDirty = true;
                }

                // Rebuild filtered list only if needed
                if (_filterDirty)
                {
                    RefreshFilteredList();
                }

                if (_displayDirty || _filterDirty)
                {
                    RenderEntries();
                }

                _filterDirty = false;
                _displayDirty = false;
            }
            catch (Exception ex)
            {
                richTextBox1.Text = $"Error reading log:\r\n{ex.Message}";
            }
        }


        /// <summary>
        /// Begins a new entry block in the RichTextBox, alternating the background color for visual distinction.
        /// </summary>
        private void BeginEntryBlock()
        {
            _alternate = !_alternate;
            richTextBox1.SelectionBackColor = _alternate
                ? Color.FromArgb(245, 245, 245)   // light gray
                : Color.White;
        }


        /// <summary>
        /// Shortens a file path for display purposes, showing only the filename if the full path is not required.
        /// </summary>
        /// <param name="path">The file path to shorten.</param>
        /// <returns>The shortened file path.</returns>
        private string ShortenPath(string path)
        {
            switch (true)
            {
                case true when string.IsNullOrEmpty(path):
                    return "";
                case true when chkShowFullPaths.Checked:
                    return path;
                default:
                    return Path.GetFileName(path); ;
            }

        }


        /// <summary>
        /// Handles the "Open" menu item click event. Opens a file dialog for the user to select a log file,
        /// then loads and displays its contents in the viewer. If a valid file is selected, it also sets up
        /// a FileSystemWatcher to monitor changes to the file for real-time updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Reverse Log (*.rlog)|*.rlog|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _logPath = dlg.FileName;
                    _fileDirty = true;
                    _displayDirty = true;
                    _filterDirty = true;
                    if (!string.IsNullOrEmpty(_logPath) && File.Exists(_logPath))
                    {
                        // Load initial content
                        LoadLog();
                        WireUpRefreshWatcher();
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

        /// <summary>
        /// Handles the "Show Full Paths" checkbox change event. When toggled, it updates the window title and reloads
        /// the log entries to reflect the new path display preference (full paths vs. shortened filenames). This allows
        /// users to quickly switch between a concise view and a detailed view of file paths in the log entries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFullPaths_CheckedChanged(object sender, EventArgs e)
        {
            _displayDirty = true;
            Text = ShortenPath(_logPath);
            if (!string.IsNullOrEmpty(_logPath) && File.Exists(_logPath))
            {
                LoadLog();
            }
            else
            {
                richTextBox1.Clear();
            }
        }

        /// <summary>
        /// Handles the filter combo box selection change event. When the user selects a different log entry type to filter by,
        /// this method marks the filter as "dirty" and reloads the log entries to apply the new filter. This allows users to
        /// quickly switch between viewing all entries and viewing only specific types of entries (e.g., errors, warnings)
        /// without needing to reload the entire file manually.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            _filterDirty = true;
            if (!string.IsNullOrWhiteSpace(_logPath) && File.Exists(_logPath))
            {
                LoadLog();
            }
            else
            {
                richTextBox1.Clear();
            }
        }

        /// <summary>
        /// Handles the form's FormClosing event. This method ensures that all resources used by the form, such as the
        /// FileSystemWatcher and timers, are properly disposed of when the form is closed. This prevents potential memory leaks
        /// and ensures that file handles are released, allowing other applications to access the log file without issues after
        /// the viewer is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _watcher?.Dispose();
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();
        }
    }
}
