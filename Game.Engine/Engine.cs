using Game.Engine.Dtos;
using Game.Engine.Enums;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using WpfAnimatedGif;

namespace Game;

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

    // Tags that scroll (move as the world) in fixed-player mode
    private static readonly HashSet<string> _scrollingTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "wall" };

    public static void RegisterLayerTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        _scrollingTags.Add(tag.Trim().ToLower());
    }

    public static void RegisterLayerTags(params string[] tags)
    {
        if (tags == null) return;
        foreach (var t in tags) RegisterLayerTag(t);
    }

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

    public static bool StepDown(this UIElement obj, double steps) => TryStep(obj, 0, +steps);

    public static bool StepUp(this UIElement obj, double steps) => TryStep(obj, 0, -steps);

    public static bool StepRight(this UIElement obj, double steps) => TryStep(obj, +steps, 0);

    public static bool StepLeft(this UIElement obj, double steps) => TryStep(obj, -steps, 0);

    private static bool TryStep(UIElement obj, double dx, double dy)
    {
        var wallHitBoxes = GetWallHitBoxes();

        if (_fixedPlayer)
        {
            // Offset wall hitboxes by the world's intended movement and test against the player
            for (int i = 0; i < wallHitBoxes.Count; i++)
            {
                var hb = wallHitBoxes[i];
                wallHitBoxes[i] = new Rect(hb.X + dx, hb.Y + dy, hb.Width, hb.Height);
            }

            if (wallHitBoxes.Any(hb => hb.IntersectsWith(mainPlayer.GetHitBox())))
            {
                return true; // collision
            }

            // Move the world (registered scrolling tags) if no collision
            MoveWorld(dx, dy);
            return false;
        }

        // Non-fixed player: try new position and test collision with walls
        Point currentPosition = GetPosition(obj);
        Point newPosition = new Point(currentPosition.X + dx, currentPosition.Y + dy);
        Rect rect = new Rect(newPosition.X, newPosition.Y, obj.GetHitBox().Width, obj.GetHitBox().Height);

        if (wallHitBoxes.Any(wallHitBox => wallHitBox.IntersectsWith(rect)))
        {
            return true;
        }

        SetPosition(obj, newPosition);
        return false;
    }

    private static void MoveWorld(double dx, double dy)
    {
        foreach (var item in _moveStates.Where(it =>
        {
            var t = it.Object.GetTag();
            return t != null && _scrollingTags.Contains(t);
        }))
        {
            var pos = item.Object.GetPosition();
            SetPosition(item.Object, new Point(pos.X + dx, pos.Y + dy));
        }
    }

    private static List<Rect> GetWallHitBoxes()
    {
        return _gameField.Children
            .OfType<UIElement>()
            .Where(uie => uie.GetTag() == "wall")
            .Select(uie => uie.GetHitBox())
            .ToList();
    }

    public static void MoveDown(this UIElement obj, double speed)
    {
        if (_fixedPlayer)
        {
            foreach (var tag in _scrollingTags) MoveTagsToTop(tag, speed);
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
            foreach (var tag in _scrollingTags) MoveTagsToBottom(tag, speed);
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
            foreach (var tag in _scrollingTags) MoveTagsToRight(tag, speed);
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
            foreach (var tag in _scrollingTags) MoveTagsToLeft(tag, speed);
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
            // Stop ALL scrolling layers (e.g., "wall", "layer0", ...)
            _moveStates
                .Where(p =>
                {
                    var tag = p.Object.GetTag();
                    return tag != null && _scrollingTags.Contains(tag);
                })
                .ToList()
                .ForEach(move =>
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

        MoveDto playerMove = _moveStates.First(p => p.Object == obj);
        playerMove.Speed = 0;
        playerMove.IsMovingLeft = false;
        playerMove.IsMovingRight = false;
        playerMove.IsMovingUp = false;
        playerMove.IsMovingDown = false;
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
        UpdateGame?.Invoke();
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
        else if (obj is System.Windows.Shapes.Path path)
        {
            // Use Width/Height for Path, assuming it's set and matches the visual bounds
            hitBox = new Rect(xy.X, xy.Y, path.Width, path.Height);
        }
        else if (obj is Canvas canvas)
        {
            hitBox = new Rect(xy.X, xy.Y, canvas.ActualWidth, canvas.ActualHeight);
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
        catch (Exception)
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

    public static List<BitmapSource> MirrorFrames(List<BitmapSource> frames)
    {
        var result = new List<BitmapSource>(frames.Count);
        foreach (var f in frames)
            result.Add(MirrorHorizontal(f));
        return result;
    }

    public static BitmapSource MirrorHorizontal(BitmapSource src)
    {
        var tb = new TransformedBitmap();
        tb.BeginInit();
        tb.Source = src;
        // Flip around the center to keep dimensions aligned
        tb.Transform = new ScaleTransform(-1, 1, src.PixelWidth / 2.0, src.PixelHeight / 2.0);
        tb.EndInit();
        tb.Freeze();
        return tb;
    }

    public static Action UpdateGame;
}