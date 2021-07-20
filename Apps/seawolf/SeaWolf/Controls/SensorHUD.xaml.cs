using System.ComponentModel;
using Xamarin.Forms;

namespace SeaWolf.Controls
{
    [DesignTimeVisible(true)]
    public partial class SensorHUD : ContentView
    {
        public static readonly BindableProperty CardValueProperty = BindableProperty.Create(nameof(CardValue), typeof(string), typeof(SensorHUD), string.Empty);
        public static readonly BindableProperty CardDescriptionProperty = BindableProperty.Create(nameof(CardDescription), typeof(string), typeof(SensorHUD), string.Empty);
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(SensorHUD), Color.DarkGray);
        public static readonly BindableProperty CardColorProperty = BindableProperty.Create(nameof(CardColor), typeof(Color), typeof(SensorHUD), Color.White);
        public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(SensorHUD), default(ImageSource));
        public static readonly BindableProperty IconKeyProperty = BindableProperty.Create(nameof(IconKey), typeof(string), typeof(SensorHUD), default(string));
        public static readonly BindableProperty IconBackgroundColorProperty = BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(SensorHUD), Color.LightGray);

        public string CardValue
        {
            get => (string)GetValue(SensorHUD.CardValueProperty);
            set => SetValue(SensorHUD.CardValueProperty, value);
        }

        public string CardDescription
        {
            get => (string)GetValue(SensorHUD.CardDescriptionProperty);
            set => SetValue(SensorHUD.CardDescriptionProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(SensorHUD.BorderColorProperty);
            set => SetValue(SensorHUD.BorderColorProperty, value);
        }

        public Color CardColor
        {
            get => (Color)GetValue(SensorHUD.CardColorProperty);
            set => SetValue(SensorHUD.CardColorProperty, value);
        }

        public ImageSource IconImageSource
        {
            get => (ImageSource)GetValue(SensorHUD.IconImageSourceProperty);
            set => SetValue(SensorHUD.IconImageSourceProperty, value);
        }

        public string IconKey
        {
            get => (string)GetValue(SensorHUD.IconKeyProperty);
            set => SetValue(SensorHUD.IconKeyProperty, value);
        }

        public Color IconBackgroundColor
        {
            get => (Color)GetValue(SensorHUD.IconBackgroundColorProperty);
            set => SetValue(SensorHUD.IconBackgroundColorProperty, value);
        }

        public SensorHUD()
        {
            InitializeComponent();
        }
    }
}