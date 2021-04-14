using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.Kiosk.App.ViewModels
{
	public class HomeViewModel : AppViewModelBase
	{
		private Uri url;

		public Uri Url
		{
			get => url;
			set 
			{
				Set(ref url, value);
			}
		}

		public HomeViewModel()
		{
			var menuItems = new ObservableCollection<MenuItem>();

			var kiosks = GetKiosks();
			foreach (var kiosk in kiosks)
			{
				menuItems.Add(new MenuItem()
				{
					FontIconKey = string.IsNullOrWhiteSpace(kiosk.Value.Item2)
						? "fa-gear"
						: "fa-gear",
					Name = kiosk.Value.Item1,
					Command = new RelayCommand(() => DoNavigation(kiosk.Value.Item2)),
					CommandParameter = kiosk
				});
			};

			menuItems.Add(new MenuItem() { FontIconKey = "fa-gear", Name = "Logout", Command = new RelayCommand(() => AuthManager.LogoutAsync()) });

			MenuItems = menuItems;
		}

		private void DoNavigation(string address)
		{
			Url = new Uri(address);
		}

		private Dictionary<Guid, Tuple<string, string>> GetKiosks()
		{
			return new Dictionary<Guid, Tuple<string, string>>()
			{
				{ Guid.NewGuid(), Tuple.Create("Kiosk 1", "https://bing.com") },
				{ Guid.NewGuid(), Tuple.Create("Kiosk 2", "https://duckduckgo.com")  },
				{ Guid.NewGuid(), Tuple.Create("Kiosk 3", "https://epicsearch.in")  }
			};
		}
	}
}
