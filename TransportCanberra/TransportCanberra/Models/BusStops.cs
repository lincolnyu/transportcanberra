using System;
using System.IO;
using Windows.Storage;

namespace TransportCanberra.Models
{
    public class BusStops : ObjectGroup
    {
        public BusStops() : base(o=>o is BusStop)
        {
        }

        public async void Load()
        {
            var fs = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TransitData/stops.txt"));

            using (var s = await fs.OpenReadAsync())
            using (var cs = s.AsStreamForRead())
            using (var sr = new StreamReader(cs))
            {
                var lc = 0;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) break;
                    var split = line.Split(',');
                    if (lc > 0)
                    {
                        var stop = new BusStop
                        {
                            Id = split[0],
                            Code = split[1],
                            Name = split[2].Trim('"'),
                            Description = split[3]
                        };
                        var lat = double.Parse(split[4]);
                        var lon = double.Parse(split[5]);
                        stop.MoveTo(lat, lon);
                        AddObject(stop);
                    }
                    lc++;
                }
            }

        }
    }
}
