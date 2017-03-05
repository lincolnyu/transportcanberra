#define RANDOM_POF

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;
using TransportCanberra.Models;
using Windows.Devices.Geolocation;
using TransportCanberra.GeoServices;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TransportCanberra
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MapIcon _locationPin;
        private GeolocationService _geolocation;
        private Dictionary<Bus, MapIcon> _busToIcon = new Dictionary<Bus, MapIcon>();
        private Dictionary<BusStop, MapIcon> _busStopToIcon = new Dictionary<BusStop, MapIcon>();

        private ObjectGroup _busSwarm = new ObjectGroup(o=>o is Bus);
        private BusStops _busStops = new BusStops();

        private RandomAccessStreamReference _busIconResource;
        private RandomAccessStreamReference _busStopIconResource;

        public MainPage()
        {
            InitializeComponent();

            _busSwarm.ObjectsInView.CollectionChanged += BusesInViewOnCollectionChanged;

            _busStops.ObjectsInView.CollectionChanged += BusStopsInViewOnCollectionChanged;
            _busStops.Load();
        }

        public Geopoint CurrentLocation => _locationPin?.Location;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_geolocation == null)
            {
                _geolocation = new GeolocationService();
                _geolocation.PositionUpdated += GeolocationPositionUpdated;
                _geolocation.Initialize();
            }

            if (_busIconResource == null)
            {
                _busIconResource = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/bus.png"));
            }

            if (_busStopIconResource == null)
            {
                _busStopIconResource = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/stop.png"));
            }
        }

        private async void GeolocationPositionUpdated(Geoposition position, GeolocationService.PositionUpdateReasons reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    if (position != null)
                    {
                        await CanberraMap.TrySetViewAsync(position.Coordinate.Point, 18);
                        if (_locationPin == null)
                        {
                            _locationPin = new MapIcon();
                            _locationPin.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/me.png"));
                            //  _locationPin.NormalizedAnchorPoint = new Point(0.5, 1.0);
                            _locationPin.ZIndex = 0;
                            CanberraMap.MapElements.Add(_locationPin);
                        }
                        _locationPin.Location = position.Coordinate.Point;
                    }
                });
        }

        private void BusesInViewOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var b in e.NewItems.Cast<Bus>())
                    {
                        b.PositionChanged += UpdateBus;
                        UpdateBus(b);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var b in e.OldItems.Cast<Bus>())
                    {
                        b.PositionChanged -= UpdateBus;
                        RemoveBus(b);
                    }
                    break;
            }
        }

        private void BusStopsInViewOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var b in e.NewItems.Cast<BusStop>())
                    {
                        b.PositionChanged += UpdateBusStop;
                        UpdateBusStop(b);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var b in e.OldItems.Cast<BusStop>())
                    {
                        b.PositionChanged -= UpdateBusStop;
                        RemoveBusStop(b);
                    }
                    break;
            }
        }

        private void UpdateBusStop(GeoObject obj)
        {
            MapIcon busStopIcon;
            var busStop = (BusStop)obj;
            if (!_busStopToIcon.TryGetValue(busStop, out busStopIcon))
            {
                busStopIcon = new MapIcon();
                busStopIcon.ZIndex = 1;
                busStopIcon.Title = busStop.Id;
                busStopIcon.Image = _busStopIconResource;
                // TODO image etc...
                _busStopToIcon.Add(busStop, busStopIcon);
                CanberraMap.MapElements.Add(busStopIcon);
            }
            busStopIcon.Location = busStop.Point;
        }

        private void RemoveBusStop(BusStop bs)
        {
            MapIcon busStopIcon;
            if (_busStopToIcon.TryGetValue(bs, out busStopIcon))
            {
                CanberraMap.MapElements.Remove(busStopIcon);
                _busStopToIcon.Remove(bs);
            }
        }

        private void RemoveBus(Bus bus)
        {
            MapIcon busIcon;
            if (_busToIcon.TryGetValue(bus, out busIcon))
            {
                CanberraMap.MapElements.Remove(busIcon);
                _busToIcon.Remove(bus);
            }
        }

        private void UpdateBus(GeoObject obj)
        {
            MapIcon busIcon;
            var bus = (Bus)obj;
            if (!_busToIcon.TryGetValue(bus, out busIcon))
            {
                busIcon = new MapIcon();
                busIcon.ZIndex = 1;
                busIcon.Title = bus.Code;
                busIcon.Image = _busIconResource;
                // TODO image etc...
                _busToIcon.Add(bus, busIcon);
                CanberraMap.MapElements.Add(busIcon);
            }
            busIcon.Location = bus.Point;
        }
        
        private async void BtnZoomToOriginOnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentLocation != null)
            {
                if (CanberraMap.ZoomLevel < 15)
                {
                    await CanberraMap.TrySetViewAsync(CurrentLocation, 15);
                }
                else
                {
                    await CanberraMap.TrySetViewAsync(CurrentLocation);
                }
            }
        }

#if RANDOM_POF

        private Timer _simTimer;

        private void GenerateRandomBusesAroundMe(int n, double radius)
        {
            var rand = new Random();
            for (var i = 0; i < n; i++)
            {
                var pos = CurrentLocation.Position;
                var bus = new Bus
                {
                    Code = rand.Next(1, 1000).ToString()
                };
                var r = radius * rand.NextDouble();
                var a = 2 * Math.PI * rand.NextDouble();
                bus.MoveTo(pos.Latitude + r * Math.Cos(a), pos.Longitude + r * Math.Sin(a));
                _busSwarm.AddBus(bus);
            }
        }

        private void ClearAllBuses()
        {
            _busSwarm.ClearBuses();
        }

        private void BtnTestOnClick(object sender, RoutedEventArgs e)
        {
            if (_simTimer != null)
            {
                _simTimer.Dispose();
                _simTimer = null;
                ClearAllBuses();
            }
            else
            {
                GenerateRandomBusesAroundMe(100, 0.02);
                const int Interval = 500;
                _simTimer = new Timer(TestOnTimer, null, Interval, Interval);
            }

        }

        private async void TestOnTimer(object state)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    var rand = new Random();
                    const double maxmove = 0.001;
                    foreach (var b in _busSwarm.Objects)
                    {
                        var lati = b.Point.Position.Latitude;
                        var longi = b.Point.Position.Longitude;
                        var move = maxmove * rand.NextDouble();
                        var a = 2 * Math.PI * rand.NextDouble();
                        var newlati = lati + move * Math.Cos(a);
                        var newlongi = longi + move * Math.Sin(a);
                        _busSwarm.MoveObjectTo(b, newlati, newlongi);
                    }
                });
        }

#endif
    }
}
