using LagoVista.IoT.Deployment.Admin.Models;
using LagoVista.PlatformManager.Core.Resources;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;

namespace LagoVista.PlatformManager.Controls
{
    public class InstanceRuntimeTreeControl : Grid
    {
        public static readonly BindableProperty RuntimeDetailsProperty = BindableProperty.Create(
                                                    propertyName: nameof(RuntimeDetails),
                                                    returnType: typeof(InstanceRuntimeDetails),
                                                    declaringType: typeof(InstanceRuntimeTreeControl),
                                                    defaultValue: null,
                                                    defaultBindingMode: BindingMode.Default,
                                                    propertyChanged: HandleRuntimeDetailsAssigned);

        public InstanceRuntimeTreeControl()
        {
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            //ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80, GridUnitType.Absolute) });
        }

        private static void HandleRuntimeDetailsAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var ctl = (InstanceRuntimeTreeControl)bindable;
            ctl.RuntimeDetails = newValue as InstanceRuntimeDetails;
        }

        public InstanceRuntimeDetails RuntimeDetails
        {
            get { return (InstanceRuntimeDetails)base.GetValue(RuntimeDetailsProperty); }
            set
            {
                base.SetValue(RuntimeDetailsProperty, value);
                Populate(value);
            }
        }


        public static readonly BindableProperty InstanceTapCommandProperty = BindableProperty.Create(
                                    propertyName: nameof(InstanceTapCommand),
                                    returnType: typeof(ICommand),
                                    declaringType: typeof(InstanceRuntimeTreeControl),
                                    defaultValue: null,
                                    defaultBindingMode: BindingMode.Default,
                                    propertyChanged: HandleInstanceTapCommandAssigned);

        private static void HandleInstanceTapCommandAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var ctl = (InstanceRuntimeTreeControl)bindable;
            ctl.InstanceTapCommand = newValue as ICommand;
        }

        public ICommand InstanceTapCommand
        {
            get { return (ICommand)base.GetValue(InstanceTapCommandProperty); }
            set { base.SetValue(InstanceTapCommandProperty, value); }
        }

        public static readonly BindableProperty PlannerTapCommandProperty = BindableProperty.Create(
                            propertyName: nameof(PlannerTapCommand),
                            returnType: typeof(ICommand),
                            declaringType: typeof(InstanceRuntimeTreeControl),
                            defaultValue: null,
                            defaultBindingMode: BindingMode.Default,
                            propertyChanged: HandlePlannerTapCommandAssigned);

        private static void HandlePlannerTapCommandAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var ctl = (InstanceRuntimeTreeControl)bindable;
            ctl.PlannerTapCommand = newValue as ICommand;
        }

        public ICommand PlannerTapCommand
        {
            get { return (ICommand)base.GetValue(PlannerTapCommandProperty); }
            set { base.SetValue(PlannerTapCommandProperty, value); }
        }

        public static readonly BindableProperty ListenerTapCommandProperty = BindableProperty.Create(
                    propertyName: nameof(ListenerTapCommand),
                    returnType: typeof(ICommand),
                    declaringType: typeof(InstanceRuntimeTreeControl),
                    defaultValue: null,
                    defaultBindingMode: BindingMode.Default,
                    propertyChanged: HandlePlannerTapCommandAssigned);

        private static void HandleListenerTapCommandAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var ctl = (InstanceRuntimeTreeControl)bindable;
            ctl.ListenerTapCommand = newValue as ICommand;
        }

        public ICommand ListenerTapCommand
        {
            get { return (ICommand)base.GetValue(ListenerTapCommandProperty); }
            set { base.SetValue(ListenerTapCommandProperty, value); }
        }

        public static readonly BindableProperty PipelineModuleTapCommandProperty = BindableProperty.Create(
            propertyName: nameof(PipelineModuleTapCommand),
            returnType: typeof(ICommand),
            declaringType: typeof(InstanceRuntimeTreeControl),
            defaultValue: null,
            defaultBindingMode: BindingMode.Default,
            propertyChanged: HandlePipelineModuleTapCommandAssigned);

        private static void HandlePipelineModuleTapCommandAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var ctl = (InstanceRuntimeTreeControl)bindable;
            ctl.PipelineModuleTapCommand = newValue as ICommand;
        }

        public ICommand PipelineModuleTapCommand
        {
            get { return (ICommand)base.GetValue(PipelineModuleTapCommandProperty); }
            set { base.SetValue(PipelineModuleTapCommandProperty, value); }
        }

        public static readonly BindableProperty MessageTypeTapCommandProperty = BindableProperty.Create(
            propertyName: nameof(MessageTypeTapCommand),
            returnType: typeof(ICommand),
            declaringType: typeof(InstanceRuntimeTreeControl),
            defaultValue: null,
            defaultBindingMode: BindingMode.Default,
            propertyChanged: HandleMessageTypeTapCommandAssigned);

        private static void HandleMessageTypeTapCommandAssigned(BindableObject bindable, object oldValue, object newValue)
        {
            var ctl = (InstanceRuntimeTreeControl)bindable;
            ctl.MessageTypeTapCommand = newValue as ICommand;
        }

        public ICommand MessageTypeTapCommand
        {
            get { return (ICommand)base.GetValue(MessageTypeTapCommandProperty); }
            set { base.SetValue(MessageTypeTapCommandProperty, value); }
        }



        private void AddSectionHeader(int leftMargin, string iconKey, string header)
        {
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            var section = new StackLayout();
            var icon = new LagoVista.XPlat.Core.Icon();
            icon.IconKey = iconKey;
            icon.Margin = new Thickness(leftMargin, 5, 0, 0);
            icon.FontSize = 16;
            icon.TextColor = Color.FromRgb(0xD4, 0x8D, 0x17);

            var label = new Label();
            label.Margin = new Thickness(0, 0, 0, 0);
            label.Text = header;
            label.FontSize = 16;
            label.FontAttributes = FontAttributes.Bold;

            section.Orientation = StackOrientation.Horizontal;
            section.Children.Add(icon);
            section.Children.Add(label);

            section.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);

            Children.Add(section);
        }

        private void AddItem(int leftMargin, string name, string detail1, string detail2, string type, string id)
        {
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            var box = new BoxView();
            box.HeightRequest = 1;
            box.BackgroundColor = Color.LightGray;
            box.SetValue(Grid.ColumnSpanProperty, 4);
            box.Margin = new Thickness(leftMargin, 0, 0, 0);
            box.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
            Children.Add(box);

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            var label = new Label();
            label.SetValue(Grid.ColumnProperty, 0);
            label.Text = name;
            label.FontSize = 14;
            label.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
            label.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            label.Margin = new Thickness(leftMargin, 0, 0, 0);
            Children.Add(label);

            var detailLabel = new Label();
            detailLabel.Text = detail1;
            detailLabel.FontSize = 14;
            detailLabel.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
            detailLabel.SetValue(Grid.ColumnProperty, 1);
            detailLabel.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            Children.Add(detailLabel);

            var detail2Label = new Label();
            detail2Label.Text = detail2;
            detail2Label.FontSize = 14;
            detail2Label.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
            detail2Label.SetValue(Grid.ColumnProperty, 2);
            detail2Label.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            Children.Add(detail2Label);

            /*            if (!String.IsNullOrEmpty(type))
                        {
                            var link = new Label();
                            link.Text = "View";
                            var tapRecognizer = new TapGestureRecognizer();
                            tapRecognizer.Tapped += TapRecognizer_Tapped;
                            link.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
                            link.SetValue(Grid.ColumnProperty, 3);
                            link.BindingContext = new TappedItem() { Id = id, Type = type };
                            link.GestureRecognizers.Add(tapRecognizer);
                            Children.Add(link);
                        }*/
        }

        private void TapRecognizer_Tapped(object sender, EventArgs e)
        {
            var tapGen = sender as Label;
            var tappedItem = tapGen.BindingContext as TappedItem;
            switch (tappedItem.Type)
            {
                case INSTANCE_TYPE: InstanceTapCommand?.Execute(tappedItem.Id); break;
                case PLANNER_TYPE: PlannerTapCommand?.Execute(tappedItem.Id); break;
                case LISTENER_TYPE: ListenerTapCommand?.Execute(tappedItem.Id); break;
                case MESSAGETYPE_TYPE: MessageTypeTapCommand?.Execute(tappedItem.Id); break;
                case PIPELINEMODULE_TYPE: PipelineModuleTapCommand?.Execute(tappedItem.Id); break;
            }
        }

        const string INSTANCE_TYPE = "instance";
        const string PLANNER_TYPE = "planner";
        const string LISTENER_TYPE = "listener";
        const string MESSAGETYPE_TYPE = "messagetype";
        const string PIPELINEMODULE_TYPE = "pipelinemodule";

        private void AddRoute(RouteSummary route)
        {
            var routeHeader = new StackLayout();
            routeHeader.SetValue(Grid.ColumnSpanProperty, 4);
            routeHeader.Orientation = StackOrientation.Horizontal;
            routeHeader.Margin = new Thickness(40, 5, 0, 0);

            var icon = new LagoVista.XPlat.Core.Icon();
            icon.IconKey = "fa-map";
            icon.Margin = new Thickness(0, 5, 0, 0);
            icon.FontSize = 16;
            icon.TextColor = Color.FromRgb(0xD4, 0x8D, 0x17);
            routeHeader.Children.Add(icon);

            var routeLabel = new Label()
            {
                Text = "Route: ",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
            };
            routeHeader.Children.Add(routeLabel);

            var routeName = new Label()
            {
                Text = $"{route.Name} ({route.MessageTypes.First().Name})",
                FontSize = 16,
            };
            routeHeader.Children.Add(routeName);

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            routeHeader.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
            Children.Add(routeHeader);

            AddSectionHeader(50, "fa-list-ol", PlatformManagerResources.InstanceDetails_PipelineModules);
            foreach (var pipelienModule in route.PipelineModules)
            {
                AddItem(60, pipelienModule.Name, pipelienModule.Status.ToString(), pipelienModule.Type, PIPELINEMODULE_TYPE, pipelienModule.Id);
            }
        }

        private void AddDeviceConfiguration(DeviceConfigSummary deviceConfig)
        {
            var deviecConfigSection = new StackLayout();
            deviecConfigSection.SetValue(Grid.ColumnSpanProperty, 4);
            deviecConfigSection.Orientation = StackOrientation.Horizontal;
            deviecConfigSection.Margin = new Thickness(30, 5, 0, 0);

            var icon = new LagoVista.XPlat.Core.Icon();
            icon.IconKey = "fa-gears";
            icon.Margin = new Thickness(0, 5, 0, 0);
            icon.FontSize = 16;
            icon.TextColor = Color.FromRgb(0xD4, 0x8D, 0x17);
            deviecConfigSection.Children.Add(icon);

            var routeLabel = new Label()
            {
                Text = "Device Configuration: ",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
            };
            deviecConfigSection.Children.Add(routeLabel);

            var deviceConfigName = new Label()
            {
                Text = deviceConfig.Name,
                FontSize = 16,
            };
            deviecConfigSection.Children.Add(deviceConfigName);

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            deviecConfigSection.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);
            Children.Add(deviecConfigSection);

            foreach (var route in deviceConfig.Routes)
            {
                AddRoute(route);
            }
        }

        private void Populate(InstanceRuntimeDetails runtimeDetails)
        {
            Children.Clear();

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            var header = new Label();
            header.Text = runtimeDetails.Name;
            header.FontSize = 18;
            header.Margin = new Thickness(5, 0, 5, 0);
            header.SetValue(Grid.ColumnSpanProperty, 4);
            header.FontAttributes = FontAttributes.Bold;
            Children.Add(header);

            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            var statusLabelSection = new StackLayout();

            statusLabelSection.Orientation = StackOrientation.Horizontal;
            statusLabelSection.SetValue(Grid.ColumnSpanProperty, 4);
            statusLabelSection.SetValue(Grid.RowProperty, RowDefinitions.Count - 1);

            var statusLabel = new Label();
            statusLabel.Text = "Status:";
            statusLabel.FontSize = 16;
            statusLabel.Margin = new Thickness(5, 0, 5, 0);
            statusLabel.FontAttributes = FontAttributes.Bold;
            statusLabelSection.Children.Add(statusLabel);

            var label = new Label();
            label.Text = runtimeDetails.Status.ToString();
            label.FontSize = 16;
            statusLabelSection.Children.Add(label);

            Children.Add(statusLabelSection);

            AddSectionHeader(5, "fa-headphones", PlatformManagerResources.InstanceDetails_Listeners);
            foreach (var listener in runtimeDetails.Listeners)
            {
                AddItem(30, listener.Name, listener.Status.ToString(), listener.ListenerType, LISTENER_TYPE, listener.Id);
            }

            AddSectionHeader(5, "fa-share-alt", PlatformManagerResources.InstanceDetails_Planner);
            AddItem(30, runtimeDetails.Planner.Name, runtimeDetails.Planner.Status.ToString(), "", PLANNER_TYPE, runtimeDetails.Planner.Id);

            foreach (var deviceConfig in runtimeDetails.DeviceConfigurations)
            {
                AddDeviceConfiguration(deviceConfig);
            }


        }

        public class TappedItem
        {
            public String Id { get; set; }
            public String Type { get; set; }
        }
    }
}
