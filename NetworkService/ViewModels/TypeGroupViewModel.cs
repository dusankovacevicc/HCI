using System.Collections.ObjectModel;
using NetworkService.Models;
using NetworkService.MVVM;

namespace NetworkService.ViewModels
{

    public class TypeGroupViewModel : ObservableObject
    {
        public TypeGroupViewModel(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }

        public ObservableCollection<Entity> Items { get; } = new ObservableCollection<Entity>();
    }
}
