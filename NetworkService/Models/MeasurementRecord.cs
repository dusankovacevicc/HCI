using System;

namespace NetworkService.Models
{
    /// <summary>
    /// A single measurement as read back from the Log file (or received live).
    /// Used by the Measurement Graph View to draw the history of the last five
    /// values for the selected entity.
    /// </summary>
    public class MeasurementRecord
    {
        public MeasurementRecord(int entityId, double value, DateTime timestamp)
        {
            EntityId = entityId;
            Value = value;
            Timestamp = timestamp;
        }

        public int EntityId { get; }

        public double Value { get; }

        public DateTime Timestamp { get; }

        public bool IsValid => Value >= Entity.MinValidValue && Value <= Entity.MaxValidValue;

        /// <summary>Time label shown on the graph X-axis (e.g. "10:20").</summary>
        public string TimeLabel => Timestamp.ToString("HH:mm:ss");
    }
}
