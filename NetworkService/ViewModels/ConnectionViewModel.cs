using NetworkService.Models;
using NetworkService.MVVM;

namespace NetworkService.ViewModels
{

    public class ConnectionViewModel : ObservableObject
    {
        private double _x1, _y1, _x2, _y2;

        public ConnectionViewModel(Entity a, Entity b)
        {
            EntityA = a;
            EntityB = b;
        }

        public Entity EntityA { get; }
        public Entity EntityB { get; }

        public double X1 { get => _x1; set => SetProperty(ref _x1, value); }
        public double Y1 { get => _y1; set => SetProperty(ref _y1, value); }
        public double X2 { get => _x2; set => SetProperty(ref _x2, value); }
        public double Y2 { get => _y2; set => SetProperty(ref _y2, value); }


        public bool Links(Entity a, Entity b)
        {
            return (EntityA == a && EntityB == b) || (EntityA == b && EntityB == a);
        }

        public bool Involves(Entity e)
        {
            return EntityA == e || EntityB == e;
        }
    }
}
