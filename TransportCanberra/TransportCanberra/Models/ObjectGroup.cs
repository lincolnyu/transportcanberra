using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TransportCanberra.Utility;
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

        public ObjectGroup(Predicate<GeoObject> filter)
        {
            ObjectFilter = filter;
            GeoObject.PositionChangedGlobal += OnPositionChanged;
        }

        public Predicate<GeoObject> ObjectFilter { get; }

        public List<GeoObject> Objects { get; } = new List<GeoObject>();

        public ObservableCollection<GeoObject> ObjectsInView { get; } = new ObservableCollection<GeoObject>();

        public GeoboundingBox BoundingBox { get; private set; }

        public void AddObject(GeoObject obj)
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

        public void RemoveObject(GeoObject obj)
        {
            ObjectsInView.Remove(obj);
            Objects.Remove(obj);
        }

        public void ClearObjects()
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
        
        public void SetViewRegion(GeoboundingBox bounds)
        {
            if (BoundingBox != bounds)
            {
                BoundingBox = bounds;
                ResetObjectsInView();
            }
        }

        private bool ObjectIsInViewRegion(GeoObject obj)
        {
            if (BoundingBox == null) return true;
            return BoundingBox.Contains(obj.Point.Position);
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

        private void ResetObjectsInView()
        {
            while (ObjectsInView.Count > 0)
            {
                ObjectsInView.RemoveAt(ObjectsInView.Count - 1);
            }
            foreach (var o in Objects.Where(ObjectIsInViewRegion))
            {
                ObjectsInView.Add(o);
            }
        }
    }
}
