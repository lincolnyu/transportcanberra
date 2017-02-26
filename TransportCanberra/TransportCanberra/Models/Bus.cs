using Windows.Devices.Geolocation;

namespace TransportCanberra.Models
{
    public class Bus
    {
        private Geopoint _point;

        public delegate void BusPositionChangedEventHandler(Bus bus);

        public Geopoint Point
        {
            get { return _point; }
            set
            {
                _point = value;
                BusPositionChanged?.Invoke(this);
                PositionChanged?.Invoke(this);
            }
        }

        public string Code
        {
            get; set;
        }
        
        public string Description
        {
            get; set;
        }

        public static event BusPositionChangedEventHandler BusPositionChanged;

        public event BusPositionChangedEventHandler PositionChanged;

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
