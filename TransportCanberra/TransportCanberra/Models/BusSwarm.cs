using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Geolocation;

namespace TransportCanberra.Models
{
    public class BusSwarm
    {
        public enum ChangeTypes
        {
            Added,
            Moved,
            Removed,
        }

        private bool _restrictBusInView = false;

        public delegate void BusChangedEventHandler(Bus bus, ChangeTypes type);

        public BusSwarm()
        {
            Bus.BusPositionChanged += OnBusPositionChanged;
        }

        public List<Bus> Buses { get; } = new List<Bus>();

        public ObservableCollection<Bus> BusesInView { get; } = new ObservableCollection<Bus>();

        public bool RestrictBusesInView
        {
            get { return _restrictBusInView; }
            set
            {
                if (_restrictBusInView != value)
                {
                    _restrictBusInView = value;
                    ResetBusesInView();
                }
            }
        }
        public double ViewMinLatitude { get; private set; }
        public double ViewMaxLatitude { get; private set; }
        public double ViewMinLongitude { get; private set; }
        public double ViewMaxLongitude { get; private set; }

        public void AddBus(Bus bus)
        {
            if (!Buses.Contains(bus))
            {
                Buses.Add(bus);
                if (BusIsInViewRegion(bus))
                {
                    BusesInView.Add(bus);
                }
            }
        }

        public void RemoveBus(Bus bus)
        {
            BusesInView.Remove(bus);
            Buses.Remove(bus);
        }

        public void ClearBuses()
        {
            while (BusesInView.Count > 0)
            {
                BusesInView.RemoveAt(BusesInView.Count - 1);
            }
            Buses.Clear();
        }

        public void MoveBusTo(Bus bus, BasicGeoposition pos)
        {
            bus.MoveTo(pos);
        }

        public void MoveBusTo(Bus bus, double latitude, double longitude)
        {
            bus.MoveTo(latitude, longitude);
        }

        public void SetViewRegion(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
        {
            ViewMinLatitude = minLatitude;
            ViewMaxLatitude = maxLatitude;
            ViewMinLongitude = minLongitude;
            ViewMaxLongitude = maxLongitude;

            ResetBusesInView();
        }

        private bool BusIsInViewRegion(Bus bus)
        {
            if (!_restrictBusInView) return true;
            var lati = bus.Point.Position.Latitude;
            var longi = bus.Point.Position.Longitude;
            return lati >= ViewMinLatitude && lati <= ViewMaxLatitude && longi >= ViewMinLongitude && longi <= ViewMaxLongitude;
        }

        private void OnBusPositionChanged(Bus bus)
        {
            // TODO optimize with indexing
            if (BusIsInViewRegion(bus))
            {
                if (!BusesInView.Contains(bus))
                {
                    BusesInView.Add(bus);
                }
            } 
            else
            {
                BusesInView.Remove(bus);
            }
        }

        private void ResetBusesInView()
        {
            while (BusesInView.Count > 0)
            {
                BusesInView.RemoveAt(BusesInView.Count - 1);
            }
            foreach (var bus in Buses.Where(BusIsInViewRegion))
            {
                BusesInView.Add(bus);
            }
        }
    }
}
