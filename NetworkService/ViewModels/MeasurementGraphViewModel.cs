using System;
using System.Collections.ObjectModel;
using NetworkService.Models;
using NetworkService.Services;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// Measurement Graph View. Draws the history (last five values) of the
    /// selected entity, based on the data written to the log file. The graph is
    /// updated in real time as new measurements arrive (no manual refresh).
    /// The drawing itself (graph type G3 - circles of different radii along the
    /// time axis) is produced programmatically in the View, without any ready-made
    /// chart control.
    /// </summary>
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

        /// <summary>Entities offered in the ComboBox for selecting the graph source.</summary>
        public ObservableCollection<Entity> Entities => Controller.Entities;

        /// <summary>The last (up to five) measurements drawn on the graph, oldest-first.</summary>
        public ObservableCollection<MeasurementRecord> RecentMeasurements { get; } =
            new ObservableCollection<MeasurementRecord>();

        /// <summary>Raised when the graph data changes so the View can redraw.</summary>
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
