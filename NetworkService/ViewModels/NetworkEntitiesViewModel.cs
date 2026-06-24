using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using NetworkService.Models;
using NetworkService.MVVM;
using NetworkService.Services;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// Network Entities View. Shows all entities in a table, supports adding and
    /// deleting them, and implements search variant P1: two radio buttons
    /// ("Naziv" / "Tip") together with a text box. Selecting an option and typing
    /// (part of) a name shows only entities whose name or type name partially or
    /// fully matches the entered text.
    /// </summary>
    public class NetworkEntitiesViewModel : PageViewModel
    {
        private readonly CollectionViewSource _viewSource;

        private bool _searchByName = true;
        private string _searchText = string.Empty;
        private Entity _selectedEntity;

        // The criteria actually applied (only updated when "Filtriraj" is pressed).
        private string _appliedText = string.Empty;
        private bool _appliedByName = true;

        public NetworkEntitiesViewModel(IAppController controller) : base(controller)
        {
            _viewSource = new CollectionViewSource { Source = controller.Entities };
            _viewSource.View.Filter = FilterPredicate;

            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ResetFilterCommand = new RelayCommand(ResetFilter);
            AddCommand = new RelayCommand(controller.NavigateToAddEntity);
            DeleteCommand = new RelayCommand(DeleteSelected, () => SelectedEntity != null);
        }

        public override string Title => "Entiteti";

        /// <summary>Filtered, bindable view of the entities for the DataGrid.</summary>
        public ICollectionView Entities => _viewSource.View;

        public bool SearchByName
        {
            get => _searchByName;
            set
            {
                if (SetProperty(ref _searchByName, value) && value)
                {
                    OnPropertyChanged(nameof(SearchByType));
                }
            }
        }

        public bool SearchByType
        {
            get => !_searchByName;
            set
            {
                if (value == !_searchByName)
                {
                    return;
                }

                SearchByName = !value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set => SetProperty(ref _selectedEntity, value);
        }

        public ICommand ApplyFilterCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        private void ApplyFilter()
        {
            _appliedText = (SearchText ?? string.Empty).Trim();
            _appliedByName = SearchByName;
            Entities.Refresh();
        }

        private void ResetFilter()
        {
            SearchText = string.Empty;
            _appliedText = string.Empty;
            Entities.Refresh();
        }

        private bool FilterPredicate(object item)
        {
            if (string.IsNullOrEmpty(_appliedText))
            {
                return true;
            }

            if (!(item is Entity entity))
            {
                return false;
            }

            string target = _appliedByName ? entity.Name : entity.Type?.Name;
            return target != null &&
                   target.IndexOf(_appliedText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DeleteSelected()
        {
            Entity toDelete = SelectedEntity;
            if (toDelete == null)
            {
                return;
            }

            Controller.Confirmation.Request(
                "Brisanje entiteta",
                $"Da li ste sigurni da želite da obrišete entitet \"{toDelete.Name}\" (ID: {toDelete.Id})?",
                () => Controller.DeleteEntity(toDelete));
        }
    }
}
