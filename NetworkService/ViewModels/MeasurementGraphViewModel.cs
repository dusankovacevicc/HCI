using System;
using System.Collections.ObjectModel;
using NetworkService.Models;
using NetworkService.Services;

namespace NetworkService.ViewModels
{

    public class MeasurementGraphViewModel : PageViewModel
    {
        private const int MaxPoints = 5;
        private readonly LogService _logService;
        private Entity _selectedEntity;

        public MeasurementGraphViewModel(IAppController controller, LogService logService) : base(controller)
        {
            _logService = logService;
            _logService.MeasurementLogged += OnMeasurementLogged;
        }

        public override string Title => "Grafikon";

        public ObservableCollection<Entity> Entities => Controller.Entities;


        public ObservableCollection<MeasurementRecord> RecentMeasurements { get; } =
            new ObservableCollection<MeasurementRecord>();


        public event Action GraphChanged;

        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (SetProperty(ref _selectedEntity, value))
                {
                    LoadHistory();
                }
            }
        }

        private void LoadHistory()
        {
            RecentMeasurements.Clear();
            if (SelectedEntity != null)
            {
                foreach (MeasurementRecord record in _logService.ReadLast(SelectedEntity.Id, MaxPoints))
                {
                    RecentMeasurements.Add(record);
                }
            }

            GraphChanged?.Invoke();
        }

        private void OnMeasurementLogged(MeasurementRecord record)
        {
            if (SelectedEntity == null || record.EntityId != SelectedEntity.Id)
            {
                return;
            }

            RecentMeasurements.Add(record);
            while (RecentMeasurements.Count > MaxPoints)
            {
                RecentMeasurements.RemoveAt(0);
            }

            GraphChanged?.Invoke();
        }
    }
}
