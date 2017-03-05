using Windows.Devices.Geolocation;

namespace TransportCanberra.Models
{
    public class GeoObject
    {
        public delegate void PositionChangedEventHandler(GeoObject bus);

        private Geopoint _point;

        public Geopoint Point
        {
            get { return _point; }
            set
            {
                _point = value;
                PositionChangedGlobal?.Invoke(this);
                PositionChanged?.Invoke(this);
            }
        }

        public static event PositionChangedEventHandler PositionChangedGlobal;

        public event PositionChangedEventHandler PositionChanged;

        public void MoveTo(BasicGeoposition pos)
        {
            Point = new Geopoint(pos);
        }

        public void MoveTo(double latitude, double longitude)
        {
            var pos = new BasicGeoposition { Latitude = latitude, Longitude = longitude };
            MoveTo(pos);
        }
    }
}
