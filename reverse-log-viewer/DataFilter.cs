using System.Collections.Generic;
using file_mover_log;

namespace reverse_log_viewer
{
    // <summary>
    // Represents a filter for categorizing log entries.
    // </summary>
    public class DataFilter
    {
        // <summary>
        // Gets or sets the list of available filters.
        // </summary>
        public List<KeyValuePair<string, LogEntryType>> Filters { get; set; } = new List<KeyValuePair<string, LogEntryType>>()
        {
            new KeyValuePair<string, LogEntryType>("All", LogEntryType.All),
            new KeyValuePair<string, LogEntryType>("Moved", LogEntryType.Moved),
            new KeyValuePair<string, LogEntryType>("Moved(v)", LogEntryType.MovedVersioned),
            new KeyValuePair<string, LogEntryType>("Warning", LogEntryType.Warning),
            new KeyValuePair<string, LogEntryType>("Error", LogEntryType.Error),
            new KeyValuePair<string, LogEntryType>("InternalError", LogEntryType.InternalError)
        };
    }
}
