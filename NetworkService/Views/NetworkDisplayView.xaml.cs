using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NetworkService.Models;
using NetworkService.ViewModels;

namespace NetworkService.Views
{
    /// <summary>
    /// Code-behind for the Drag&amp;Drop network. It only handles the low-level
    /// pointer mechanics (start drag, drop, distinguish a click from a drag) and
    /// delegates every actual state change to the NetworkDisplayViewModel.
    /// </summary>
    public partial class NetworkDisplayView : UserControl
    {
        private const double DragThreshold = 6.0;

        private Point _pressPosition;
        private bool _draggingFromGrid;

        public NetworkDisplayView()
        {
            InitializeComponent();
        }

        private NetworkDisplayViewModel ViewModel => DataContext as NetworkDisplayViewModel;

        // ----- Dragging an entity out of the TreeView -----

        private void TreeItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _pressPosition = e.GetPosition(null);
        }

        private void TreeItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (!MovedEnough(e.GetPosition(null)))
            {
                return;
            }

            if (sender is FrameworkElement element && element.DataContext is Entity entity)
            {
                DragDrop.DoDragDrop(element, entity, DragDropEffects.Move);
            }
        }

        // ----- Dragging / clicking a grid cell -----

        private void Slot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _pressPosition = e.GetPosition(null);
            _draggingFromGrid = false;
        }

        private void Slot_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggingFromGrid)
            {
                return;
            }

            if (!MovedEnough(e.GetPosition(null)))
            {
                return;
            }

            if (sender is FrameworkElement element && element.DataContext is SlotViewModel slot && slot.IsOccupied)
            {
                _draggingFromGrid = true;
                DragDrop.DoDragDrop(element, slot, DragDropEffects.Move);
            }
        }

        private void Slot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool wasDragging = _draggingFromGrid;
            _draggingFromGrid = false;
            if (wasDragging)
            {
                return;
            }

            // A genuine click (no significant movement) toggles a connection.
            if (MovedEnough(e.GetPosition(null)))
            {
                return;
            }

            if (sender is FrameworkElement element && element.DataContext is SlotViewModel slot)
            {
                ViewModel?.SlotClicked(slot);
            }
        }

        private void Slot_Drop(object sender, DragEventArgs e)
        {
            if (!(sender is FrameworkElement element) || !(element.DataContext is SlotViewModel target))
            {
                return;
            }

            if (e.Data.GetDataPresent(typeof(Entity)))
            {
                ViewModel?.PlaceFromTree((Entity)e.Data.GetData(typeof(Entity)), target);
            }
            else if (e.Data.GetDataPresent(typeof(SlotViewModel)))
            {
                ViewModel?.MoveOnGrid((SlotViewModel)e.Data.GetData(typeof(SlotViewModel)), target);
            }
        }

        // ----- Dropping a grid cell back onto the TreeView returns the entity -----

        private void Tree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SlotViewModel)))
            {
                ViewModel?.ReturnToTree((SlotViewModel)e.Data.GetData(typeof(SlotViewModel)));
            }
        }

        private bool MovedEnough(Point current)
        {
            return Math.Abs(current.X - _pressPosition.X) > DragThreshold ||
                   Math.Abs(current.Y - _pressPosition.Y) > DragThreshold;
        }
    }
}
