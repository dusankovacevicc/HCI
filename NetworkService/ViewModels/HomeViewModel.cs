using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using NetworkService.Models;
using NetworkService.MVVM;

namespace NetworkService.ViewModels
{

    public class HomeViewModel : PageViewModel
    {
        public HomeViewModel(IAppController controller) : base(controller)
        {
            OpenEntitiesCommand = new RelayCommand(controller.NavigateToEntities);
            OpenDisplayCommand = new RelayCommand(controller.NavigateToDisplay);
            OpenGraphCommand = new RelayCommand(controller.NavigateToGraph);

            controller.Entities.CollectionChanged += OnEntitiesChanged;
            HookAll();
        }

        public override string Title => "Reactor Monitor";

        public ICommand OpenEntitiesCommand { get; }
        public ICommand OpenDisplayCommand { get; }
        public ICommand OpenGraphCommand { get; }

        public int OutOfRangeCount =>
            Controller.Entities.Count(e => e.HasMeasurement && !e.IsValid);

        public bool HasWarning => OutOfRangeCount > 0;

        public string WarningText =>
            $"{OutOfRangeCount} vrednosti van opsega ({Entity.MinValidValue:0}–{Entity.MaxValidValue:0}{Entity.Unit})";

        private void OnEntitiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Entity entity in e.OldItems.OfType<Entity>())
                {
                    entity.PropertyChanged -= OnEntityPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (Entity entity in e.NewItems.OfType<Entity>())
                {
                    entity.PropertyChanged += OnEntityPropertyChanged;
                }
            }

            RaiseWarningChanged();
        }

        private void HookAll()
        {
            foreach (Entity entity in Controller.Entities)
            {
                entity.PropertyChanged += OnEntityPropertyChanged;
            }
        }

        private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Entity.IsValid) || e.PropertyName == nameof(Entity.HasMeasurement))
            {
                RaiseWarningChanged();
            }
        }

        private void RaiseWarningChanged()
        {
            OnPropertyChanged(nameof(OutOfRangeCount));
            OnPropertyChanged(nameof(HasWarning));
            OnPropertyChanged(nameof(WarningText));
        }
    }
}
