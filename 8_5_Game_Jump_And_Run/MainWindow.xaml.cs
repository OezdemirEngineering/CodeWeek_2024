using Game;
using System.Numerics;
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

namespace _8_5_Game_Jump_And_Run
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage spriteSheet;
        private List<CroppedBitmap> walkCycle;
        private List<CroppedBitmap> idleCycle;
        private List<CroppedBitmap> jumpCycle;

        private bool pressedLeft = false;
        private bool pressedRight = false;
        private bool pressedSpace = false;

        private int currentWalkIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            GameField.Init(frameRate: 20, UpdateGame, isJumpAndRun: true, fixedPlayer: true);
            Player.InitPlayer();
            spriteSheet = new BitmapImage(new Uri("pack://application:,,,/Sprites/Player.png"));
            
            var nWidth = (int)spriteSheet.PixelWidth / 8;
            var nHeight = (int)spriteSheet.PixelHeight / 9;

            Player.Width = nWidth;
            Player.Height = nHeight;
            var imgIdle = new CroppedBitmap(spriteSheet, new Int32Rect(0, 8*nHeight, nWidth, nHeight));



            var imgWalk1 = new CroppedBitmap(spriteSheet, new Int32Rect(0*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk2 = new CroppedBitmap(spriteSheet, new Int32Rect(1*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk3 = new CroppedBitmap(spriteSheet, new Int32Rect(2*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk4 = new CroppedBitmap(spriteSheet, new Int32Rect(3*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk5 = new CroppedBitmap(spriteSheet, new Int32Rect(4*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk6 = new CroppedBitmap(spriteSheet, new Int32Rect(5*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk7 = new CroppedBitmap(spriteSheet, new Int32Rect(6*nWidth,4*nHeight,nWidth, nHeight));
            var imgWalk8 = new CroppedBitmap(spriteSheet, new Int32Rect(7*nWidth,4*nHeight,nWidth, nHeight));

            var imgJump1 = new CroppedBitmap(spriteSheet, new Int32Rect(0*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump2 = new CroppedBitmap(spriteSheet, new Int32Rect(1*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump3 = new CroppedBitmap(spriteSheet, new Int32Rect(2*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump4 = new CroppedBitmap(spriteSheet, new Int32Rect(3*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump5 = new CroppedBitmap(spriteSheet, new Int32Rect(4*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump6 = new CroppedBitmap(spriteSheet, new Int32Rect(5*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump7 = new CroppedBitmap(spriteSheet, new Int32Rect(6*nWidth,1*nHeight,nWidth, nHeight));
            var imgJump8 = new CroppedBitmap(spriteSheet, new Int32Rect(7*nWidth,1*nHeight,nWidth, nHeight));

            walkCycle =
            [
                imgWalk1, imgWalk2, imgWalk3, imgWalk4,
                imgWalk5, imgWalk6, imgWalk7, imgWalk8
            ];
            jumpCycle =
            [
                imgJump1, imgJump2, imgJump3, imgJump4,
                imgJump5, imgJump6, imgJump7, imgJump8
            ];

            idleCycle = [ imgIdle ];

            RegisterKeys();

        }

        private void RegisterKeys()
        {
            GameHelper.RegisterKeyDownAction(Key.Left, () => pressedLeft = true);
            GameHelper.RegisterKeyDownAction(Key.Right, () => pressedRight = true);
            GameHelper.RegisterKeyDownAction(Key.Space, () => pressedSpace = true);

            GameHelper.RegisterKeyUpAction(Key.Left, () => pressedLeft = false);
            GameHelper.RegisterKeyUpAction(Key.Right, () => pressedRight = false);
            GameHelper.RegisterKeyUpAction(Key.Space, () => pressedSpace = false);
        }

        private void UpdateGame()
        {
            if(pressedLeft)
            {
                Player.MoveLeft(5);
                Player.MirrorRight();
                Player.Source = walkCycle[currentWalkIndex];
            }
            else if(pressedRight)
            {
                Player.MoveRight(5);
                Player.MirrorLeft();
                Player.Source = walkCycle[currentWalkIndex];
            }
            else
            {
                Player.Stop();
                Player.Source=(idleCycle[0]);
            }

            if (pressedSpace)
            {
                Player.Jump(20, 15);
            }

            currentWalkIndex++;
            if(currentWalkIndex >= walkCycle.Count)
                currentWalkIndex = 0;

        }
    }
}