using System.Collections.Generic;
using file_mover_log;

namespace ReverseLogViewer
{
    public class DataFilter
    {

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
