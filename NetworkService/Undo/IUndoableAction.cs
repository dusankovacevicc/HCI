namespace NetworkService.Undo
{
    /// <summary>
    /// Represents a single user action whose effect can be reverted.
    /// CG3 (mobile users) requires the ability to undo actions one by one,
    /// walking back through the whole history of performed actions.
    /// </summary>
    public interface IUndoableAction
    {
        /// <summary>Short human-readable description shown in tooltips / history.</summary>
        string Description { get; }

        /// <summary>Reverts the effect of this action.</summary>
        void Undo();
    }
}
