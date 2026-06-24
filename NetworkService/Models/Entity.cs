using NetworkService.MVVM;

namespace NetworkService.Models
{

    public class Entity : ObservableObject
    {

        public const double MinValidValue = 250.0;


        public const double MaxValidValue = 350.0;


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


        public bool IsValid =>
            !HasMeasurement || (LastValue >= MinValidValue && LastValue <= MaxValidValue);


        public string LastValueDisplay =>
            HasMeasurement
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} {1}", LastValue, Unit)
                : "-";
    }
}
