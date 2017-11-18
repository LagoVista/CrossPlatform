using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LagoVista.UWP.UI
{
    public class TapableImage : Grid
    {
        Image _image;
        TextBlock _caption;

        ICommand _tapUpCommand;
        ICommand _tapDownCommand;

        Object _tapUpCommandParameter;
        Object _tapDownCommandParameter;

        public TapableImage()
        {
            _image = new Image();
            _image.Stretch = Stretch.Uniform;
            _caption = new TextBlock() { Foreground = new SolidColorBrush(Colors.White), FontSize = 16, TextAlignment = TextAlignment.Center };

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            _image.PointerReleased += TapableImage_PointerReleased;
            _image.PointerPressed += TapableImage_PointerPressed;
            _image.PointerExited += TapableImage_PointerExited;

            _caption.SetValue(Grid.RowProperty, 1);

            Children.Add(_image);
            Children.Add(_caption);
        }

        private void TapableImage_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_tapUpCommand != null && _tapUpCommand.CanExecute(_tapUpCommandParameter))
                _tapUpCommand.Execute(_tapUpCommandParameter);

        }

        private void TapableImage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_tapDownCommand != null && _tapDownCommand.CanExecute(_tapDownCommandParameter))
                _tapDownCommand.Execute(_tapDownCommandParameter);

        }

        private void TapableImage_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_tapUpCommand != null && _tapUpCommand.CanExecute(_tapUpCommandParameter))
                _tapUpCommand.Execute(_tapUpCommandParameter);

        }


        #region Commands and Parameters
        public ICommand TapDownCommand
        {
            get { return (ICommand)GetValue(TapDownCommandProperty); }
            set { SetValue(TapDownCommandProperty, value); }
        }

        public Object TapDownCommandParameter
        {
            get { return GetValue(TapDownCommandParameterProperty); }
            set { SetValue(TapDownCommandParameterProperty, value); }
        }

        public ICommand TapUpCommand
        {
            get { return (ICommand)GetValue(TapUpCommandProperty); }
            set { SetValue(TapUpCommandProperty, value); }
        }


        public Object TapUpCommandParameter
        {
            get { return GetValue(TapUpCommandParameterProperty); }
            set { SetValue(TapUpCommandParameterProperty, value); }
        }

        public String Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }
        #endregion

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(String), typeof(TapableImage), new PropertyMetadata(null, OnCaptionChanged));

        public static readonly DependencyProperty TapDownCommandProperty =
            DependencyProperty.Register("TapDownCommand", typeof(ICommand), typeof(TapableImage), new PropertyMetadata(null, OnTapDownCommandChanged));

        public static readonly DependencyProperty TapUpCommandProperty =
            DependencyProperty.Register("TapUpCommand", typeof(ICommand), typeof(TapableImage), new PropertyMetadata(null, OnTapUpCommandChanged));

        public static readonly DependencyProperty TapDownCommandParameterProperty =
            DependencyProperty.Register("TapDownCommandParameter", typeof(object), typeof(TapableImage), new PropertyMetadata(null, OnTapDownCommandParameterChanged));

        public static readonly DependencyProperty TapUpCommandParameterProperty =
            DependencyProperty.Register("TapUpCommandParameter", typeof(object), typeof(TapableImage), new PropertyMetadata(null, OnTapUpCommandParameterChanged));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(TapableImage), new PropertyMetadata(null, OnSourceChanged));

        private static void OnTapDownCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableImage)obj;
            ctl._tapDownCommand = args.NewValue as ICommand;
        }

        private static void OnTapUpCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableImage)obj;
            ctl._tapUpCommand = args.NewValue as ICommand;
        }

        private static void OnTapDownCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableImage)obj;
            ctl._tapDownCommandParameter = args.NewValue;
        }

        private static void OnTapUpCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableImage)obj;
            ctl._tapUpCommandParameter = args.NewValue;
        }

        public static void OnCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableImage)obj;
            if (args.NewValue != null)
                ctl._caption.Text = args.NewValue as String;
            else
                ctl._caption.Text = String.Empty;
        }

        public static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableImage)obj;
            var imageUriString = args.NewValue as String;
            if(imageUriString != null)
            {
                Uri uri;
                if (Uri.TryCreate(imageUriString, UriKind.RelativeOrAbsolute, out uri))
                    ctl._image.Source = new BitmapImage(uri);
                else
                    Debug.WriteLine("{Bingind - Invalidate Image String: " + imageUriString);
            }
            else
                ctl._image.Source = args.NewValue as ImageSource;
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
    }
}
