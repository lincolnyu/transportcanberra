using System;
using Windows.Devices.Geolocation;

namespace TransportCanberra.GeoServices
{
    public class GeolocationService
    {
        public enum PositionUpdateReasons
        {
            Normal,
            Disabled,
            AccessDenied,
            AccessUnspecified
        }

        public delegate void StatusChangedEventHandler(Geolocator sender, StatusChangedEventArgs args);

        public delegate void PositionUpdatedEventHandler(Geoposition position, PositionUpdateReasons reason);

        public GeolocationService(uint reportIntervalMilliseconds = 1000, uint? desiredAccuracyInMeters = null)
        {
            DesireAccuracyInMetersValue = desiredAccuracyInMeters;
            ReportIntervalMilliseconds = reportIntervalMilliseconds;
        }

        public uint? DesireAccuracyInMetersValue { get; }
        
        public uint ReportIntervalMilliseconds { get; }

        public event StatusChangedEventHandler StatusChanged;

        public event PositionUpdatedEventHandler PositionUpdated;

        public async void Initialize()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    var geolocator = new Geolocator
                    {
                        DesiredAccuracyInMeters = DesireAccuracyInMetersValue,
                        ReportInterval = ReportIntervalMilliseconds
                    };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    geolocator.StatusChanged += OnStatusChanged;
                    geolocator.PositionChanged += OnPositionChanged;

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    PositionUpdated?.Invoke(pos, PositionUpdateReasons.Normal);

                    break;
                case GeolocationAccessStatus.Denied:
                    PositionUpdated?.Invoke(null, PositionUpdateReasons.AccessDenied);
                    break;
                case GeolocationAccessStatus.Unspecified:
                    PositionUpdated?.Invoke(null, PositionUpdateReasons.AccessUnspecified);
                    break;
            }
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            PositionUpdated?.Invoke(args.Position, PositionUpdateReasons.Normal);
        }

        private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            if (args.Status == PositionStatus.Disabled)
            {
                PositionUpdated?.Invoke(null, PositionUpdateReasons.AccessDenied);
            }

            StatusChanged?.Invoke(sender, args);
        }
    }
}
