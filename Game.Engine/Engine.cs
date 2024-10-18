using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Input;
using Game.Engine.Dtos;
using Game.Engine.Enums;

namespace Game
{
    public static class GameHelper
    {
        public static double FallStepSize { get; set; } = 10;
        private static DispatcherTimer _frameRate;

        private static UIElement mainPlayer;

        private static Random _random;

        private static Canvas _gameField;

        private static List<JumpStateDto> _jumpStates;
        private static List<MoveDto> _moveStates;
        private static bool _isJumpAndRun;

        private static bool _fixedPlayer;

        private static Dictionary<Key, Action> _keyDownActions = new Dictionary<Key, Action>();
        private static Dictionary<Key, Action> _keyUpActions = new Dictionary<Key, Action>();

        private static List<Projectile> _projectiles = new List<Projectile>();


        // Method to register key actions
        public static void RegisterKeyDownAction(Key key, Action action)
        {
            if (!_keyDownActions.ContainsKey(key))
            {
                _keyDownActions[key] = action;
            }
        }

        public static void RegisterKeyUpAction(Key key, Action action)
        {
            if (!_keyUpActions.ContainsKey(key))
            {
                _keyUpActions[key] = action;
            }
        }

        // Method to handle key down events
        public static void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (_keyDownActions.ContainsKey(e.Key))
            {
                _keyDownActions[e.Key].Invoke();
            }
        }

