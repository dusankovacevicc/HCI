using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NetworkService.Models;
using NetworkService.MVVM;
using NetworkService.Services;
using NetworkService.Undo;

namespace NetworkService.ViewModels
{

    public class MainViewModel : ObservableObject, IAppController
    {
        private readonly object _snapshotLock = new object();
        private List<Entity> _snapshot = new List<Entity>();

        private readonly SimulatorServer _server;

        private readonly HomeViewModel _homeViewModel;
        private readonly NetworkEntitiesViewModel _entitiesViewModel;
        private readonly NetworkDisplayViewModel _displayViewModel;
        private readonly MeasurementGraphViewModel _graphViewModel;

        private PageViewModel _currentViewModel;
        private bool _suppressUndo;

        public MainViewModel()
        {
            Entities = new ObservableCollection<Entity>();
            UndoManager = new UndoManager();
            Toast = new ToastService();
            Confirmation = new ConfirmationService();
            LogService = new LogService();

            Entities.CollectionChanged += OnEntitiesCollectionChanged;


            foreach (Entity entity in SeedData.CreateInitialEntities())
            {
                Entities.Add(entity);
            }

            _homeViewModel = new HomeViewModel(this);
            _entitiesViewModel = new NetworkEntitiesViewModel(this);
            _displayViewModel = new NetworkDisplayViewModel(this);
            _graphViewModel = new MeasurementGraphViewModel(this, LogService);

            _currentViewModel = _homeViewModel;

            UndoCommand = new RelayCommand(UndoManager.Undo, () => UndoManager.CanUndo);
            HomeCommand = new RelayCommand(NavigateToHome);
            EntitiesCommand = new RelayCommand(NavigateToEntities);
            DisplayCommand = new RelayCommand(NavigateToDisplay);
            GraphCommand = new RelayCommand(NavigateToGraph);

            _server = new SimulatorServer(GetEntitiesSnapshot);
            _server.MeasurementReceived += OnMeasurementReceived;
        }

        // ----- Shared state (IAppController) ------------------------------------------

        public ObservableCollection<Entity> Entities { get; }
        public UndoManager UndoManager { get; }
        public ToastService Toast { get; }
        public ConfirmationService Confirmation { get; }
        public LogService LogService { get; }

        public PageViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand UndoCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand EntitiesCommand { get; }
        public ICommand DisplayCommand { get; }
        public ICommand GraphCommand { get; }

        // ----- Lifecycle --------------------------------------------------------------

        public void Start()
        {
            _server.Start();
        }

        public void Shutdown()
        {
            _server.Stop();
        }

        // ----- Navigation -------------------------------------------------------------

        public void NavigateToHome() => Navigate(_homeViewModel);
        public void NavigateToEntities() => Navigate(_entitiesViewModel);
        public void NavigateToDisplay() => Navigate(_displayViewModel);
        public void NavigateToGraph() => Navigate(_graphViewModel);

        public void NavigateToAddEntity() => Navigate(new AddEntityViewModel(this));

        private void Navigate(PageViewModel target)
        {
            if (target == null || target == CurrentViewModel)
            {
                return;
            }

            PageViewModel previous = CurrentViewModel;
            CurrentViewModel = target;


            if (!_suppressUndo && previous != null)
            {
                UndoManager.Push(
                    $"Navigacija: {previous.Title} → {target.Title}",
                    () =>
                    {
                        _suppressUndo = true;
                        CurrentViewModel = previous;
                        _suppressUndo = false;
                    });
            }
        }

        // ----- Data operations --------------------------------------------------------

        public void AddEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            Entities.Add(entity);
            _server.RestartSimulator();

            UndoManager.Push(
                $"Dodavanje entiteta \"{entity.Name}\"",
                () =>
                {
                    Entities.Remove(entity);
                    _server.RestartSimulator();
                });

            Toast.Show("Entitet dodat", $"Entitet \"{entity.Name}\" (ID: {entity.Id}) je uspešno dodat.", ToastType.Success);
        }

        public void DeleteEntity(Entity entity)
        {
            if (entity == null || !Entities.Contains(entity))
            {
                return;
            }

            Entities.Remove(entity);
            _server.RestartSimulator();

            UndoManager.Push(
                $"Brisanje entiteta \"{entity.Name}\"",
                () =>
                {
                    Entities.Add(entity);
                    _server.RestartSimulator();
                });

            Toast.Show("Entitet obrisan", $"Entitet \"{entity.Name}\" (ID: {entity.Id}) je uspešno obrisan.", ToastType.Success);
        }

        public bool IsIdUnique(int id)
        {
            return Entities.All(e => e.Id != id);
        }

        // ----- Simulator wiring -------------------------------------------------------

        private List<Entity> GetEntitiesSnapshot()
        {
            lock (_snapshotLock)
            {
                return _snapshot;
            }
        }

        private void OnEntitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            lock (_snapshotLock)
            {
                _snapshot = Entities.ToList();
            }
        }

        private void OnMeasurementReceived(Entity entity, double value, DateTime timestamp)
        {

            Application.Current?.Dispatcher.Invoke(() =>
            {
                entity.LastValue = value;
                entity.HasMeasurement = true;
                LogService.Log(entity, value, timestamp);
            });
        }
    }
}
