using System.Collections.ObjectModel;
using NetworkService.Models;
using NetworkService.MVVM;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// A group in the TreeView of the Network Display View. Entities are grouped
    /// by type; a group lists only the entities of that type which are NOT yet
    /// placed on the Drag&amp;Drop grid.
    /// </summary>
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