        // Method to handle key up events
        public static void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (_keyUpActions.ContainsKey(e.Key))
            {
                _keyUpActions[e.Key].Invoke();
            }
        }

        public static void Init(this Canvas gameField, int frameRate = 1, Action updateGame = null, bool isJumpAndRun = false, bool fixedPlayer = false)
        {
            InitializeFields(gameField, frameRate, isJumpAndRun);
            InitializeGameStates();
            InitializeFrameRate(updateGame);

            gameField.KeyDown += GameHelper.HandleKeyDown;
            gameField.KeyUp += GameHelper.HandleKeyUp;

            _fixedPlayer = fixedPlayer;
        }

        public static void InitPlayer(this UIElement obj)
        {
            Jump(obj, 0, 0);
            mainPlayer = obj;
        }

        private static void InitializeFields(Canvas gameField, int frameRate, bool isJumpAndRun)
        {
            _random = new Random();
            _gameField = gameField;
            _gameField.Focusable = true;
            _isJumpAndRun = isJumpAndRun;
            _gameField.Focus();
            _frameRate = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, frameRate)
            };
        }

        private static void InitializeGameStates()
        {
            _jumpStates = new List<JumpStateDto>();
            _moveStates = new List<MoveDto>();
        }

        private static void InitializeFrameRate(Action updateGame)
        {
            _frameRate.Tick += _frameRate_Tick;
            UpdateGame = updateGame;
            _frameRate.Start();
        }


        public static void StartUpdateGame()
        {
            _frameRate.Start();
        }

        public static void StopUpdateGame()
        {
            _frameRate.Stop();
        }

        public static Point GetPosition(this UIElement obj)
        {
            return new Point(Canvas.GetLeft(obj), Canvas.GetTop(obj));
        }


        //write a static method that sets only the y position 
        public static void SetXPostion(this UIElement obj, int x)
        {
            Canvas.SetLeft(obj, x);
        }
        public static void SetYPostion(this UIElement obj, int y)
        {
            Canvas.SetTop(obj, y);
        }

        public static void SetPosition(this UIElement obj, double x, double y)
        {
            SetPosition(obj, new Point(x, y));
        }

        public static void SetPosition(this UIElement obj, Point xy)
        {
            Canvas.SetLeft(obj, xy.X);
            Canvas.SetTop(obj, xy.Y);
        }

        public static bool StepDown(this UIElement obj, double steps)
        {

            var wallHitBoxes = _gameField.Children.OfType<UIElement>().Where(uie => uie.GetTag() == "wall").Select(uie => uie.GetHitBox()).ToList();

            if (_fixedPlayer)
            {
                for (int i = 0; i < wallHitBoxes.Count; i++)
                {
                    wallHitBoxes[i] = new Rect(wallHitBoxes[i].X, wallHitBoxes[i].Y + steps, wallHitBoxes[i].Width, wallHitBoxes[i].Height);
                }
                var collisions = wallHitBoxes.Where(wallHitBox => wallHitBox.IntersectsWith(mainPlayer.GetHitBox())).ToList();
                if (collisions.Count > 0)
                {
                    return true;
                }


                foreach (var item in _moveStates.Where(it => it.Object.GetTag() == "wall"))
                {
                    var pos = item.Object.GetPosition();

                    Point newPos = new Point(pos.X, pos.Y + steps);
                    SetPosition(item.Object, newPos);
                }
                return false;
            }

            Point currentPosition = GetPosition(obj);
            Point newPosition = new Point(currentPosition.X, currentPosition.Y + steps);
            Rect rect = new Rect(newPosition.X, newPosition.Y, obj.GetHitBox().Width, obj.GetHitBox().Height);

            if (wallHitBoxes.Any(wallHitBox => wallHitBox.IntersectsWith(rect)))
            {
                return true;
            }

            SetPosition(obj, newPosition);

            return false;
        }

        public static bool StepUp(this UIElement obj, double steps)
        {
            var wallHitBoxes = _gameField.Children.OfType<UIElement>().Where(uie => uie.GetTag() == "wall").Select(uie => uie.GetHitBox()).ToList();

            if (_fixedPlayer)
            {
                for (int i = 0; i < wallHitBoxes.Count; i++)
                {
                    wallHitBoxes[i] = new Rect(wallHitBoxes[i].X, wallHitBoxes[i].Y - steps, wallHitBoxes[i].Width, wallHitBoxes[i].Height);
                }
                var collisions = wallHitBoxes.Where(wallHitBox => wallHitBox.IntersectsWith(mainPlayer.GetHitBox())).ToList();
                if (collisions.Count > 0)
                {
                    return true;
                }

                foreach (var item in _moveStates.Where(it => it.Object.GetTag() == "wall"))
                {
                    var pos = item.Object.GetPosition();

                    Point newPos = new Point(pos.X, pos.Y - steps);
                    SetPosition(item.Object, newPos);
                }
                return false;
            }

            Point currentPosition = GetPosition(obj);
            Point newPosition = new Point(currentPosition.X, currentPosition.Y - steps);
            Rect rect = new Rect(newPosition.X, newPosition.Y, obj.GetHitBox().Width, obj.GetHitBox().Height);

            if (wallHitBoxes.Any(wallHitBox => wallHitBox.IntersectsWith(rect)))
            {
                return true;
            }
            SetPosition(obj, newPosition);

            return false;
        }

        public static bool StepRight(this UIElement obj, double steps)
        {

            var wallHitBoxes = _gameField.Children.OfType<UIElement>().Where(uie => uie.GetTag() == "wall").Select(uie => uie.GetHitBox()).ToList();

            if (_fixedPlayer)
            {
                for (int i = 0; i < wallHitBoxes.Count; i++)
                {
                    wallHitBoxes[i] = new Rect(wallHitBoxes[i].X + steps, wallHitBoxes[i].Y, wallHitBoxes[i].Width, wallHitBoxes[i].Height);
                }
                var collisions = wallHitBoxes.Where(wallHitBox => wallHitBox.IntersectsWith(mainPlayer.GetHitBox())).ToList();
                if (collisions.Count > 0)
                {
                    return true;
                }

                foreach (var item in _moveStates.Where(it => it.Object.GetTag() == "wall"))
                {
                    var pos = item.Object.GetPosition();

                    Point newPos = new Point(pos.X + steps, pos.Y);
                    SetPosition(item.Object, newPos);
                }
                return false;
            }

            Point currentPosition = GetPosition(obj);
            Point newPosition = new Point(currentPosition.X +steps, currentPosition.Y);
            Rect rect = new Rect(newPosition.X + steps, newPosition.Y, obj.GetHitBox().Width, obj.GetHitBox().Height);

            if (wallHitBoxes.Any(wallHitBox => wallHitBox.IntersectsWith(rect)))
            {
                return true;
            }
            SetPosition(obj, newPosition);

            return false;
        }

        public static bool StepLeft(this UIElement obj, double steps)
        {
            var wallHitBoxes = _gameField.Children.OfType<UIElement>().Where(uie => uie.GetTag() == "wall").Select(uie => uie.GetHitBox()).ToList();
            if (_fixedPlayer)
            {
                for (int i = 0; i < wallHitBoxes.Count; i++)
                {
                    wallHitBoxes[i] = new Rect(wallHitBoxes[i].X - steps, wallHitBoxes[i].Y, wallHitBoxes[i].Width, wallHitBoxes[i].Height);
                }
                var collisions = wallHitBoxes.Where(wallHitBox => wallHitBox.IntersectsWith(mainPlayer.GetHitBox())).ToList();
                if (collisions.Count > 0)
                {
                    return true;
                }


                foreach (var item in _moveStates.Where(it => it.Object.GetTag() == "wall"))
                {
                    var pos = item.Object.GetPosition();

                    Point newPos = new Point(pos.X - steps, pos.Y);
                    SetPosition(item.Object, newPos);
                }
                return false;
            }

            Point currentPosition = GetPosition(obj);
            Point newPosition = new Point(currentPosition.X - steps, currentPosition.Y);
            Rect rect = new Rect(newPosition.X - steps, newPosition.Y, obj.GetHitBox().Width, obj.GetHitBox().Height);

            if (wallHitBoxes.Any(wallHitBox => wallHitBox.IntersectsWith(rect)))
            {
                return true;
            }
            SetPosition(obj, newPosition);

            return false;

        }


        public static void MoveDown(this UIElement obj, double speed)
        {
            if (_fixedPlayer)
            {
                MoveTagsToTop("wall", speed);
            }
            else
            {
                StartMoving(obj, speed, Direction.Down);
            }
        }

        public static void MoveUp(this UIElement obj, double speed)
        {
            if (_fixedPlayer)
            {
                MoveTagsToBottom("wall", speed);
            }
            else
            {
                StartMoving(obj, speed, Direction.Up);
            }
        }

        public static void MoveLeft(this UIElement obj, double speed)
        {
            if (_fixedPlayer)
            {
                MoveTagsToRight("wall", speed);
            }
            else
            {
                StartMoving(obj, speed, Direction.Left);
            }
        }

        public static void MoveRight(this UIElement obj, double speed)
        {
            if (_fixedPlayer)
            {
                MoveTagsToLeft("wall", speed);
            }
            else
            {
                StartMoving(obj, speed, Direction.Right);
            }
        }

        public static void StartMoving(this UIElement obj, double speed, Direction direction)
        {
            if (!_moveStates.Any(p => p.Object == obj))
            {
                _moveStates.Add(new MoveDto(obj, speed));
            }

            MoveDto move = _moveStates.First(p => p.Object == obj);
            move.Speed = speed;
            switch (direction)
            {
                case Direction.Left:
                    move.IsMovingLeft = true;
                    break;
                case Direction.Right:
                    move.IsMovingRight = true;
                    break;
                case Direction.Up:
                    move.IsMovingUp = true;
                    break;
                case Direction.Down:
                    move.IsMovingDown = true;
                    break;
            }
        }

        public static void Stop(this UIElement obj)
        {
            if (_fixedPlayer)
            {
                _moveStates.Where(p => p.Object.GetTag() == "wall").ToList().ForEach(move =>
                {
                    move.Speed = 0;
                    move.IsMovingLeft = false;
                    move.IsMovingRight = false;
                    move.IsMovingUp = false;
                    move.IsMovingDown = false;
                });
            }


            if (!_moveStates.Exists(p => p.Object == obj))
            {
                _moveStates.Add(new MoveDto(obj, 0));
            }

            MoveDto move = _moveStates.First(p => p.Object == obj);
            move.Speed = 0;
            move.IsMovingLeft = false;
            move.IsMovingRight = false;
            move.IsMovingUp = false;
            move.IsMovingDown = false;
        }


        public static int GetRandomNumber(int min, int max)
            => _random.Next(min, max);

        public static void Jump(this UIElement player, double speed, int maximalJumpCount)
        {
            if (!_isJumpAndRun)
                return;

            if (!_jumpStates.Exists(p => p.Player == player))
            {
                _jumpStates.Add(new JumpStateDto(player, speed, maximalJumpCount));
            }
            else
            {
                JumpStateDto jumper = _jumpStates.First(p => p.Player == player);

                if (!jumper.IsJumping() && !jumper.IsFalling())
                {
                    _jumpStates.First(p => p.Player == player).Jump(speed, maximalJumpCount);
                }
            }
        }

        private static void CheckJumps()
        {
            foreach (var jumper in _jumpStates)
            {
                if (!jumper.IsJumping() && !jumper.IsFalling() && !IsTagColliding(jumper.Player, "wall"))
                {
                    jumper.SetFall();
                }

                // Todo - Fix it for fixed player 
                if (jumper.IsFalling())
                {
                    if (StepDown(jumper.Player, FallStepSize))
                        jumper.MakeReady();
                    continue;
                }

                if (jumper.IsJumping())
                {
                    StepUp(jumper.Player, jumper.Speed);
                }
            }
        }

        private static void CheckMoves()
        {
            foreach (var move in _moveStates)
            {
                if (move.IsMovingDown)
                {
                    StepDown(move.Object, move.Speed);
                }
                if (move.IsMovingUp)
                {
                    StepUp(move.Object, move.Speed);
                }
                if (move.IsMovingLeft)
                {
                    StepLeft(move.Object, move.Speed);
                }
                if (move.IsMovingRight)
                {
                    StepRight(move.Object, move.Speed);
                }
            }
        }

        private static void _frameRate_Tick(object sender, EventArgs e)
        {
            CheckMoves();
            if (_isJumpAndRun)
                CheckJumps();

            CheckProjectiles();  // Bewege die Geschosse in jedem Frame
            UpdateGame.Invoke();
        }

        public static void Rotate(this UIElement obj, double angle)
        {
            if (obj is Rectangle rectangle)
                obj.RenderTransform = new RotateTransform(angle, rectangle.Width / 2, rectangle.Height / 2);
            else if (obj is Ellipse ellipse)
                obj.RenderTransform = new RotateTransform(angle, ellipse.Width / 2, ellipse.Height / 2);
            else if (obj is Image image)
                obj.RenderTransform = new RotateTransform(angle, image.Width / 2, image.Height / 2);
        }


        public static void Orbit(this UIElement obj, UIElement target, double radius, double angle)
        {
            if (angle > 360)
            {
                angle = 0;
            }
            double centerX = Canvas.GetLeft(target) + target.RenderSize.Width / 2;
            double centerY = Canvas.GetTop(target) + target.RenderSize.Height / 2;
            double x = centerX + radius * Math.Cos(angle);
            double y = centerY + radius * Math.Sin(angle);
            SetPosition(obj, new Point(x, y));
        }

        public static void MirrorLeft(this UIElement obj)
        {
            obj.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleX = 1;
            obj.RenderTransform = flipTrans;
        }
        public static void MirrorRight(this UIElement obj)
        {
            obj.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform flipTrans = new ScaleTransform();
            flipTrans.ScaleX = -1;
            obj.RenderTransform = flipTrans;
        }


        public static Rect GetHitBox(this UIElement obj)
        {
            Point xy = GetPosition(obj);
            Rect hitBox;

            if (obj is Rectangle rectangle)
            {
                hitBox = new Rect(xy.X, xy.Y, rectangle.Width, rectangle.Height);
            }
            else if (obj is Ellipse ellipse)
            {
                hitBox = new Rect(xy.X, xy.Y, ellipse.Width, ellipse.Height);
            }
            else
            {
                hitBox = new Rect(xy.X, xy.Y, ((Image)obj).Width, ((Image)obj).Height);
            }

            return hitBox;
        }

        public static bool IsColliding(this UIElement obj1, UIElement obj2)
            => GetHitBox(obj1).IntersectsWith(GetHitBox(obj2));

        public static string? GetTag(this UIElement obj)
        {
            try
            {
                if (obj is Rectangle rect && ((Rectangle)obj).Tag != null)
                {
                    return rect.Tag.ToString()!.Trim().ToLower();
                }
                else if (obj is Ellipse ell && ((Ellipse)obj).Tag != null)
                {
                    return ell.Tag.ToString()!.Trim().ToLower();
                }
                else if (obj is Image img && ((Image)obj).Tag != null)
                {
                    return img.Tag.ToString()!.Trim().ToLower();
                }
            }
            catch (Exception ex)
            {
                // No Tag Exception 
            }
            return null;
        }

        public static bool IsCollidingToWall(this UIElement obj)
        {
            return IsTagColliding(obj, "object", "wall");
        }

        public static bool IsCollidingToItem(this UIElement obj)
        {
            return IsTagColliding(obj, "object", "item");
        }

        public static bool IsTagColliding(this UIElement obj, string tag1, string tag2)
        {
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(obj) == tag1 && GetTag(child) == tag2 && IsColliding(obj, child))
                {
                    return true;
                }
                else if (GetTag(obj) == tag2 && GetTag(child) == tag1 && IsColliding(obj, child))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsTagColliding(this UIElement obj, string tag)
        {
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag && IsColliding(obj, child))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsTagCollidingWithRemove(this UIElement obj, string tag)
        {
            UIElement chObj = null;
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag && IsColliding(obj, child))
                {
                    chObj = child;
                    _gameField.Children.Remove(chObj);
                    return true;
                }
            }
            return false;
        }

        public static Image InsertElementTag(this UIElement sourceObj, Image obj, string tag)
        {
            Point xy = GetPosition(sourceObj);


            string objStr = XamlWriter.Save(obj);
            StringReader stringReader = new StringReader(objStr);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            obj = (Image)XamlReader.Load(xmlReader);
            obj.Tag = tag;
            SetPosition(obj, xy);
            _gameField.Children.Insert(0, obj);
            return obj;
        }

        public static void Shoot(this UIElement sourceObj, Image obj, string tag, double speed, Direction direction)
        {
            var insertedObj = InsertElementTag(sourceObj, obj, tag);
            var projectile = new Projectile(insertedObj, speed, direction);
            _projectiles.Add(projectile);  // Füge das Projektil der Liste hinzu
        }

        private static void CheckProjectiles()
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _projectiles[i];
                switch (projectile.Direction)
                {
                    case Direction.Left:
                        StepLeft(projectile.Object, projectile.Speed);
                        break;
                    case Direction.Right:
                        StepRight(projectile.Object, projectile.Speed);
                        break;
                    case Direction.Up:
                        StepUp(projectile.Object, projectile.Speed);
                        break;
                    case Direction.Down:
                        StepDown(projectile.Object, projectile.Speed);
                        break;
                }

                // Entferne das Projektil, wenn es aus dem Spielfeld heraus ist
                if (IsOutOfBounds(projectile.Object))
                {
                    _gameField.Children.Remove(projectile.Object);
                    _projectiles.RemoveAt(i);
                }
            }
        }

        private static bool IsOutOfBounds(UIElement obj)
        {
            var pos = GetPosition(obj);
            return pos.X < 0 || pos.Y < 0 || pos.X > _gameField.ActualWidth || pos.Y > _gameField.ActualHeight;
        }


        public static void MoveTagsToLeft(string tag, double step)
        {
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag)
                {
                    StartMoving(child, step, Direction.Left);
                }
            }
        }

        public static void MoveTagsToRight(string tag, double step)
        {
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag)
                {
                    StartMoving(child, step, Direction.Right);
                }
            }
        }

        public static void MoveTagsToTop(string tag, double step)
        {
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag)
                {
                    StartMoving(child, step, Direction.Up);
                }
            }
        }

        public static void MoveTagsToBottom(string tag, double step)
        {
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag)
                {
                    StartMoving(child, step, Direction.Down);
                }
            }
        }


        public static void RemoveIfTagHitTo(this UIElement obj, string tag)
        {
            UIElement chObj = null;
            foreach (UIElement child in _gameField.Children)
            {
                if (GetTag(child) == tag && IsColliding(obj, child))
                {
                    chObj = child;
                }
            }

            if (chObj != null)
                _gameField.Children.Remove(chObj);
        }


        public static void StartAnimation(this Image obj)
        {
            ImageBehavior.SetRepeatBehavior(obj, RepeatBehavior.Forever);
        }

        public static void StopAnimation(this Image obj)
        {
            ImageBehavior.SetRepeatBehavior(obj, new RepeatBehavior(1));
        }

        public static void PlaySound(string audioPath, int volume)
        {
            Task.Run(() =>
            {
                string pfadSound = Directory.GetCurrentDirectory() + audioPath;
                MediaPlayer myPlayer = new MediaPlayer();
                myPlayer.Open(new System.Uri(pfadSound));
                myPlayer.Volume = volume;
                myPlayer.Play();
            });

        }

        public static Action UpdateGame;
    }

}