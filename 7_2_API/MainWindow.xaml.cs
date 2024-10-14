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

namespace _7_2_API;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        GetRandomUserAsync();
    }

    private static readonly HttpClient client = new HttpClient();

    public static async Task GetRandomUserAsync()
    {
        // Page : https://randomuser.me/

        string url = "https://randomuser.me/api/";
        var response = await client.GetStringAsync(url);

        var userData = JsonSerializer.Deserialize<RandomUserResponse>(response);
        var user = userData.results[0];

    }

    public class RandomUserResponse
    {
        public User[] results { get; set; }
    }

    public class User
    {
        public Name name { get; set; }
    }

    public class Name
    {
    }
}

