﻿namespace TransportCanberra.Models
{
    public class BusStop : GeoObject
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
