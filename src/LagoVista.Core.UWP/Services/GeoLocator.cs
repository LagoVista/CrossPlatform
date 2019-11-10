using LagoVista.Core.Geo;
using LagoVista.Core.Models.Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace LagoVista.Core.UWP.Services
{
    public class GeoLocator : IGeoLocator
    {
        Geolocator _geolocator;

        public event EventHandler<IGeoLocation> LocationUpdated;

        public bool HasLocation { get; private set; }

        public bool HasLocationAccess { get; private set; }

        public IGeoLocation CurrentLocation { get; private set; }


        public async Task InitAsync()
        {


            var accessStatus = await Geolocator.RequestAccessAsync();
            HasLocationAccess = accessStatus == GeolocationAccessStatus.Allowed;
            if (HasLocation)
            {
                _geolocator = new Geolocator { DesiredAccuracyInMeters = 1 };
                _geolocator.PositionChanged += _geolocator_PositionChanged;
                var pos = await _geolocator.GetGeopositionAsync();
                CurrentLocation = new GeoLocation(pos.Coordinate.Point.Position.Latitude,
                        pos.Coordinate.Point.Position.Longitude,
                        pos.Coordinate.Point.Position.Altitude,
                        pos.Coordinate.Heading,
                        pos.Coordinate.SatelliteData.HorizontalDilutionOfPrecision,
                        pos.Coordinate.SatelliteData.VerticalDilutionOfPrecision,
                        pos.Coordinate.Accuracy);

                HasLocation = CurrentLocation.Latitude.HasValue && CurrentLocation.Longitude.HasValue;
            }
        }

        private void _geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var pos = args.Position;
            CurrentLocation = new GeoLocation(pos.Coordinate.Point.Position.Latitude,
                pos.Coordinate.Point.Position.Longitude,
                pos.Coordinate.Point.Position.Altitude,
                pos.Coordinate.Heading,
                pos.Coordinate.SatelliteData.HorizontalDilutionOfPrecision,
                pos.Coordinate.SatelliteData.VerticalDilutionOfPrecision,
                pos.Coordinate.Accuracy);

            HasLocation = CurrentLocation.Latitude.HasValue && CurrentLocation.Longitude.HasValue;
        }
    }
}
