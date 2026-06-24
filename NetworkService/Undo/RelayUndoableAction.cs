using System;

namespace NetworkService.Undo
{
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
