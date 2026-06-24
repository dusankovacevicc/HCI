using NetworkService.Models;
using NetworkService.MVVM;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// A visual line connecting two entities placed on the Drag&amp;Drop grid.
    /// The two endpoints reference entities (not fixed slots), so when an entity
    /// is moved to another cell the line follows it. The X1/Y1/X2/Y2 coordinates
    /// are recomputed by the NetworkDisplayViewModel whenever placement changes.
    /// </summary>
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

        /// <summary>True if this connection links the same (unordered) pair of entities.</summary>
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
