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

namespace _8_3_Game;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Movement state flags
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isMovingUp = false;
    private bool isMovingDown = false;

    private BitmapImage spriteSheet;

    // Sprite animation state
    private enum FaceDir { Down, Up, Left, Right }
    private FaceDir currentDirection = FaceDir.Down;
    private int currentFrameIndex = 0; // per-direction frame index
    private int frameTick = 0;
    private const int ticksPerFrame = 4; // lower = faster animation

    // Sprite frames per direction
    private List<BitmapSource> frontFrames = [];
    private List<BitmapSource> backFrames = [];
    private List<BitmapSource> leftFrames = [];
    private List<BitmapSource> rightFrames = [];

    // Sprite sheet frame size
    private const int spriteWidth = 224;  // width of one frame
    private const int spriteHeight = 284; // height of one frame
    private const double speed = 1.2;     // movement speed

    public MainWindow()
    {
        InitializeComponent();

        // Register non-colliding scrolling layers
        GameHelper.RegisterLayerTag("layer0");

        // Load sprite sheet
        spriteSheet = new BitmapImage(new Uri("pack://application:,,,/Sprites/PlayerRedSpriteMap.png"));

        BuildFrames();

        // Initial idle frame (facing down)
        currentDirection = FaceDir.Down;
        currentFrameIndex = 0;
        Player.Source = frontFrames[currentFrameIndex];

        // Start game loop
        GameField.Init(frameRate: 20, UpdateGame, isJumpAndRun: false, fixedPlayer: true);
        Player.InitPlayer();

        // Register controls
        RegisterKeys();
    }

    private void BuildFrames()
    {
        // Build frames from sprite sheet (3 columns x 4 rows)
        var imgFront1 = new CroppedBitmap(spriteSheet, new Int32Rect(0, 0, spriteWidth, spriteHeight));
        var imgFront2 = new CroppedBitmap(spriteSheet, new Int32Rect(spriteWidth, 0, spriteWidth, spriteHeight));
        var imgFront3 = new CroppedBitmap(spriteSheet, new Int32Rect(2 * spriteWidth, 0, spriteWidth, spriteHeight));

        // Note: custom offsets preserved as in your code
        var imgBack1 = new CroppedBitmap(spriteSheet, new Int32Rect(0, spriteHeight + 10, spriteWidth, spriteHeight + 10));
        var imgBack2 = new CroppedBitmap(spriteSheet, new Int32Rect(spriteWidth - 14, spriteHeight + 10, spriteWidth - 14, spriteHeight + 10));
        var imgBack3 = new CroppedBitmap(spriteSheet, new Int32Rect(2 * spriteWidth - 14, spriteHeight + 10, spriteWidth - 14, spriteHeight + 10));

        var imgLeft1 = new CroppedBitmap(spriteSheet, new Int32Rect(0, 2 * spriteHeight + 20, spriteWidth, spriteHeight + 10));
        var imgLeft2 = new CroppedBitmap(spriteSheet, new Int32Rect(spriteWidth - 14, 2 * spriteHeight + 20, spriteWidth - 14, spriteHeight + 10));
        var imgLeft3 = new CroppedBitmap(spriteSheet, new Int32Rect(2 * spriteWidth - 14, 2 * spriteHeight + 20, spriteWidth - 14, spriteHeight + 10));

        // Use 4-frame ping-pong for all directions for consistent modulo
        frontFrames = [imgFront1, imgFront2, imgFront3, imgFront2];
        backFrames  = [imgBack1,  imgBack2,  imgBack3,  imgBack2 ];
        leftFrames  = [imgLeft1,  imgLeft2,  imgLeft3,  imgLeft2 ];

        // Mirror LEFT frames to build RIGHT frames
        rightFrames = GameHelper.MirrorFrames(leftFrames);
    }

    private void RegisterKeys()
    {
        GameHelper.RegisterKeyDownAction(Key.Left,  () => StartMove(FaceDir.Left));
        GameHelper.RegisterKeyDownAction(Key.Right, () => StartMove(FaceDir.Right));
        GameHelper.RegisterKeyDownAction(Key.Up,    () => StartMove(FaceDir.Up));
        GameHelper.RegisterKeyDownAction(Key.Down,  () => StartMove(FaceDir.Down));

        GameHelper.RegisterKeyUpAction(Key.Left,  () => StopMove(FaceDir.Left));
        GameHelper.RegisterKeyUpAction(Key.Right, () => StopMove(FaceDir.Right));
        GameHelper.RegisterKeyUpAction(Key.Up,    () => StopMove(FaceDir.Up));
        GameHelper.RegisterKeyUpAction(Key.Down,  () => StopMove(FaceDir.Down));
    }

    private void StartMove(FaceDir dir)
    {
        SetDirFlag(dir, true);
        currentDirection = dir;

        switch (dir)
        {
            case FaceDir.Left:  Player.MoveLeft(speed);  break;
            case FaceDir.Right: Player.MoveRight(speed); break;
            case FaceDir.Up:    Player.MoveUp(speed);    break;
            case FaceDir.Down:  Player.MoveDown(speed);  break;
        }
    }

    private void StopMove(FaceDir dir)
    {
        SetDirFlag(dir, false);

        // If any other direction is still active, keep moving in that direction
        if (isMovingLeft || isMovingRight || isMovingUp || isMovingDown)
        {
            // Prefer to continue in the currentDirection if still pressed, else pick any pressed
            if (!IsDirPressed(currentDirection))
            {
                if (isMovingLeft)  currentDirection = FaceDir.Left;
                else if (isMovingRight) currentDirection = FaceDir.Right;
                else if (isMovingUp)    currentDirection = FaceDir.Up;
                else if (isMovingDown)  currentDirection = FaceDir.Down;
            }

            // Re-issue movement in the active direction (Stop resets speeds in the engine)
            switch (currentDirection)
            {
                case FaceDir.Left:  Player.MoveLeft(speed);  break;
                case FaceDir.Right: Player.MoveRight(speed); break;
                case FaceDir.Up:    Player.MoveUp(speed);    break;
                case FaceDir.Down:  Player.MoveDown(speed);  break;
            }
        }
        else
        {
            // No direction pressed: stop world/player
            Player.Stop();
        }
    }

    private void SetDirFlag(FaceDir dir, bool pressed)
    {
        switch (dir)
        {
            case FaceDir.Left:  isMovingLeft  = pressed; break;
            case FaceDir.Right: isMovingRight = pressed; break;
            case FaceDir.Up:    isMovingUp    = pressed; break;
            case FaceDir.Down:  isMovingDown  = pressed; break;
        }
    }

    private bool IsDirPressed(FaceDir dir) =>
        dir switch
        {
            FaceDir.Left  => isMovingLeft,
            FaceDir.Right => isMovingRight,
            FaceDir.Up    => isMovingUp,
            FaceDir.Down  => isMovingDown,
            _ => false
        };

    private void UpdateGame()
    {
        bool isMoving = isMovingLeft || isMovingRight || isMovingUp || isMovingDown;

        // Select current direction frames
        List<BitmapSource> frames = currentDirection switch
        {
            FaceDir.Left  => leftFrames,
            FaceDir.Right => rightFrames,
            FaceDir.Up    => backFrames,
            _             => frontFrames
        };

        if (isMoving)
        {
            frameTick++;
            if (frameTick >= ticksPerFrame)
            {
                frameTick = 0;
                currentFrameIndex = (currentFrameIndex + 1) % frames.Count;
            }
        }
        else
        {
            // Idle frame per direction (keep your original choices)
            currentFrameIndex = 1;
            frameTick = 0;
        }

        // Apply sprite once
        Player.Source = frames[currentFrameIndex];
    }
}

