using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.XPlat.Core.Resources;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.Controls.FormControls
{
    public class ChildListRow : FormControl
    {
        Label _label;
        IconButton _addImage;
        StackLayout _childItemList;

        public event EventHandler<string> Add;
        public event EventHandler<DeleteItemEventArgs> Deleted;

        public event EventHandler<ItemSelectedEventArgs> ItemSelected;

        public ChildListRow(FormViewer formViewer, FormField field) : base(formViewer, field)
        {
            HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true);

            var titleBar = new Grid()
            {
                BackgroundColor = AppStyle.MenuBarBackground.ToXamFormsColor(),
                HeightRequest = 48,
                HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true)
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            _label = new Label()
            {
                FontSize = 20,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                TextColor = AppStyle.MenuBarForeground.ToXamFormsColor(),
                Text = field.Label
            };

            _addImage = new IconButton()
            {
                Margin = new Thickness(0, 0, 20, 0),
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                IconKey = "fa-plus",
                WidthRequest = 48,
                HeightRequest = 48,
                FontSize = 22
            };
            _addImage.Clicked += _addImage_Clicked;
            _addImage.SetValue(Grid.ColumnProperty, 1);

            _childItemList = new StackLayout();

            titleBar.Children.Add(_label);
            titleBar.Children.Add(_addImage);

            Children.Add(titleBar);
            Children.Add(_childItemList);
        }

        private void _addImage_Clicked(object sender, EventArgs e)
        {
            Add?.Invoke(this, Field.Name);
        }

        private void Item_Tapped(object sender, EventArgs e)
        {
            var childItem = (sender as Grid).BindingContext as IEntityHeaderEntity;

            ItemSelected?.Invoke(this, new ItemSelectedEventArgs()
            {
                Id = childItem.ToEntityHeader().Id,
                Type = Field.Name
            });
        }

        public override void Refresh()
        {
            _childItemList.Children.Clear();

            if (_childItems != null)
            {
                foreach (var child in _childItems)
                {
                    var label = new Label();
                    label.Margin = new Thickness(15, 10, 10, 10);
                    label.FontSize = 24;
                    label.TextColor = Color.FromRgb(0x5B, 0x5B, 0x5B);
                    label.Text = child.ToEntityHeader().Text;

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                    var boxView = new BoxView();
                    boxView.HeightRequest = 1;
                    boxView.SetValue(Grid.ColumnSpanProperty, 3);
                    boxView.SetValue(Grid.RowProperty, 1);
                    boxView.Color = Color.SlateGray;

                    var tapGenerator = new TapGestureRecognizer();
                    grid.BindingContext = child;
                    tapGenerator.Tapped += Item_Tapped;

                    var deleteButton = new IconButton()
                    {
                        IconKey = "fa-trash-o",
                        FontSize = 20,
                        TextColor = Color.Red,
                        VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                        Tag = child.ToEntityHeader().Id
                    };

                    deleteButton.SetValue(Grid.ColumnProperty, 1);
                    deleteButton.Clicked += DeleteButton_Clicked;

                    var img = new Icon()
                    {
                        IconKey = "fa-chevron-right",
                        Margin = new Thickness(2, 10, 30, 0),
                        HeightRequest = 24,
                        WidthRequest = 24,
                        VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                    };
                    img.SetValue(Grid.ColumnProperty, 2);


                    grid.GestureRecognizers.Add(tapGenerator);

                    grid.Children.Add(label);
                    grid.Children.Add(boxView);
                    grid.Children.Add(deleteButton);
                    grid.Children.Add(img);

                    _childItemList.Children.Add(grid);
                }
            }
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            var btn = sender as IconButton;
            var deleteItemArgs = new DeleteItemEventArgs()
            {
                Id = btn.Tag as String,
                Type = Field.Name.ToPropertyName()
            };

            var obj = ChildItems as Object;

            if (obj is System.Collections.IList)
            {
                if (await SLWIOC.Get<LagoVista.Core.PlatformSupport.IPopupServices>().ConfirmAsync(XPlatResources.Msg_ConfirmDeleteItemTitle, XPlatResources.Msg_ConfirmDeleteItem))
                {
                    var childList = ChildItems as System.Collections.IList;
                    var itemToBeDeleted = ChildItems.Where(itm => itm.ToEntityHeader().Id == btn.Tag as string).FirstOrDefault();
                    childList.Remove(itemToBeDeleted);
                    Deleted?.Invoke(sender, deleteItemArgs);
                    Refresh();
                }
            }
        }

        IEnumerable<IEntityHeaderEntity> _childItems;
        public IEnumerable<IEntityHeaderEntity> ChildItems
        {
            get { return _childItems; }
            set
            {
                _childItems = value;
                if (value is INotifyCollectionChanged)
                {
                    (value as INotifyCollectionChanged).CollectionChanged += ChildListRow_CollectionChanged;
                }
                Refresh();
            }
        }

        private void ChildListRow_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
