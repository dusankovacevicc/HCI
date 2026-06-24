using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetworkService.MVVM;

namespace NetworkService.Undo
{

    public class UndoManager : ObservableObject
    {
        private readonly Stack<IUndoableAction> _actions = new Stack<IUndoableAction>();


        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

        public bool CanUndo => _actions.Count > 0;


        public string NextUndoDescription =>
            _actions.Count > 0 ? _actions.Peek().Description : "Nema akcija za poništavanje";


        public void Push(IUndoableAction action)
        {
            _actions.Push(action);
            History.Insert(0, action.Description);
            RaiseChanged();
        }


        public void Push(string description, System.Action undo)
        {
            Push(new RelayUndoableAction(description, undo));
        }


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
