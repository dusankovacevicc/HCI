using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using NetworkService.Models;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// Network Display View. A 3x4 grid of cells (12 canvases) onto which entities
    /// are placed using Drag&amp;Drop. Entities not yet placed are listed in a
    /// TreeView grouped by type; once placed they disappear from the TreeView and
    /// reappear there when removed from the grid. Placed entities can be connected
    /// with lines that follow them when they are moved. Deleting an entity removes
    /// it and all of its connections from the grid. The whole state is preserved
    /// across navigation because a single instance of this ViewModel is kept alive.
    /// </summary>
    public class NetworkDisplayViewModel : PageViewModel
    {
        // Grid layout constants (tuned for the portrait phone window).
        private const int Columns = 3;
        private const int Rows = 4;
        private const double CellWidth = 104;
        private const double CellHeight = 80;
        private const double Gap = 8;
        private const double Margin = 6;

        private SlotViewModel _connectionSource;

        public NetworkDisplayViewModel(IAppController controller) : base(controller)
        {
            BuildSlots();
            BuildGroups();

            foreach (Entity entity in controller.Entities)
            {
                AddToTree(entity);
            }

            controller.Entities.CollectionChanged += OnEntitiesChanged;
        }

        public override string Title => "Mreza";

        public ObservableCollection<SlotViewModel> Slots { get; } = new ObservableCollection<SlotViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();
        public ObservableCollection<TypeGroupViewModel> TreeGroups { get; } = new ObservableCollection<TypeGroupViewModel>();

        public double CanvasWidth => Margin * 2 + Columns * CellWidth + (Columns - 1) * Gap;
        public double CanvasHeight => Margin * 2 + Rows * CellHeight + (Rows - 1) * Gap;

        // ----- Drag & Drop operations -------------------------------------------------

        /// <summary>Places an entity coming from the TreeView onto an empty slot.</summary>
        public void PlaceFromTree(Entity entity, SlotViewModel target)
        {
            if (entity == null || target == null || target.IsOccupied)
            {
                return;
            }

            RemoveFromTree(entity);
            target.Entity = entity;
            UpdateConnectionGeometry();

            Controller.UndoManager.Push(
                $"Postavljanje entiteta \"{entity.Name}\" na mrežu",
                () =>
                {
                    target.Entity = null;
                    AddToTree(entity);
                    UpdateConnectionGeometry();
                });
        }

        /// <summary>Moves an already placed entity from one cell to another empty cell.</summary>
        public void MoveOnGrid(SlotViewModel source, SlotViewModel target)
        {
            if (source == null || target == null || source == target ||
                !source.IsOccupied || target.IsOccupied)
            {
                return;
            }

            Entity entity = source.Entity;
            source.Entity = null;
            target.Entity = entity;
            UpdateConnectionGeometry();

            Controller.UndoManager.Push(
                $"Premeštanje entiteta \"{entity.Name}\"",
                () =>
                {
                    target.Entity = null;
                    source.Entity = entity;
                    UpdateConnectionGeometry();
                });
        }

        /// <summary>Returns a placed entity to the TreeView and drops its connections.</summary>
        public void ReturnToTree(SlotViewModel source)
        {
            if (source == null || !source.IsOccupied)
            {
                return;
            }

            Entity entity = source.Entity;
            List<ConnectionViewModel> removed = Connections.Where(c => c.Involves(entity)).ToList();
            foreach (ConnectionViewModel c in removed)
            {
                Connections.Remove(c);
            }

            source.Entity = null;
            if (_connectionSource == source)
            {
                source.IsSelected = false;
                _connectionSource = null;
            }

            AddToTree(entity);
            UpdateConnectionGeometry();

            Controller.UndoManager.Push(
                $"Uklanjanje entiteta \"{entity.Name}\" sa mreže",
                () =>
                {
                    RemoveFromTree(entity);
                    source.Entity = entity;
                    foreach (ConnectionViewModel c in removed)
                    {
                        Connections.Add(c);
                    }
                    UpdateConnectionGeometry();
                });
        }

        // ----- Connections ------------------------------------------------------------

        /// <summary>
        /// Click behaviour used to connect two entities: first click selects an
        /// endpoint, the second click on a different placed entity creates a line.
        /// </summary>
        public void SlotClicked(SlotViewModel slot)
        {
            if (slot == null || !slot.IsOccupied)
            {
                ClearConnectionSelection();
                return;
            }

            if (_connectionSource == null)
            {
                _connectionSource = slot;
                slot.IsSelected = true;
            }
            else if (_connectionSource == slot)
            {
                ClearConnectionSelection();
            }
            else
            {
                CreateConnection(_connectionSource.Entity, slot.Entity);
                ClearConnectionSelection();
            }
        }

        private void CreateConnection(Entity a, Entity b)
        {
            if (a == null || b == null || a == b)
            {
                return;
            }

            // Prevent drawing more than one line between the same pair of entities.
            if (Connections.Any(c => c.Links(a, b)))
            {
                Controller.Toast.Show("Veza", "Veza između ova dva entiteta već postoji.", Services.ToastType.Info);
                return;
            }

            var connection = new ConnectionViewModel(a, b);
            Connections.Add(connection);
            UpdateConnectionGeometry();

            Controller.UndoManager.Push(
                $"Povezivanje \"{a.Name}\" i \"{b.Name}\"",
                () =>
                {
                    Connections.Remove(connection);
                    UpdateConnectionGeometry();
                });
        }

        private void ClearConnectionSelection()
        {
            if (_connectionSource != null)
            {
                _connectionSource.IsSelected = false;
                _connectionSource = null;
            }
        }

        /// <summary>Recomputes line endpoints from the current cell of each entity.</summary>
        private void UpdateConnectionGeometry()
        {
            foreach (ConnectionViewModel connection in Connections)
            {
                SlotViewModel slotA = FindSlot(connection.EntityA);
                SlotViewModel slotB = FindSlot(connection.EntityB);
                if (slotA == null || slotB == null)
                {
                    continue;
                }

                connection.X1 = slotA.CenterX;
                connection.Y1 = slotA.CenterY;
                connection.X2 = slotB.CenterX;
                connection.Y2 = slotB.CenterY;
            }
        }

        private SlotViewModel FindSlot(Entity entity)
        {
            return Slots.FirstOrDefault(s => s.Entity == entity);
        }

        // ----- Tree (available entities) ----------------------------------------------

        private void AddToTree(Entity entity)
        {
            if (entity?.Type == null)
            {
                return;
            }

            TypeGroupViewModel group = TreeGroups.FirstOrDefault(g => g.TypeName == entity.Type.Name);
            if (group != null && !group.Items.Contains(entity))
            {
                group.Items.Add(entity);
            }
        }

        private void RemoveFromTree(Entity entity)
        {
            foreach (TypeGroupViewModel group in TreeGroups)
            {
                group.Items.Remove(entity);
            }
        }

        // ----- Reaction to data-store changes -----------------------------------------

        private void OnEntitiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearEverything();
                return;
            }

            if (e.OldItems != null)
            {
                foreach (Entity entity in e.OldItems.OfType<Entity>())
                {
                    RemoveEntityCompletely(entity);
                }
            }

            if (e.NewItems != null)
            {
                foreach (Entity entity in e.NewItems.OfType<Entity>())
                {
                    AddToTree(entity);
                }
            }
        }

        /// <summary>Removes a deleted entity from grid, tree and all of its connections.</summary>
        private void RemoveEntityCompletely(Entity entity)
        {
            SlotViewModel slot = FindSlot(entity);
            if (slot != null)
            {
                slot.Entity = null;
                if (_connectionSource == slot)
                {
                    ClearConnectionSelection();
                }
            }

            RemoveFromTree(entity);

            foreach (ConnectionViewModel c in Connections.Where(c => c.Involves(entity)).ToList())
            {
                Connections.Remove(c);
            }

            UpdateConnectionGeometry();
        }

        private void ClearEverything()
        {
            foreach (SlotViewModel slot in Slots)
            {
                slot.Entity = null;
                slot.IsSelected = false;
            }

            Connections.Clear();
            foreach (TypeGroupViewModel group in TreeGroups)
            {
                group.Items.Clear();
            }

            _connectionSource = null;
        }

        // ----- Construction helpers ---------------------------------------------------

        private void BuildSlots()
        {
            int index = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    double x = Margin + col * (CellWidth + Gap);
                    double y = Margin + row * (CellHeight + Gap);
                    Slots.Add(new SlotViewModel(index++, x, y, CellWidth, CellHeight));
                }
            }
        }

        private void BuildGroups()
        {
            foreach (EntityType type in Services.EntityTypeCatalog.All)
            {
                TreeGroups.Add(new TypeGroupViewModel(type.Name));
            }
        }
    }
}
