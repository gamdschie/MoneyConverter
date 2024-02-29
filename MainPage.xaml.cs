using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace MoneyConverterFalchetto
{
    public partial class MainPage : ContentPage
    {
        private List<XmlDaten> raten;

        public MainPage()
        {
            InitializeComponent();
            WaehrungsPicker.SelectedIndexChanged+=WaehrungPickerSelectedIndexChanged;
            EntryEingeben.TextChanged += ValueEntryTextChanged;
            //ich hatte hier probleme weil meine app die .xml datei mit dem relativen path nicht finden konnte
            raten = XmlAuslesen.Loaden();
            WaehrungsPicker.ItemsSource = raten.Select(r => r.Currency).ToList();
        }

        private void WaehrungPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            GeldUmwandeln();
        }

        private void ValueEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            GeldUmwandeln();
        }

        public void GeldUmwandeln()
        {
            if (WaehrungsPicker.SelectedItem is not null && decimal.TryParse(EntryEingeben.Text, out var value))
            {
                var waehrung = WaehrungsPicker.SelectedItem.ToString();
                var multiplikator = raten.FirstOrDefault(r => r.Currency == waehrung)?.Rate ?? 0M;
                var neueWaehrung = value * multiplikator;
                EntryUmgewandelt.Text = neueWaehrung.ToString("N2");
            }
            else EntryUmgewandelt.Text="Bitte eine Währung aussuchen und Geldbetrag eingeben";
        }
    }

    //die daten im xml file bestehen aus currency (das ist ein string) und rate (das ist ein decimal)
    class XmlDaten
    {
        public string Currency { get; set; }
        public decimal Rate { get; set; }
    }

    //hier loade ich einfach das file und parse die daten aus dem xml file in eine liste    
    class XmlAuslesen
    {
        public static List<XmlDaten> Loaden()
        {
            var resourceName = "MoneyConverterFalchetto.Resources.Data.daten.xml";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream is not null)
                {
                    XDocument document = XDocument.Load(stream);
                    XNamespace namesp = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";
                    var data = document.Descendants(namesp + "Cube")
                                   .Where(x => x.Attribute("currency") != null)
                                   .Select(x => new XmlDaten
                                   {
                                       Currency = x.Attribute("currency").Value,
                                       Rate = Convert.ToDecimal(x.Attribute("rate").Value, CultureInfo.InvariantCulture)
                                   }).ToList();
                    return data;
                }
                else return null;
            }
        }
    }
}
