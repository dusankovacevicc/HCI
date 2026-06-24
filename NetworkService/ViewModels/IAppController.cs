using System.Collections.ObjectModel;
using NetworkService.Models;
using NetworkService.Services;
using NetworkService.Undo;

namespace NetworkService.ViewModels
{

    public interface IAppController
    {
        ObservableCollection<Entity> Entities { get; }
        UndoManager UndoManager { get; }
        ToastService Toast { get; }
        ConfirmationService Confirmation { get; }


        void NavigateToHome();
        void NavigateToEntities();
        void NavigateToDisplay();
        void NavigateToGraph();
        void NavigateToAddEntity();


        void AddEntity(Entity entity);
        void DeleteEntity(Entity entity);
        bool IsIdUnique(int id);
    }
}
