using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetworkService.MVVM;

namespace NetworkService.Undo
{
    /// <summary>
    /// Central history of performed actions. Implemented as a stack so that
    /// the most recent action is undone first. Exposes an observable history
    /// collection for the UI and a CanUndo flag that drives the Undo button.
    ///
    /// A single shared instance lives in the MainViewModel and is passed to all
    /// child ViewModels, so every action across the whole application ends up in
    /// one unified history (CG3 requirement: undo one action at a time through
    /// the entire history).
    /// </summary>
    public class UndoManager : ObservableObject
    {
        private readonly Stack<IUndoableAction> _actions = new Stack<IUndoableAction>();

        /// <summary>Newest-first list of action descriptions, for display.</summary>
        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

        public bool CanUndo => _actions.Count > 0;

        /// <summary>Description of the action that would be undone next (for tooltip).</summary>
        public string NextUndoDescription =>
            _actions.Count > 0 ? _actions.Peek().Description : "Nema akcija za poništavanje";

        /// <summary>Registers a new action at the top of the history.</summary>
        public void Push(IUndoableAction action)
        {
            _actions.Push(action);
            History.Insert(0, action.Description);
            RaiseChanged();
        }

        /// <summary>Convenience overload for delegate-based actions.</summary>
        public void Push(string description, System.Action undo)
        {
            Push(new RelayUndoableAction(description, undo));
        }

        /// <summary>Reverts the most recent action, if any.</summary>
        public void Undo()
        {
            if (_actions.Count == 0)
            {
                return;
            }

            IUndoableAction action = _actions.Pop();
            if (History.Count > 0)
            {
                History.RemoveAt(0);
            }

            action.Undo();
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(NextUndoDescription));
        }
    }
}
