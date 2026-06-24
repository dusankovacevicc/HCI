using System;

namespace NetworkService.Undo
{
    /// <summary>
    /// A generic undoable action whose revert logic is provided as a delegate.
    /// This lets ViewModels register undo behaviour with a captured closure
    /// instead of writing a dedicated class for every kind of action.
    /// </summary>
    public class RelayUndoableAction : IUndoableAction
    {
        private readonly Action _undo;

        public RelayUndoableAction(string description, Action undo)
        {
            Description = description;
            _undo = undo ?? throw new ArgumentNullException(nameof(undo));
        }

        public string Description { get; }

        public void Undo()
        {
            _undo();
        }
    }
}
