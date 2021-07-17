using LagoVista.Core.Models.Geo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
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
        }

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
    }
}
