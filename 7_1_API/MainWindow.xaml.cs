using RestSharp;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _7_1_API;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        GetWeatherAsync();
    }



    private static readonly HttpClient client = new HttpClient();

    public static async Task GetWeatherAsync()
    {
        // Hole das Wetter für Aachen von wttr.in
        string weatherUrl = "https://wttr.in/Schramberg?format=%C+%t"; // %C gibt den Wetterzustand, %t die Temperatur
        string weatherResponse = await client.GetStringAsync(weatherUrl);
    }

}