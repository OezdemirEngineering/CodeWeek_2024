using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _4_Schleifen;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    List<string> IDs = [];
    List<string> FirstNames = [];
    List<string> LastNames = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        int index = -1;
        for(int i =0; i<IDs.Count; i++)
        {
            string id = IDs[i];

            if( id ==ID.Text)
            {
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            Vorname.Text = FirstNames[index];
            Nachname.Text = LastNames[index];
        }

    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        IDs.Add(ID.Text);
        FirstNames.Add(Vorname.Text);
        LastNames.Add(Nachname.Text);
    }
}