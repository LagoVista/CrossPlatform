using LagoVista.Client.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace LagoVista.Kiosk.App.ViewModels
{
	public class KioskViewerViewModel : AppViewModelBase
	{
		private Uri url;
		public Uri Url
		{
			get => url;
			set { Set(ref url, value); }
		}

		public override Task InitAsync()
		{
			base.InitAsync();

			if (LaunchArgs != null && LaunchArgs.Parameters.TryGetValue("url", out object address))
			{
				Url = new Uri((string)address);
			}

			return Task.CompletedTask;
		}
	}
}