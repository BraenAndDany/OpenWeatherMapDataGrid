using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;

namespace OpenWeatherMapDataGrid
{
    public partial class MainWindow : Window
    {
        private string[] QueryCodes = { "q", "zip", "id", };

        public MainWindow()
        {
            InitializeComponent();
        }
        // api key is 32 characters long and this is just a sample.
        // you need to get your own free one
        private const string API_KEY = "0065e4b8a4a698aa6b05bfd95c5f21fc";

        // Query URLs. Replace @LOC@ with the location.
        private const string CurrentUrl =
            "http://api.openweathermap.org/data/2.5/weather?" +
            "@QUERY@=@LOC@&mode=xml&units=imperial&APPID=" + API_KEY;
        private const string ForecastUrl =
            "http://api.openweathermap.org/data/2.5/forecast?" +
            "@QUERY@=@LOC@&mode=xml&units=imperial&APPID=" + API_KEY;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // don't forget Loaded="Window_Loaded" in XAML
            cboQuery.Items.Add("City");
            cboQuery.Items.Add("ZIP");
            cboQuery.Items.Add("ID");

            cboQuery.SelectedIndex = 0; // select the first one (City) by default.
        }
        private void BtnForecast_Click(object sender, RoutedEventArgs e)
        {
            // Compose the query URL.
            string url = ForecastUrl.Replace("@LOC@", txtLocation.Text);
            url = url.Replace("@QUERY@", QueryCodes[cboQuery.SelectedIndex]);

            // Create a web client.
            using (WebClient client = new WebClient())
            {
                //Get the response string from the URL.
                try
                {
                    DisplayForecast(client.DownloadString(url));
                }
                catch (WebException ex)
                {
                    DisplayError(ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unknown error\n" + ex.Message);
                }
            }
        }
        // Display the forecast.
        private void DisplayForecast(string xml)
        {
            dataGrid.Items.Clear();
            // Load the response into an XML document.
            XmlDocument xml_doc = new XmlDocument();
            xml_doc.LoadXml(xml);

            // Get the city, country, latitude, and longitude.
            XmlNode loc_node = xml_doc.SelectSingleNode("weatherdata/location");
            txtCity.Text = loc_node.SelectSingleNode("name").InnerText;
            txtCountry.Text = loc_node.SelectSingleNode("country").InnerText;
            XmlNode geo_node = loc_node.SelectSingleNode("location");

            txtLat.Text = geo_node.Attributes["latitude"].Value;
            txtLong.Text = geo_node.Attributes["longitude"].Value;
            txtId.Text = geo_node.Attributes["geobaseid"].Value;

            char degrees = (char)176;

            foreach (XmlNode time_node in xml_doc.SelectNodes("//time"))
            {
                // Get the time in UTC.
                DateTime time =
                   DateTime.Parse(time_node.Attributes["from"].Value,
                        null, DateTimeStyles.AssumeUniversal);

                // Get the temperature.
                XmlNode temp_node = time_node.SelectSingleNode("temperature");
                string temp = temp_node.Attributes["value"].Value;

                WeatherInfo wi = new WeatherInfo
                {
                    DayOfWeek = time.DayOfWeek.ToString(),
                    TimeOfDay = time.ToShortTimeString(),
                    Farenheit = temp + degrees + "F",
                };
                dataGrid.Items.Add(wi);
            }
        }
        // Display an error message.
        private void DisplayError(WebException exception)
        {
            try
            {
                StreamReader reader = new StreamReader(exception.Response.GetResponseStream());
                XmlDocument response_doc = new XmlDocument();
                response_doc.LoadXml(reader.ReadToEnd());
                XmlNode message_node = response_doc.SelectSingleNode("//message");
                MessageBox.Show(message_node.InnerText);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unknown error\n" + ex.Message);
            }
        }

    }
    public class WeatherInfo
    {
        public string DayOfWeek { get; set; }
        public string TimeOfDay { get; set; }
        public string Farenheit { get; set; }
    }
}