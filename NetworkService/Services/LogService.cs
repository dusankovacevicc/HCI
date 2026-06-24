using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NetworkService.Models;

namespace NetworkService.Services
{
    /// <summary>
    /// Writes every received measurement to a log file (.txt) on disk and reads
    /// it back for the graph. Each line stores the timestamp, the entity it
    /// refers to and the measured value.
    ///
    /// Line format (pipe separated):
    ///   yyyy-MM-dd HH:mm:ss | entityId | entityName | value | VALID/INVALID
    /// </summary>
    public class LogService
    {
        private static readonly object FileLock = new object();

        /// <summary>Raised on the calling thread whenever a measurement is logged.</summary>
        public event Action<MeasurementRecord> MeasurementLogged;

        public string LogFilePath { get; }

        public LogService()
        {
            LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        }

        /// <summary>Appends a new measurement to the log and notifies listeners.</summary>
        public void Log(Entity entity, double value, DateTime timestamp)
        {
            bool valid = value >= Entity.MinValidValue && value <= Entity.MaxValidValue;
            string line = string.Format(
                CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss} | {1} | {2} | {3:0.00} | {4}",
                timestamp,
                entity.Id,
                entity.Name,
                value,
                valid ? "VALID" : "INVALID");

            lock (FileLock)
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }

            MeasurementLogged?.Invoke(new MeasurementRecord(entity.Id, value, timestamp));
        }

        /// <summary>
        /// Reads the last <paramref name="count"/> measurements for the given
        /// entity from the log file (oldest-first). Used to initialise the graph.
        /// </summary>
        public List<MeasurementRecord> ReadLast(int entityId, int count)
        {
            var result = new List<MeasurementRecord>();

            lock (FileLock)
            {
                if (!File.Exists(LogFilePath))
                {
                    return result;
                }

                string[] lines = File.ReadAllLines(LogFilePath);
                for (int i = lines.Length - 1; i >= 0 && result.Count < count; i--)
                {
                    if (TryParse(lines[i], out MeasurementRecord record) && record.EntityId == entityId)
                    {
                        result.Insert(0, record);
                    }
                }
            }

            return result;
        }

        private static bool TryParse(string line, out MeasurementRecord record)
        {
            record = null;
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string[] parts = line.Split('|');
            if (parts.Length < 4)
            {
                return false;
            }

            if (!DateTime.TryParse(parts[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ts))
            {
                return false;
            }

            if (!int.TryParse(parts[1].Trim(), out int id))
            {
                return false;
            }

            if (!double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return false;
            }

            record = new MeasurementRecord(id, value, ts);
            return true;
        }
    }
}
