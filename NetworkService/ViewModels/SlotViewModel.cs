using NetworkService.Models;
using NetworkService.MVVM;

namespace NetworkService.ViewModels
{

    public class SlotViewModel : ObservableObject
    {
        private Entity _entity;
        private bool _isSelected;

        public SlotViewModel(int index, double x, double y, double width, double height)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Index { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public double CenterX => X + Width / 2.0;
        public double CenterY => Y + Height / 2.0;

        public Entity Entity
        {
            get => _entity;
            set
            {
                if (SetProperty(ref _entity, value))
                {
                    OnPropertyChanged(nameof(IsOccupied));
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }

        public bool IsOccupied => _entity != null;
        public bool IsEmpty => _entity == null;


        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
