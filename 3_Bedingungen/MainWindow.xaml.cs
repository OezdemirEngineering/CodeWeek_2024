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
    private int frameIndex = 0;

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

    private void UpdateGame()
    {

        if (isLifting)
        {
            isLifting = false;
            frameIndex+=4;
        }

        // Win at frameIndex 140


        if (frameIndex < 0)
            frameIndex = 0;

        if (frameIndex >= controller.FrameCount)
            frameIndex = controller.FrameCount - 1;


        controller.GotoFrame(frameIndex);
        frameIndex--;
    }

    private void EnableWinnerText()
    {
        WinnerText.Visibility = Visibility.Visible;
    }
}