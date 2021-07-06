using LagoVista.XPlat.Core;
using System;
using Xamarin.Forms.Xaml;

namespace LagoVista.Kiosk.App.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class KioskViewerView : LagoVistaContentPage
	{
		public KioskViewerView()
		{
			InitializeComponent();
		}

		protected override bool OnBackButtonPressed()
		{
			KioskViewer.Source = new Uri("about:blank");
			return base.OnBackButtonPressed();
		}
	}
}