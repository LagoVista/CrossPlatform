using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Geo;
using LagoVista.IoT.DeviceManagement.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace LagoVista.XPlat.Core
{
    public class BindableMap : Map
    {
        public BindableMap()
        {
            PinsSource = new ObservableCollection<Pin>();
            PinsSource.CollectionChanged += PinsSourceOnCollectionChanged;
            base.MapClicked += BindableMap_MapClicked;
        }

        private void BindableMap_MapClicked(object sender, MapClickedEventArgs e)
        {
            if(_mapTappedCommand != null)
            {
                _mapTappedCommand.Execute(new GeoLocation(e.Position.Latitude, e.Position.Longitude));
            }
        }

        #region PinSource Property
        public ObservableCollection<Pin> PinsSource
        {
            get { return (ObservableCollection<Pin>)GetValue(PinsSourceProperty); }
            set { SetValue(PinsSourceProperty, value); }
        }

        public static readonly BindableProperty PinsSourceProperty = BindableProperty.Create(
                                                         propertyName: "PinsSource",
                                                         returnType: typeof(ObservableCollection<Pin>),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         validateValue: null,
                                                         propertyChanged: PinsSourcePropertyChanged);


        private static void PinsSourcePropertyChanged(BindableObject bindable, object oldvalue, object newValue)
        {
            var thisInstance = bindable as BindableMap;
            var newPinsSource = newValue as ObservableCollection<Pin>;

            if (thisInstance == null ||
                newPinsSource == null)
                return;

            UpdatePinsSource(thisInstance, newPinsSource);
        }
        private void PinsSourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePinsSource(this, sender as IEnumerable<Pin>);
        }

        private static void UpdatePinsSource(Map bindableMap, IEnumerable<Pin> newSource)
        {
            bindableMap.Pins.Clear();
            foreach (var pin in newSource)
                bindableMap.Pins.Add(pin);
        }
        #endregion

        #region MapSpan Property
        public MapSpan MapSpan
        {
            get { return (MapSpan)GetValue(MapSpanProperty); }
            set { SetValue(MapSpanProperty, value); }
        }

        public static readonly BindableProperty MapSpanProperty = BindableProperty.Create(
                                                         propertyName: "MapSpan",
                                                         returnType: typeof(MapSpan),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         validateValue: null,
                                                         propertyChanged: MapSpanPropertyChanged);

        private static void MapSpanPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var thisInstance = bindable as BindableMap;
            var newMapSpan = newValue as MapSpan;

            thisInstance?.MoveToRegion(newMapSpan);
        }
        #endregion

        #region MapCenter Property
        public GeoLocation MapCenter
        {
            get => (GeoLocation)GetValue(MapCenterProperty);
            set => SetValue(MapCenterProperty, value);
        }

        public static readonly BindableProperty MapCenterProperty = BindableProperty.Create(
                                                         propertyName: nameof(MapCenter),
                                                         returnType: typeof(GeoLocation),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         validateValue: null,
                                                         propertyChanged: MapCenterPropertyChanged);

        public static void MapCenterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var thisInstance = bindable as BindableMap;
            var newCenter = newValue as GeoLocation;
            if (newCenter != null && newCenter.HasLocation)
            {
                thisInstance.MapSpan = new MapSpan(new Position(newCenter.Latitude.Value, newCenter.Longitude.Value), 0.5, 0.5);
            }
        }
        #endregion

        #region Geo Fences Property
        public ObservableCollection<GeoFence> GeoFences
        {
            get => (ObservableCollection<GeoFence>)GetValue(GeoFencesProperty);
            set => SetValue(GeoFencesProperty, value);
        }

        public static readonly BindableProperty GeoFencesProperty = BindableProperty.Create(
                                                         propertyName: nameof(GeoFences),
                                                         returnType: typeof(ObservableCollection<GeoFence>),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         validateValue: null,
                                                         propertyChanged: GeoFencesPropertyChanged);

        public static void GeoFencesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var thisInstance = bindable as BindableMap;
            var oldGeoFences = newValue as ObservableCollection<GeoFence>;
            var newGeoFences = newValue as ObservableCollection<GeoFence>;
            if (oldGeoFences != null)
            {
                oldGeoFences.CollectionChanged -= thisInstance.GeoFences_CollectionChanged;
            }

            if (newGeoFences != null)
            {
                newGeoFences.CollectionChanged += thisInstance.GeoFences_CollectionChanged;
                thisInstance.RenderGeoFences();
            }
            else
            {
                thisInstance.MapElements.Clear();
            }
        }

        List<Circle> _circles = new List<Circle>();

        private void RenderGeoFences()
        {
            foreach (var circle in _circles)
            {
                MapElements.Remove(circle);
            }

            _circles.Clear();

            foreach (var fence in GeoFences)
            {
                var circle = new Circle()
                {
                    Center = new Position(fence.Center.Latitude.Value, fence.Center.Longitude.Value),
                    Radius = new Distance(fence.RadiusMeters)
                };
                _circles.Add(circle);
                MapElements.Add(circle);
            };
        }

        private void GeoFences_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RenderGeoFences();
        }

        #endregion

        #region Current GeoFence Property
        public GeoFence CurrentGeoFences
        {
            get => (GeoFence)GetValue(CurrentGeoFenceProperty);
            set => SetValue(CurrentGeoFenceProperty, value);
        }

        public static readonly BindableProperty CurrentGeoFenceProperty = BindableProperty.Create(
                                                         propertyName: nameof(GeoFences),
                                                         returnType: typeof(GeoFence),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         validateValue: null,
                                                         propertyChanged: CurrentGeoFencePropertyChanged);

        Circle _currentGeoFence;

        public static void CurrentGeoFencePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var thisInstance = bindable as BindableMap;
            var geoFence = newValue as GeoFence;

            if (thisInstance._currentGeoFence != null)
            {
                thisInstance.MapElements.Remove(thisInstance._currentGeoFence);
            }
        }
        #endregion  

        #region Map Tapped Command
        RelayCommand<GeoLocation> _mapTappedCommand;

        public static readonly BindableProperty MapTappedCommandProperty = BindableProperty.Create(
                                                         propertyName: nameof(MapTappedCommand),
                                                         returnType: typeof(RelayCommand<GeoLocation>),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.OneWay,
                                                         validateValue: null,
                                                         propertyChanged: MapTappedCommandPropertyChanged);

        public static void MapTappedCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var thisInstance = bindable as BindableMap;
            thisInstance._mapTappedCommand = newValue as RelayCommand<GeoLocation>;
        }

        public RelayCommand<GeoLocation> MapTappedCommand
        {
            get => (RelayCommand<GeoLocation>)GetValue(MapTappedCommandProperty);
            set => SetValue(MapTappedCommandProperty, value);
        }
        #endregion
    }
}