// GameHelper

// Key Input Management
// ----------------------------------------------------------------------------

// RegisterKeyDownAction(Key key, Action action)
// Registriert eine Aktion, die bei einem Key-Down-Ereignis für eine bestimmte Taste ausgeführt wird.

// RegisterKeyUpAction(Key key, Action action)
// Registriert eine Aktion, die bei einem Key-Up-Ereignis für eine bestimmte Taste ausgeführt wird.

// HandleKeyDown(object sender, KeyEventArgs e)
// Behandelt Key-Down-Ereignisse und führt die registrierte Aktion für die gedrückte Taste aus.

// HandleKeyUp(object sender, KeyEventArgs e)
// Behandelt Key-Up-Ereignisse und führt die registrierte Aktion für die freigegebene Taste aus.

// Game Initialization and Management
// ----------------------------------------------------------------------------

// Init(Canvas gameField, int frameRate = 1, Action updateGame = null, bool isJumpAndRun = false)
// Initialisiert das Spielfeld, setzt die Frame-Rate und aktiviert optional den Jump-and-Run-Modus.

// InitPlayer(UIElement obj)
// Initialisiert den Spieler, indem die Sprunglogik mit Standardwerten gestartet wird.

// StartUpdateGame()
// Startet das regelmäßige Aktualisieren des Spiels basierend auf der Frame-Rate.

// StopUpdateGame()
// Stoppt das regelmäßige Aktualisieren des Spiels.

// UI Element Position and Movement
// ----------------------------------------------------------------------------

// GetPosition(UIElement obj)
// Gibt die aktuelle Position eines UI-Elements (wie des Spielers) auf dem Canvas zurück.

// SetPosition(UIElement obj, Point xy)
// Setzt die Position eines UI-Elements auf dem Canvas.

// StepDown(UIElement obj, double steps)
// Bewegt das UI-Element um eine bestimmte Anzahl von Schritten nach unten und prüft auf Kollisionen.

// StepUp(UIElement obj, double steps)
// Bewegt das UI-Element um eine bestimmte Anzahl von Schritten nach oben und prüft auf Kollisionen.

