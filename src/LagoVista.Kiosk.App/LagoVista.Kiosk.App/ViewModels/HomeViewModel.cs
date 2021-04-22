using LagoVista.Client.Core;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using LagoVista.XPlat.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Kiosk.App.ViewModels
{
	public class HomeViewModel : ListViewModelBase<KioskView>
	{
		private readonly IRestClient _restClient;

		private ObservableCollection<MenuItem> menuOptions;
		public ObservableCollection<MenuItem> MenuOptions
		{
			get => menuOptions;
			set { Set(ref menuOptions, value); }
		}

		private object selectedMenuOption;

		public object SelectedMenuOption
		{
			get => selectedMenuOption;
			set => Set(ref selectedMenuOption, value);
		}

		public HomeViewModel(IRestClient restClient)
		{
			_restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));

			var menuItems = new ObservableCollection<MenuItem>()
			{
				new MenuItem() {FontIconKey = "fa-sign-in", Name = "Change Organization", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)) },
				new MenuItem() { FontIconKey = "fa-sign-out", Name = "Logout", Command = new RelayCommand(() => AuthManager.LogoutAsync()) }
			};

			MenuItems = menuItems;
		}

		private async Task DoNavigationAsync(string address)
		{
			var response = await _restClient.GetAsync<KioskClientAppSummary>(address);

			var viewModelLaunchArgs = new ViewModelLaunchArgs()
			{
				ParentViewModel = this,
				LaunchType = LaunchTypes.View,
				ViewModelType = typeof(KioskViewerViewModel)
			};
			viewModelLaunchArgs.Parameters.Add("url", response.Result.KioskUrl);

			SLWIOC.TryResolve<IViewModelNavigation>(out var navigation);
			await navigation.NavigateAsync(viewModelLaunchArgs);
		}

		protected override string GetListURI()
		{
			return "/api/ui/kiosks";
		}

		protected override void SetListItems(IEnumerable<KioskView> items)
		{
			base.SetListItems(items);

			var menuOptions = new ObservableCollection<MenuItem>();

			foreach (var kiosk in items)
			{
				var endpointUrl = $"/api/kioskclientapp/{kiosk.Id}";
				menuOptions.Add(new MenuItem()
				{
					FontIconKey = "fa-desktop",
					Name = kiosk.Name,
					Command = new RelayCommand(async () => await DoNavigationAsync(endpointUrl)),
					CommandParameter = kiosk.Title
				});
			};

			MenuOptions = menuOptions;
		}
	}

	public class KioskView
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Title { get; set; }
	}

	public class KioskClientAppSummary
	{
		public string KioskUrl { get; set; }
	}
}