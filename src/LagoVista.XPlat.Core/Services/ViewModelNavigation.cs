
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.Core.Views;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


namespace LagoVista.XPlat.Core.Services
{
    public class ViewModelNavigation : IViewModelNavigation
    {
        Dictionary<Type, Type> _viewModelLookup = new Dictionary<Type, Type>();

        public Stack<ViewModelBase> ViewModelBackStack { get; private set; }

        global::Xamarin.Forms.Application _app;
        global::Xamarin.Forms.INavigation _navigation;

        public ViewModelNavigation(global::Xamarin.Forms.Application app)
        {
            _app = app;
            ViewModelBackStack = new Stack<ViewModelBase>();
        }

        public void Start<V>() where V : XPlatViewModel
        {
            var view = Activator.CreateInstance(_viewModelLookup[typeof(V)]) as LagoVistaContentPage;
            _navigation = view.Navigation;
            var viewModel = SLWIOC.CreateForType<V>();
            view.ViewModel = viewModel;
            _app.MainPage = new LagoVistaNavigationPage(view)
            {
                Title = "HelloWorld"
            };

            Debug.WriteLine(_app.MainPage);
        }

        public void Add<T, V>() where T : ViewModelBase where V : ILagoVistaPage
        {
            _viewModelLookup.Add(typeof(T), typeof(V));
        }

        private Task ShowViewModelAsync(ViewModelLaunchArgs args)
        {
            if (args.ParentViewModel == null)
            {
                args.ParentViewModel = ViewModelBackStack.FirstOrDefault();
            }

            var view = Activator.CreateInstance(_viewModelLookup[args.ViewModelType]) as LagoVistaContentPage;
            var viewModel = SLWIOC.CreateForType(args.ViewModelType) as XPlatViewModel;
            ViewModelBackStack.Push(viewModel);

            viewModel.LaunchArgs = args;
            view.ViewModel = viewModel;
            return _navigation.PushAsync(view);

        }

        public Task NavigateAsync<TViewModel>(ViewModelBase parentVM, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.ParentViewModel = parentVM;
            launchArgs.LaunchType = LaunchTypes.Other;
            launchArgs.ViewModelType = typeof(TViewModel);

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAsync(ViewModelLaunchArgs args)
        {
            return ShowViewModelAsync(args);
        }

        public async Task GoBackAsync()
        {
            if (ViewModelBackStack.Any())
            {
                ViewModelBackStack.Pop();
                await _navigation.PopAsync();
            }
        }


        public void PopToRoot()
        {
            while (ViewModelBackStack.Count > 1)
            {
                ViewModelBackStack.Pop();
            }

            _navigation.PopToRootAsync();
        }

        public bool CanGoBack()
        {
            return _navigation.NavigationStack.Count > 1;
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentVM, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.ParentViewModel = parentVM;
            launchArgs.LaunchType = LaunchTypes.Create;
            launchArgs.ViewModelType = typeof(TViewModel);

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentVM, object parent, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.Create;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.ParentViewModel = parentVM;
            launchArgs.Parent = parent;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentVM, object parent, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.Edit;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.Parent = parent;
            launchArgs.ParentViewModel = parentVM;
            launchArgs.Child = child;


            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndPickAsync<TViewModel>(ViewModelBase parentVM, Action<Object> selectedAction, Action cancelledAction = null, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.Picker;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.ParentViewModel = parentVM;
            launchArgs.SelectedAction = selectedAction;
            launchArgs.CancelledAction = cancelledAction;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentVM, object parent, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.Edit;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.Parent = parent;
            launchArgs.ParentViewModel = parentVM;
            launchArgs.ChildId = id;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentVM, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.Edit;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.ChildId = id;
            launchArgs.ParentViewModel = parentVM;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentVM, object parent, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.View;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.Parent = parent;
            launchArgs.ParentViewModel = parentVM;
            launchArgs.Child = child;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentVM, object parent, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.View;
            launchArgs.ViewModelType = typeof(TViewModel);
            launchArgs.Parent = parent;
            launchArgs.ParentViewModel = parentVM;
            launchArgs.ChildId = id;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task NavigateAsync(ViewModelBase parentVM, Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            var launchArgs = new ViewModelLaunchArgs();
            launchArgs.LaunchType = LaunchTypes.View;
            launchArgs.ViewModelType = viewModelType;
            launchArgs.ParentViewModel = parentVM;

            foreach (var arg in args)
            {
                launchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            return ShowViewModelAsync(launchArgs);
        }

        public Task SetAsNewRootAsync<TViewModel>(params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var viewModel = SLWIOC.CreateForType<TViewModel>() as ViewModelBase;
            ViewModelBackStack.Clear();
            ViewModelBackStack.Push(viewModel);
            viewModel.LaunchArgs = new ViewModelLaunchArgs()
            {
                IsNewRoot = true,
                LaunchType = LaunchTypes.Other
            };

            var viewModelType = typeof(TViewModel);
            var view = Activator.CreateInstance(_viewModelLookup[viewModelType]) as LagoVistaContentPage;
            view.ViewModel = viewModel as XPlatViewModel;
            _navigation = view.Navigation;
            _app.MainPage = new LagoVistaNavigationPage(view);

            return Task.FromResult(default(object));
        }

        public Task SetAsNewRootAsync(Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            var viewModel = SLWIOC.CreateForType(viewModelType) as ViewModelBase;
            viewModel.LaunchArgs = new ViewModelLaunchArgs()
            {
                IsNewRoot = true,
                LaunchType = LaunchTypes.Other,
            };

            ViewModelBackStack.Clear();
            ViewModelBackStack.Push(viewModel);

            var view = Activator.CreateInstance(_viewModelLookup[viewModelType]) as LagoVistaContentPage;
            view.ViewModel = viewModel as XPlatViewModel;
            _navigation = view.Navigation;
            _app.MainPage = new LagoVistaNavigationPage(view);

            return Task.FromResult(default(object));
        }
    }
}