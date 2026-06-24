using System;

namespace NetworkService.Models
{
    
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


        public string TimeLabel => Timestamp.ToString("HH:mm:ss");
    }
}
