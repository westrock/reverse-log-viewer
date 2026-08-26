using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using file_mover_log;
using Newtonsoft.Json;

namespace reverse_log_viewer
{
    public static class LogEntryHelpers
    {
        /// <summary>
        /// Loads log entries from a file and parses them using the provided parsing function.
        /// </summary>
        /// <param name="_logPath">The path to the log file.</param>
        /// <param name="parseLines">A function that parses an array of strings into an enumerable of <see cref="LogEntry"/> instances.</param>
        /// <returns>A list of <see cref="LogEntry"/> instances.</returns>
        public static List<LogEntry> LoadFile(string _logPath, Func<string[], IEnumerable<LogEntry>> parseLines)
        {
            if (string.IsNullOrEmpty(_logPath) || !File.Exists(_logPath))
            {
                return new List<LogEntry>();
            }

            string[] lines;
            using (FileStream fs = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    lines = sr.ReadToEnd()
                              .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                }
            }

            return parseLines(lines)
                            .Reverse()   // newest first
                            .ToList();
        }


        /// <summary>
        /// Parse an array of JSON log lines into <see cref="LogEntry"/> instances.
        /// </summary>
        /// <param name="lines">An array of strings, each representing a JSON log entry.</param>
        /// <returns>An enumerable collection of <see cref="LogEntry"/> instances.</returns>
        public static IEnumerable<LogEntry> ParseJsonLines(string[] lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                LogEntry entry;

                try
                {
                    entry = FromJson(line);
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


        /// <summary>
        /// Deserialize a single JSON log line into a <see cref="LogEntry"/> instance.
        /// 
        /// Attempts to parse the provided JSON string and return a populated <see cref="LogEntry"/>.
        /// Behavior summary:
        /// - If <paramref name="json"/> is <c>null</c>, the method returns <c>null</c>.
        /// - If deserialization returns <c>null</c>, a placeholder <see cref="LogEntry"/> with
        ///   <see cref="LogEntryType.Undefined"/> and the raw JSON placed in <see cref="LogEntry.Message"/>
        ///   is returned.
        /// - If the JSON is malformed or another error occurs while parsing, the method returns a
        ///   <see cref="LogEntry"/> describing the error (often with <see cref="LogEntryType.Undefined"/>
        ///   or <see cref="LogEntryType.InternalError"/>), rather than throwing exceptions to callers.
        /// </summary>
        /// <param name="json">A single-line JSON representation of a log entry. Leading and trailing
        /// whitespace will be trimmed before attempting deserialization.</param>
        /// <returns>
        /// A <see cref="LogEntry"/> representing the parsed data, a placeholder/error <see cref="LogEntry"/>
        /// when parsing fails, or <c>null</c> if <paramref name="json"/> is <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This helper is designed for line-oriented log files where each line is a JSON object.
        /// It is intentionally fault-tolerant: parsing errors are converted into <see cref="LogEntry"/>
        /// instances so callers can handle invalid input without exception handling.
        /// For stricter validation, callers should validate the returned <see cref="LogEntry"/> and
        /// its <see cref="LogEntryType"/> after calling this method.
        /// </remarks>
        public static LogEntry FromJson(string json)
        {
            if (json == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<LogEntry>(json.Trim()) ?? new LogEntry
                {
                    EntryType = LogEntryType.Undefined,
                    Message = "JSON: " + json
                };
            }
            catch (JsonReaderException ex)
            {
                if (ex.Message == "Unexpected character encountered while parsing value: a. Path '', line 0, position 0.")
                {
                    return new LogEntry
                    {
                        EntryType = LogEntryType.Undefined,
                        Message = json
                    };
                }
                else
                {
                    return new LogEntry
                    {
                        EntryType = LogEntryType.Undefined,
                        Message = json
                    };
                }
            }
            catch (Exception ex)
            {
                return new LogEntry
                {
                    EntryType = LogEntryType.InternalError,
                    Message = $"{ex}"
                };
            }
        }
    }
}