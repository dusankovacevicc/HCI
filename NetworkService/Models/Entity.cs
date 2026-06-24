using NetworkService.MVVM;

namespace NetworkService.Models
{
    /// <summary>
    /// A monitored entity. Theme T7 - Reactor temperature: the entity models
    /// temperature-measuring equipment with attributes Id, Name and Type.
    /// A valid measured value is between 250 and 350 degrees Celsius; anything
    /// outside that interval is considered invalid (out of range).
    /// </summary>
    public class Entity : ObservableObject
    {
        /// <summary>Lower bound of the valid measurement range (inclusive).</summary>
        public const double MinValidValue = 250.0;

        /// <summary>Upper bound of the valid measurement range (inclusive).</summary>
        public const double MaxValidValue = 350.0;

        /// <summary>Measurement unit for the reactor-temperature theme.</summary>
        public const string Unit = "°C";

        private int _id;
        private string _name;
        private EntityType _type;
        private double _lastValue;
        private bool _hasMeasurement;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public EntityType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        /// <summary>Last measured value received from the simulator.</summary>
        public double LastValue
        {
            get => _lastValue;
            set
            {
                if (SetProperty(ref _lastValue, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                    OnPropertyChanged(nameof(LastValueDisplay));
                }
            }
        }

        /// <summary>True once at least one measurement has been received.</summary>
        public bool HasMeasurement
        {
            get => _hasMeasurement;
            set
            {
                if (SetProperty(ref _hasMeasurement, value))
                {
                    OnPropertyChanged(nameof(IsValid));
                    OnPropertyChanged(nameof(LastValueDisplay));
                }
            }
        }

        /// <summary>
        /// True when the last measurement is within the valid range. Entities
        /// without a measurement yet are treated as valid (neutral) so they are
        /// not falsely flagged as in danger.
        /// </summary>
        public bool IsValid =>
            !HasMeasurement || (LastValue >= MinValidValue && LastValue <= MaxValidValue);

        /// <summary>Formatted value for display, e.g. "317.45 C" or "-".</summary>
        public string LastValueDisplay =>
            HasMeasurement
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} {1}", LastValue, Unit)
                : "-";
    }
}
