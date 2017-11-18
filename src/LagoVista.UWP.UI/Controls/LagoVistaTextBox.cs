using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace LagoVista.UWP.UI
{
    public class LagoVistaTextBox : Grid
    {
        Image _dismissButton;
        Image _passwordButton;

        TextBox _textBox;
        PasswordBox _passwordBox;
        Button _focusGainer;

        Storyboard _showDismiss;
        Storyboard _hideDismiss;

        private bool _isPasswordBox;

        public LagoVistaTextBox()
        {
            _passwordButton = new Image();
            _passwordButton.Source = new BitmapImage(new Uri("ms-appx:///LagoVista.Common.UI/Images/ViewPassword.png", UriKind.Absolute));
            _passwordButton.Width = 36;
            _passwordButton.SetValue(Grid.ColumnProperty, 1);
            _passwordButton.Margin = new Thickness(24, 0, 0, 0);
            _passwordButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _passwordButton.Height = 36;

            _dismissButton = new Image();
            _dismissButton.Source = new BitmapImage(new Uri("ms-appx:///LagoVista.Common.UI/Images/CheckMark.png", UriKind.Absolute));
            _dismissButton.Width = 0;
            _dismissButton.SetValue(Grid.ColumnProperty, 2);
            _dismissButton.Margin = new Thickness(24, 0, 0, 0);
            _dismissButton.Height = 36;

            _textBox = new TextBox();

            _passwordBox = new PasswordBox();
            _passwordBox.Visibility = Visibility.Collapsed;

            _focusGainer = new Button();
            _focusGainer.Width = 0;
            _focusGainer.Height = 0;

            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Children.Add(_passwordButton);
            Children.Add(_dismissButton);
            Children.Add(_textBox);
            Children.Add(_passwordBox);
            Children.Add(_focusGainer);

            _textBox.GotFocus += _textBox_GotFocus;
            _textBox.LostFocus += _textBox_LostFocus;

            _passwordBox.GotFocus += _passwordBox_GotFocus;
            _passwordBox.LostFocus += _passwordBox_LostFocus;

            _dismissButton.Tapped += _dismissButton_Tap;
            _passwordButton.Tapped +=  _passwordButton_Tap;

            _textBox.TextChanged += _textBox_TextChanged;

            _showDismiss = new Storyboard();
            var showAnimation = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTarget(_showDismiss, _dismissButton);
            Storyboard.SetTargetProperty(_showDismiss, "Width");

            var start = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var end = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100));

            showAnimation.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = start, Value = 0 });
            showAnimation.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = end, Value = 36 });

            _showDismiss.Children.Add(showAnimation);

            _hideDismiss = new Storyboard();
            var hideAnimation = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTarget(_hideDismiss, _dismissButton);
            Storyboard.SetTargetProperty(_hideDismiss, "Width");

            start = KeyTime.FromTimeSpan(TimeSpan.Zero);
            end = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100));

            hideAnimation.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = start, Value = 36 });
            hideAnimation.KeyFrames.Add(new LinearDoubleKeyFrame() { KeyTime = end, Value = 0 });

            _isPasswordBox = false;

            _hideDismiss.Children.Add(hideAnimation);
        }
        

        void _passwordButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            if (_passwordBox.Visibility == Visibility.Visible)
            {
                _textBox.Visibility = Visibility.Visible;
                _passwordBox.Visibility = Visibility.Collapsed;
                _textBox.Text = _passwordBox.Password;
            }
            else
            {
                _textBox.Visibility = Visibility.Collapsed;
                _passwordBox.Visibility = Visibility.Visible;
                _passwordBox.Password = _textBox.Text;
            }
        }

        public bool IsPasswordBox
        {
            get { return _isPasswordBox; }
            set
            {
                _isPasswordBox = value;
                if (value)
                {
                    _dismissButton.Margin = new Thickness(14, 0, 0, 0);
                    _passwordButton.Visibility = Visibility.Visible;
                    _textBox.Visibility = Visibility.Collapsed;
                    _passwordBox.Visibility = Visibility.Visible;
                }
                else
                {
                    _dismissButton.Margin = new Thickness(24, 0, 0, 0);
                    _passwordButton.Visibility = Visibility.Collapsed;
                    _textBox.Visibility = Visibility.Visible;
                    _passwordBox.Visibility = Visibility.Collapsed;
                }
                
            }

            
        }

        public InputScope InputScope
        {
            get { return _textBox.InputScope; }
            set { _textBox.InputScope = value; }
        }

        void _textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textBox.Text.EndsWith("\r"))
                _focusGainer.Focus(FocusState.Programmatic);
        }

        void _dismissButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            UpdateBindings();

            _focusGainer.Focus(FocusState.Programmatic);
        }

        void UpdateBindings()
        {
            if (_passwordBox.Visibility == Visibility.Visible)
                _textBox.Text = _passwordBox.Password;

            SetValue(TextProperty, _textBox.Text);

            var binding = this.GetBindingExpression(LagoVistaTextBox.TextProperty);
            if (binding != null)
            {
                Debug.WriteLine("Got Binding, should update it");
                binding.UpdateSource();
            }
        }

        #region Focus Stuff
        void _textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateBindings();
            _hideDismiss.Begin();
        }

        void _textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _showDismiss.Begin();
        }

        void _passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateBindings();
            _hideDismiss.Begin();
        }

        void _passwordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _showDismiss.Begin();
        }
        #endregion

        public TextBox TextBox { get { return _textBox; } }

        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(LagoVistaTextBox), new PropertyMetadata(null, TextChanged));

        private static void TextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (LagoVistaTextBox)obj;
            if (args.NewValue != null)
            {
                ctl._textBox.Text = args.NewValue as String;
                ctl._passwordBox.Password = args.NewValue as string;
            }
            else
            {
                ctl._textBox.Text = String.Empty;
                ctl._passwordBox.Password = String.Empty;
            }
        }
    }
}