// StepRight(UIElement obj, double steps)
// Bewegt das UI-Element um eine bestimmte Anzahl von Schritten nach rechts und prüft auf Kollisionen.

// StepLeft(UIElement obj, double steps)
// Bewegt das UI-Element um eine bestimmte Anzahl von Schritten nach links und prüft auf Kollisionen.

// MoveDown(UIElement obj, double speed)
// Startet die Bewegung eines UI-Elements nach unten mit der angegebenen Geschwindigkeit.

// MoveUp(UIElement obj, double speed)
// Startet die Bewegung eines UI-Elements nach oben mit der angegebenen Geschwindigkeit.

// MoveLeft(UIElement obj, double speed)
// Startet die Bewegung eines UI-Elements nach links mit der angegebenen Geschwindigkeit.

// MoveRight(UIElement obj, double speed)
// Startet die Bewegung eines UI-Elements nach rechts mit der angegebenen Geschwindigkeit.

// StartMoving(UIElement obj, double speed, Direction direction)
// Startet die Bewegung eines UI-Elements in die angegebene Richtung mit der angegebenen Geschwindigkeit.

// Stop(UIElement obj)
// Stoppt die Bewegung eines UI-Elements.

// GetRandomNumber(int min, int max)
// Gibt eine Zufallszahl im angegebenen Bereich zurück.

// Jump and Movement Enhancements
// ----------------------------------------------------------------------------

// Jump(UIElement player, double speed, int maximalJumpCount)
// Führt den Sprung für das angegebene UI-Element mit der angegebenen Geschwindigkeit und Sprunganzahl durch.

// Rotate(UIElement obj, double angle)
// Dreht ein UI-Element um den angegebenen Winkel.

// Orbit(UIElement obj, UIElement target, double radius, double angle)
// Lässt das UI-Element in einem Kreis um das Ziel-Element mit dem angegebenen Radius und Winkel kreisen.

// MirrorLeft(UIElement obj)
// Spiegelt das UI-Element nach links.

// MirrorRight(UIElement obj)
// Spiegelt das UI-Element nach rechts.

// Collision and Tagging
// ----------------------------------------------------------------------------

// GetHitBox(UIElement obj)
// Gibt die Hitbox eines UI-Elements zurück (benutzt für Kollisionserkennung).

// IsColliding(UIElement obj1, UIElement obj2)
// Überprüft, ob zwei UI-Elemente kollidieren.

// GetTag(UIElement obj)
// Gibt das Tag eines UI-Elements zurück (optional zur Kategorisierung von Objekten).

// IsCollidingToWall(UIElement obj)
// Überprüft, ob ein UI-Element mit einem Objekt oder einer Wand kollidiert.

// IsCollidingToItem(UIElement obj)
// Überprüft, ob ein UI-Element mit einem "Item" kollidiert.

// IsTagColliding(UIElement obj, string tag1, string tag2)
// Überprüft, ob zwei UI-Elemente mit den angegebenen Tags kollidieren.

// IsTagColliding(UIElement obj, string tag)
// Überprüft, ob ein UI-Element mit einem anderen Element kollidiert, das den angegebenen Tag besitzt.

// IsTagCollidingWithRemove(UIElement obj, string tag)
// Überprüft, ob ein UI-Element mit einem anderen Element kollidiert und entfernt dieses bei einer Kollision.

// Projectile Management
// ----------------------------------------------------------------------------

// InsertElementTag(UIElement sourceObj, Image obj, string tag)
// Fügt ein neues Bild mit einem Tag in das Spielfeld ein.

// Shoot(UIElement sourceObj, Image obj, string tag, double speed, Direction direction)
// Feuert ein Projektil von einem UI-Element in die angegebene Richtung mit der angegebenen Geschwindigkeit ab.

// Batch Movement
// ----------------------------------------------------------------------------

// MoveTagsToLeft(string tag, double step)
// Bewegt alle UI-Elemente mit einem bestimmten Tag nach links.

// MoveTagsToRight(string tag, double step)
// Bewegt alle UI-Elemente mit einem bestimmten Tag nach rechts.

// MoveTagsToTop(string tag, double step)
// Bewegt alle UI-Elemente mit einem bestimmten Tag nach oben.

// MoveTagsToBottom(string tag, double step)
// Bewegt alle UI-Elemente mit einem bestimmten Tag nach unten.

// Object Removal
// ----------------------------------------------------------------------------

// RemoveIfTagHitTo(UIElement obj, string tag)
// Entfernt ein UI-Element, wenn es mit einem anderen Element mit einem bestimmten Tag kollidiert.

// Animation and Sound
// ----------------------------------------------------------------------------

// StartAnimation(Image obj)
// Startet die Endlos-Wiederholung der Animation eines Bildes.

// StopAnimation(Image obj)
// Stoppt die Animation eines Bildes.

// PlaySound(string audioPath, int volume)
// Spielt einen Sound ab, der sich im angegebenen Pfad befindet, mit der angegebenen Lautstärke.