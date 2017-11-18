using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using System.Reflection;
using Windows.UI.Xaml.Controls;

namespace LagoVista.UWP.UI
{
    public class Navigation : IViewModelNavigation
    {
        private static Navigation _instance = new Navigation();
        private Frame _rootFrame;

        private IDictionary<Type, Type> _navDictionary = new Dictionary<Type, Type>();

        public void Initialize(Frame rootFrame)
        {
            _rootFrame = rootFrame;
            SystemNavigationManager.GetForCurrentView().BackRequested += Navigation_BackRequested;
        }

        private void Navigation_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (_rootFrame.CanGoBack)
                _rootFrame.GoBack();

            e.Handled = true;
        }

        public void Navigate(ViewModelLaunchArgs args)
        {
            Type viewType = null;
            if (_navDictionary.Keys.Contains(args.ViewModelType))
                viewType = _navDictionary[args.ViewModelType];
            else
            {
                Type viewModelInterface = args.ViewModelType;
                var key = _navDictionary.Keys.Where(cls => cls.GetInterfaces().Contains(args.ViewModelType)).FirstOrDefault();
                if (key == null)
                    throw new Exception(String.Format("Could not find view for {0}", args.ViewModelType.FullName));
                args.ViewModelType = key;
                viewType = _navDictionary[key];
            }

            _rootFrame.Navigate(viewType, args);
        }

        public static Navigation Instance { get { return _instance; } }

        public void Add<T, V>() where T : ViewModelBase where V : LagoVistaPage
        {
            _navDictionary.Add(typeof(T), typeof(V));
        }

        public bool CanGoBack()
        {
            return _rootFrame.CanGoBack;
        }
        

        public void Navigate<T>() where T : ViewModelBase
        {
            Navigate(new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(T),
                LaunchType = LaunchTypes.Other
            });
        }
        
        public Task NavigateAsync(ViewModelLaunchArgs args)
        {
            Navigate(args);

            return Task.FromResult(default(object));
        }

        public Task NavigateAsync<TViewModel>() where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            args.LaunchType = LaunchTypes.Other;
            return NavigateAsync(args);

        }

        public Task NavigateAndCreateAsync<TViewModel>(params KeyValuePair<string, object>[] parms) where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            foreach(var param in parms)
            {
                args.Parameters.Add(param.Key, param.Value);
            }
            args.LaunchType = LaunchTypes.Create;

            return NavigateAsync(args);
        }

        public Task NavigateAndCreateAsync<TViewModel>(object parent, params KeyValuePair<string, object>[] parms) where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            foreach (var param in parms)
            {
                args.Parameters.Add(param.Key, param.Value);
            }
            args.Parent = parent;
            args.LaunchType = LaunchTypes.Create;

            return NavigateAsync(args);
        }

        public Task NavigateAndEditAsync<TViewModel>(object parent, object child, params KeyValuePair<string, object>[] parms) where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            foreach (var param in parms)
            {
                args.Parameters.Add(param.Key, param.Value);
            }
            args.Parent = parent;
            args.Child = child;
            args.LaunchType = LaunchTypes.Edit;

            return NavigateAsync(args);
        }

        public Task NavigateAndEditAsync<TViewModel>(object parent, string id, params KeyValuePair<string, object>[] parms) where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            foreach (var param in parms)
            {
                args.Parameters.Add(param.Key, param.Value);
            }
            args.Parent = parent;
            args.ChildId = id;
            args.LaunchType = LaunchTypes.Edit;

            return NavigateAsync(args);
        }

        public Task NavigateAndEditAsync<TViewModel>(string id, params KeyValuePair<string, object>[] parms) where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            foreach (var param in parms)
            {
                args.Parameters.Add(param.Key, param.Value);
            }

            args.ChildId = id;
            args.LaunchType = LaunchTypes.Edit;

            return NavigateAsync(args);
        }

        public Task NavigateAndPickAsync<TViewModel>(Action<object> selectedAction, Action cancelledAction = null, params KeyValuePair<string, object>[] parms) where TViewModel : ViewModelBase
        {
            var args = new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel) };
            foreach (var param in parms)
            {
                args.Parameters.Add(param.Key, param.Value);
            }

            args.SelectedAction = selectedAction;
            args.CancelledAction = cancelledAction;
            args.LaunchType = LaunchTypes.Picker;

            return NavigateAsync(args);
        }

        public async Task SetAsNewRootAsync<TViewModel>() where TViewModel : ViewModelBase
        {
            await NavigateAsync(new ViewModelLaunchArgs() { ViewModelType = typeof(TViewModel), LaunchType = LaunchTypes.View });
            _rootFrame.BackStack.Clear();
        }

        public Task GoBackAsync()
        {
            _rootFrame.GoBack();
            return Task.FromResult(default(object));
        }

        public Task NavigateAsync(ViewModelBase parentViewModel, Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
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

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
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

        public Task SetAsNewRootAsync<TViewModel>(params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task SetAsNewRootAsync(Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }
    }
}
