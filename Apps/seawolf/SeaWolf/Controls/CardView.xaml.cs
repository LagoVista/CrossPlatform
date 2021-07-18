using System.ComponentModel;
using Xamarin.Forms;

namespace SeaWolf.Controls
{
    [DesignTimeVisible(true)]
    public partial class CardView : ContentView
    {
        public static readonly BindableProperty CardValueProperty = BindableProperty.Create(nameof(CardValue), typeof(string), typeof(CardView), string.Empty);
        public static readonly BindableProperty CardDescriptionProperty = BindableProperty.Create(nameof(CardDescription), typeof(string), typeof(CardView), string.Empty);
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CardView), Color.DarkGray);
        public static readonly BindableProperty CardColorProperty = BindableProperty.Create(nameof(CardColor), typeof(Color), typeof(CardView), Color.White);
        public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(CardView), default(ImageSource));
        public static readonly BindableProperty IconKeyProperty = BindableProperty.Create(nameof(IconKey), typeof(string), typeof(CardView), default(string));
        public static readonly BindableProperty IconBackgroundColorProperty = BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(CardView), Color.LightGray);

        public string CardValue
        {
            get => (string)GetValue(CardView.CardValueProperty);
            set => SetValue(CardView.CardValueProperty, value);
        }

        public string CardDescription
        {
            get => (string)GetValue(CardView.CardDescriptionProperty);
            set => SetValue(CardView.CardDescriptionProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(CardView.BorderColorProperty);
            set => SetValue(CardView.BorderColorProperty, value);
        }

        public Color CardColor
        {
            get => (Color)GetValue(CardView.CardColorProperty);
            set => SetValue(CardView.CardColorProperty, value);
        }

        public ImageSource IconImageSource
        {
            get => (ImageSource)GetValue(CardView.IconImageSourceProperty);
            set => SetValue(CardView.IconImageSourceProperty, value);
        }

        public string IconKey
        {
            get => (string)GetValue(CardView.IconKeyProperty);
            set => SetValue(CardView.IconKeyProperty, value);
        }

        public Color IconBackgroundColor
        {
            get => (Color)GetValue(CardView.IconBackgroundColorProperty);
            set => SetValue(CardView.IconBackgroundColorProperty, value);
        }

        public CardView()
        {
            InitializeComponent();
        }
    }
}