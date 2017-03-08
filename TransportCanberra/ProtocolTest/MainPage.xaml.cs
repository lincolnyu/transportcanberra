using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Xml;
using System.IO;
using System;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.Collections.Generic;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ProtocolTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void SendOnClick(object sender, RoutedEventArgs e)
        {
            await WriteMessage();
        }

        private async Task WriteMessage()
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("XML", new List<string> { ".xml" });
            picker.SuggestedFileName = "test";
            picker.DefaultFileExtension = ".xml";
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
            };
            var file = await picker.PickSaveFileAsync();
            using (var sw = await file.OpenStreamForWriteAsync())
            using (var x = XmlWriter.Create(sw, settings))
            {
                x.WriteStartElement("Siri", "http://www.siri.org.uk/siri");
                x.WriteAttributeString(null, "version", null, "2.0");
                x.WriteAttributeString("xmlns", "ns", null, "http://www.ifopt.org.uk/acsb");
                x.WriteAttributeString("xmlns", "", null, "http://www.siri.org.uk/siri");
                x.WriteAttributeString("xmlns", "ns4", null, "http://datex2.eu/schema/2_0RC1/2_0");
                x.WriteAttributeString("xmlns", "ns3", null, "http://www.ifopt.org.uk/ifopt");
                x.WriteStartElement("ServiceRequest");
                x.WriteEndElement();
                x.WriteEndElement();
            }
        }
    }
}
