using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LagoVista.UWP.UI
{
    public class TapableGrid : Grid
    {
        ICommand _tapUpCommand;
        ICommand _tapDownCommand;

        Object _tapUpCommandParameter;
        Object _tapDownCommandParameter;

        public TapableGrid()
        {
            this.PointerReleased += TapableImage_PointerReleased;
            this.PointerPressed += TapableImage_PointerPressed;
            this.PointerExited += TapableImage_PointerExited;
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
        #endregion

        public static readonly DependencyProperty TapDownCommandProperty =
            DependencyProperty.Register("TapDownCommand", typeof(ICommand), typeof(TapableGrid), new PropertyMetadata(null, OnTapDownCommandChanged));

        public static readonly DependencyProperty TapUpCommandProperty =
            DependencyProperty.Register("TapUpCommand", typeof(ICommand), typeof(TapableGrid), new PropertyMetadata(null, OnTapUpCommandChanged));

        public static readonly DependencyProperty TapDownCommandParameterProperty =
            DependencyProperty.Register("TapDownCommandParameter", typeof(object), typeof(TapableGrid), new PropertyMetadata(null, OnTapDownCommandParameterChanged));

        public static readonly DependencyProperty TapUpCommandParameterProperty =
            DependencyProperty.Register("TapUpCommandParameter", typeof(object), typeof(TapableGrid), new PropertyMetadata(null, OnTapUpCommandParameterChanged));

        private static void OnTapDownCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableGrid)obj;
            ctl._tapDownCommand = args.NewValue as ICommand;
        }

        private static void OnTapUpCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableGrid)obj;
            ctl._tapUpCommand = args.NewValue as ICommand;
        }

        private static void OnTapDownCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableGrid)obj;
            ctl._tapDownCommandParameter = args.NewValue;
        }

        private static void OnTapUpCommandParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = (TapableGrid)obj;
            ctl._tapUpCommandParameter = args.NewValue;
        }
    }
}
