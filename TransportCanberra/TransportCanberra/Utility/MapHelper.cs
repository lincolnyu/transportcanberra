using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace TransportCanberra.Utility
{
    public static class MapHelper
    {
        public static GeoboundingBox GetBounds(this MapControl map)
        {
            try
            {
                return map.GetBoundsNormal();
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        ///  Returns the boundingbox of the current map view
        /// </summary>
        /// <param name="map">The map</param>
        /// <returns>The bounding box</returns>
        /// <references>
        /// http://stackoverflow.com/questions/24468236/get-view-bounds-of-a-map
        /// </references>
        private static GeoboundingBox GetBoundsNormal(this MapControl map)
        {
            Geopoint topLeft = null;

            try
            {
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, 0), out topLeft);
            }
            catch
            {
                var topOfMap = new Geopoint(new BasicGeoposition()
                {
                    Latitude = 85,
                    Longitude = 0
                });

                Windows.Foundation.Point topPoint;
                map.GetOffsetFromLocation(topOfMap, out topPoint);
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, topPoint.Y), out topLeft);
            }

            Geopoint bottomRight = null;
            try
            {
                map.GetLocationFromOffset(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight), out bottomRight);
            }
            catch
            {
                var bottomOfMap = new Geopoint(new BasicGeoposition()
                {
                    Latitude = -85,
                    Longitude = 0
                });

                Windows.Foundation.Point bottomPoint;
                map.GetOffsetFromLocation(bottomOfMap, out bottomPoint);
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, bottomPoint.Y), out bottomRight);
            }

            if (topLeft != null && bottomRight != null)
            {
                return new GeoboundingBox(topLeft.Position, bottomRight.Position);
            }

            return null;
        }

        public static bool Contains(this GeoboundingBox box, BasicGeoposition pos)
        {
            if (box.NorthwestCorner.Longitude <= box.SoutheastCorner.Longitude)
            {
                if (pos.Longitude < box.NorthwestCorner.Longitude || pos.Longitude > box.SoutheastCorner.Longitude) return false;
            }
            else
            {
                if (pos.Longitude < box.NorthwestCorner.Longitude && pos.Longitude > box.SoutheastCorner.Longitude) return false;
            }

            return (pos.Latitude <= box.NorthwestCorner.Latitude && pos.Latitude >= box.SoutheastCorner.Latitude);
        }

        public static double DistanceInView(this GeoboundingBox box, double viewWidth, double viewHeight, BasicGeoposition pos1, BasicGeoposition pos2)
        {
            throw new System.NotImplementedException();
        }
    }
}
