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

namespace FirstSimpleGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isMovingRight = true;
        int speed = 5;
        public MainWindow()
        {
            InitializeComponent();
            GameField.Init(frameRate: 10, UpdateGame);

            RegisterKeys();
        }

        private void RegisterKeys()
        {
            GameHelper.RegisterKeyDownAction(Key.Up, () => WallRight.MoveUp(10));
            GameHelper.RegisterKeyDownAction(Key.Down, () => WallRight.MoveDown(10));
            GameHelper.RegisterKeyUpAction(Key.Up, () => WallRight.Stop());
            GameHelper.RegisterKeyUpAction(Key.Down, () => WallRight.Stop());
        }

        private void UpdateGame()
        {

            if(Ball.IsColliding(WallRight))
            {
                isMovingRight=false;
                speed++;
            }

            if(Ball.IsColliding(WallLeft))
            {
                isMovingRight = true;
                speed++;
            }

            if(isMovingRight)
            {
                Ball.StepRight(speed);
            }
            else
            {
                Ball.StepLeft(speed);
            }

        }
    }
}