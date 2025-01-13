using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.Maui.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Maui.Services
{
    public class ViewModelNavigation : IViewModelNavigation
    {
        private INavigation _host;
        private readonly Dictionary<Type, Type> _viewModelLookup = new Dictionary<Type, Type>();

        public void RegisterView<TViewModel, TView>() where TViewModel : ViewModelBase where TView : PageBase 
        {
            _viewModelLookup.Add(typeof(TViewModel), typeof(TView));
        }

        public void SetHost(INavigation navHost)
        {
            _host = navHost;
        }

        public bool CanGoBack()
        {
            throw new NotImplementedException();
        }

        public Task GoBackAsync()
        {
            throw new NotImplementedException();
        }

        public Task GoBackAsync(int dropPageCount)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndPickAsync<TViewModel>(ViewModelBase parentViewModel, Action<object> selectedAction, Action cancelledAction = null, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync(ViewModelLaunchArgs args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync(ViewModelBase parentViewModel, Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task SetAsNewRootAsync<TViewModel>(params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var viewModelType = typeof(TViewModel);
            var viewType = _viewModelLookup[viewModelType];
            var vmInstance = SLWIOC.CreateForType(viewModelType);
            var pageInstance = SLWIOC.CreateForType(viewType);
            var page = pageInstance as PageBase;
            page.BindingContext = vmInstance;
            return _host.PushAsync(page);
        }

        public Task SetAsNewRootAsync(Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            var viewType = _viewModelLookup[viewModelType];
            var pageInstance = SLWIOC.CreateForType(viewType);
            var vmInstance = SLWIOC.CreateForType(viewModelType);
            var page = pageInstance as PageBase;
            page.BindingContext = vmInstance;
            return _host.PushAsync(page);
        }
    }
}
