using Game;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace _1_Variablen
{
    public partial class MainWindow : Window
    {
        private ImageAnimationController? controller;
        private bool isFeeding = false;
        private int idleEndFrame = 14;
        private int frameIndex = 0;

        public double kirbySize;


        // Wird einmal durchlaufen , wenn das Fenster erstellt wird
        public MainWindow()
        {
            InitializeComponent();

            // Nur Gamenengine Spezifisch
            //*******************************************
            GameField.Init(frameRate: 100, UpdateGame);
            Loaded += OnLoaded;
            //******************************************

            kirbySize = Kirby.Height;
            SizeOutput.Text = "Size: " + kirbySize.ToString();
        }

        // Feeding here!!!!!!
        //********************************************************************************
        // Wird ausgeführt, wenn der Button geklickt wird
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            isFeeding = true;

            kirbySize = kirbySize + 10;

            // Set new size
            Kirby.Width = kirbySize;
            Kirby.Height = kirbySize;

            SizeOutput.Text = "Size: " + kirbySize.ToString();

            // Move up because of new size
            Kirby.StepUp(7);
            Kirby.StepLeft(5);

        }
        //********************************************************************************

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Lade das animierte GIF als Resource
            var gif = new BitmapImage(new Uri("pack://application:,,,/Kirby/kirby.gif", UriKind.Absolute));
            ImageBehavior.SetAnimatedSource(Kirby, gif);
            controller = ImageBehavior.GetAnimationController(Kirby);
        }

        private void UpdateGame()
        {

            // Idle-Schleife
            if (isFeeding)
            {
                isFeeding = false;
                frameIndex = 0;
            }
            else
            {
                frameIndex++;
                if (frameIndex >= idleEndFrame)
                    frameIndex = 5;
            }

            controller.GotoFrame(frameIndex);
        }
    }
}
