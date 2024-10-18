using Game;
using Game.Engine.Enums;
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

namespace _8_2_Game;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isJumping = false;
    private bool attack = false;


    public MainWindow()
    {
        InitializeComponent();
        GameField.Init( 1, UpdateGame, true);
        Pikachu.InitPlayer();
        RegisterKeys();

    }

    private void RegisterKeys()
    {
        GameHelper.RegisterKeyDownAction(Key.Left, () => { Pikachu.MoveLeft(10);Pikachu.StartAnimation();Pikachu.MirrorRight(); });
        GameHelper.RegisterKeyDownAction(Key.Right, () => { Pikachu.MoveRight(10); Pikachu.StartAnimation();Pikachu.MirrorLeft(); });
        GameHelper.RegisterKeyDownAction(Key.Space, () => Pikachu.Jump(20, 10));
        GameHelper.RegisterKeyDownAction(Key.S, () => Pikachu.Shoot(energy,"shoot",10,Direction.Left));


        GameHelper.RegisterKeyUpAction(Key.Left, () => { Pikachu.Stop();Pikachu.StopAnimation(); });
        GameHelper.RegisterKeyUpAction(Key.Right, () => { Pikachu.Stop(); Pikachu.StopAnimation(); });

        GameHelper.RegisterKeyUpAction(Key.S, () => attack = false);
    }


    private void UpdateGame()
    {
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