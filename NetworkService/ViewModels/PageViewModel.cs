using NetworkService.MVVM;

namespace NetworkService.ViewModels
{

    public abstract class PageViewModel : ObservableObject
    {
        protected PageViewModel(IAppController controller)
        {
            Controller = controller;
        }

        protected IAppController Controller { get; }

        public abstract string Title { get; }
    }
}
