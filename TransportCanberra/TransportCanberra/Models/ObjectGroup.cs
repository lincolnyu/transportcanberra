using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Geolocation;

namespace TransportCanberra.Models
{
    public class ObjectGroup
    {
        public enum ChangeTypes
        {
            Added,
            Moved,
            Removed,
        }

        private bool _restrictInView = false;

        public delegate void ObjectChangedEventHandler(Bus bus, ChangeTypes type);

        public ObjectGroup(Predicate<GeoObject> filter)
        {
            ObjectFilter = filter;
            GeoObject.PositionChangedGlobal += OnPositionChanged;
        }

        public Predicate<GeoObject> ObjectFilter { get; }

        public List<GeoObject> Objects { get; } = new List<GeoObject>();

        public ObservableCollection<GeoObject> ObjectsInView { get; } = new ObservableCollection<GeoObject>();

        public bool RestrictBusesInView
        {
            get { return _restrictInView; }
            set
            {
                if (_restrictInView != value)
                {
                    _restrictInView = value;
                    ResetBusesInView();
                }
            }
        }
        public double ViewMinLatitude { get; private set; }
        public double ViewMaxLatitude { get; private set; }
        public double ViewMinLongitude { get; private set; }
        public double ViewMaxLongitude { get; private set; }

        public void AddBus(GeoObject obj)
        {
            if (!Objects.Contains(obj))
            {
                Objects.Add(obj);
                if (ObjectIsInViewRegion(obj))
                {
                    ObjectsInView.Add(obj);
                }
            }
        }

        public void RemoveBus(Bus bus)
        {
            ObjectsInView.Remove(bus);
            Objects.Remove(bus);
        }

        public void ClearBuses()
        {
            while (ObjectsInView.Count > 0)
            {
                ObjectsInView.RemoveAt(ObjectsInView.Count - 1);
            }
            Objects.Clear();
        }

        public void MoveObjectTo(GeoObject obj, BasicGeoposition pos)
        {
            obj.MoveTo(pos);
        }

        public void MoveObjectTo(GeoObject obj, double latitude, double longitude)
        {
            obj.MoveTo(latitude, longitude);
        }

        public void SetViewRegion(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
        {
            ViewMinLatitude = minLatitude;
            ViewMaxLatitude = maxLatitude;
            ViewMinLongitude = minLongitude;
            ViewMaxLongitude = maxLongitude;

            ResetBusesInView();
        }

        private bool ObjectIsInViewRegion(GeoObject obj)
        {
            if (!_restrictInView) return true;
            var lati = obj.Point.Position.Latitude;
            var longi = obj.Point.Position.Longitude;
            return lati >= ViewMinLatitude && lati <= ViewMaxLatitude && longi >= ViewMinLongitude && longi <= ViewMaxLongitude;
        }

        private void OnPositionChanged(GeoObject obj)
        {
            if (!ObjectFilter(obj)) return;
            // TODO optimize with indexing
            if (ObjectIsInViewRegion(obj))
            {
                if (!ObjectsInView.Contains(obj))
                {
                    ObjectsInView.Add(obj);
                }
            } 
            else
            {
                ObjectsInView.Remove(obj);
            }
        }

        private void ResetBusesInView()
        {
            while (ObjectsInView.Count > 0)
            {
                ObjectsInView.RemoveAt(ObjectsInView.Count - 1);
            }
            foreach (var bus in Objects.Where(ObjectIsInViewRegion))
            {
                ObjectsInView.Add(bus);
            }
        }
    }
}
