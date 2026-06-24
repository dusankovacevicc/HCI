using System.Collections.ObjectModel;
using NetworkService.Models;
using NetworkService.Services;
using NetworkService.Undo;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// Contract that the MainViewModel exposes to all child ViewModels. It bundles
    /// navigation, the shared data store and the cross-cutting services (undo,
    /// toast, confirmation) so that child ViewModels do not depend on the concrete
    /// MainViewModel implementation.
    /// </summary>
    public interface IAppController
    {
        ObservableCollection<Entity> Entities { get; }
        UndoManager UndoManager { get; }
        ToastService Toast { get; }
        ConfirmationService Confirmation { get; }

        // Navigation between the application's views.
        void NavigateToHome();
        void NavigateToEntities();
        void NavigateToDisplay();
        void NavigateToGraph();
        void NavigateToAddEntity();

        // Data operations (record undo, restart simulator, show feedback).
        void AddEntity(Entity entity);
        void DeleteEntity(Entity entity);
        bool IsIdUnique(int id);
    }
}
