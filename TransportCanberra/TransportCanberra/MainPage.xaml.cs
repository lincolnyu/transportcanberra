using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TransportCanberra.GeoServices;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;
using Windows.Storage.Streams;

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

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_geolocation == null)
            {
                _geolocation = new GeolocationService();
                _geolocation.PositionUpdated += GeolocationPositionUpdated;
                _geolocation.Initialize();
            }
        }

        private async void GeolocationPositionUpdated(Windows.Devices.Geolocation.Geoposition position, GeolocationService.PositionUpdateReasons reason)
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
    }
}
