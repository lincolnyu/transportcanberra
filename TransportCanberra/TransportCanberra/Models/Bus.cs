using Windows.Devices.Geolocation;

namespace TransportCanberra.Models
{
    public class Bus : GeoObject
    {
        public string Code
        {
            get; set;
        }
        
        public string Description
        {
            get; set;
        }
    }
}
