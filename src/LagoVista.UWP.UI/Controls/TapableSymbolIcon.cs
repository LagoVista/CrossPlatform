using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LagoVista.UWP.UI
{
    public class TapableSymbolIcon : Grid
    {
        ICommand _tapUpCommand;
        ICommand _tapDownCommand;

        Object _tapUpCommandParameter;
        Object _tapDownCommandParameter;

        Viewbox _iconContainer;
        SymbolIcon _symbol;

        public TapableSymbolIcon()
        {
            this.PointerReleased += TapableSymbolIcon_PointerReleased;
            this.PointerPressed += TapableSymbolIcon_PointerPressed;
            this.PointerExited += TapableSymbolIcon_PointerExited;

            _iconContainer = new Viewbox();
            _symbol = new SymbolIcon();

            Children.Add(_iconContainer);
            Children.Add(_symbol);
        }

        private void TapableSymbolIcon_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_tapUpCommand != null && _tapUpCommand.CanExecute(_tapUpCommandParameter))
                _tapUpCommand.Execute(_tapUpCommandParameter);

        }

        private void TapableSymbolIcon_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_tapDownCommand != null && _tapDownCommand.CanExecute(_tapDownCommandParameter))
                _tapDownCommand.Execute(_tapDownCommandParameter);

        }

        private void TapableSymbolIcon_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
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

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        #endregion

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(TapableSymbolIcon), new PropertyMetadata(null, OnForegroundChanged));


        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(Symbol), typeof(TapableSymbolIcon), new PropertyMetadata(null, OnSymbolChanged));

        public static readonly DependencyProperty TapDownCommandProperty =
            DependencyProperty.Register("TapDownCommand", typeof(ICommand), typeof(TapableSymbolIcon), new PropertyMetadata(null, OnTapDownCommandChanged));

        public static readonly DependencyProperty TapUpCommandProperty =
            DependencyProperty.Register("TapUpCommand", typeof(ICommand), typeof(TapableSymbolIcon), new PropertyMetadata(null, OnTapUpCommandChanged));

        public static readonly DependencyProperty TapDownCommandParameterProperty =
            DependencyProperty.Register("TapDownCommandParameter", typeof(object), typeof(TapableSymbolIcon), new PropertyMetadata(null, OnTapDownCommandParameterChanged));

        public static readonly DependencyProperty TapUpCommandParameterProperty =
            DependencyProperty.Register("TapUpCommandParameter", typeof(object), typeof(TapableSymbolIcon), new PropertyMetadata(null, OnTapUpCommandParameterChanged));

        private static void OnTapDownCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableSymbolIcon)obj;
            ctl._tapDownCommand = args.NewValue as ICommand;
        }

        private static void OnTapUpCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableSymbolIcon)obj;
            ctl._tapUpCommand = args.NewValue as ICommand;
        }

        private static void OnTapDownCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableSymbolIcon)obj;
            ctl._tapDownCommandParameter = args.NewValue;
        }

        private static void OnTapUpCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableSymbolIcon)obj;
            ctl._tapUpCommandParameter = args.NewValue;
        }

        public static void OnSymbolChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableSymbolIcon)obj;
            ctl._symbol.Symbol = (Symbol)args.NewValue;
        }

        public static void OnForegroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableSymbolIcon)obj;
            ctl._symbol.Foreground = (Brush)args.NewValue;
        }
    }
}
