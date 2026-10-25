using Game;
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
using WpfAnimatedGif;

namespace _3_Bedingungen;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ImageAnimationController? controller;
    private bool isLifting = false;
    private int Level = 0;


    public bool WinnerTextVisbility
    {
        set => WinnerText.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        get => WinnerText.Visibility == Visibility.Visible;
    }


    public MainWindow()
    {
        InitializeComponent();
        GameField.Init(frameRate: 50, UpdateGame);

        Loaded += OnLoaded;
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        isLifting = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Lade das animierte GIF als Resource
        var gif = new BitmapImage(new Uri("pack://application:,,,/Images/lifter.gif", UriKind.Absolute));
        ImageBehavior.SetAnimatedSource(Lifter, gif);
        controller = ImageBehavior.GetAnimationController(Lifter);
    }

    // Schreibe eine Funktion das den wert
    // frameindex nimmt und unter score abglegt

    private void UpdateGame()
    {

        if (isLifting)
        {
            isLifting = false;
            Level=Level + 10;
        }

        // Win at Level 140 , WinnerTextVisibility makes the text visible if is set to true 





        if (Level < 0)
            Level = 0;

        if (Level >= controller.FrameCount)
            Level = controller.FrameCount - 1;


        controller.GotoFrame(Level);
        Level--;
    }

}