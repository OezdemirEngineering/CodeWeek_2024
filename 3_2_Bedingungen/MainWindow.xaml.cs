using Game;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _3_2_Bedingungen;

public partial class MainWindow : Window
{
    private Path pacman;
    private double pacSpeed = 3;
    private int score = 0;
    private TextBlock scoreText;

    // Movement
    private bool movingRight, movingDown, movingLeft, movingUp;
    private enum PacDir { Right, Down, Left, Up }
    private PacDir currentDir = PacDir.Right;

    // Mouth animation
    private bool mouthClosing = false;
    private double mouthAngle = 40;
    private const double pacmanRadius = 12;

    // Map Data
    private Point pacmanStart = new(50, 200);
    private List<Point> ghostStarts = new();
    private List<Canvas> ghosts = new();

    // Random for ghosts
    private Random rnd = new();
    private Dictionary<Canvas, int> ghostDirs = new();

    public MainWindow()
    {
        InitializeComponent();

        GameField.Init(frameRate: 30, UpdateGame);

        BuildLevel();
        CreatePacman();
        CreateGhosts();

        RegisterControls();
        GameField.Focus();
    }

    // --- Game Loop ---
    private void UpdateGame()
    {
        if (movingRight) pacman.MoveRight(pacSpeed);
        else if (movingDown) pacman.MoveDown(pacSpeed);
        else if (movingLeft) pacman.MoveLeft(pacSpeed);
        else if (movingUp) pacman.MoveUp(pacSpeed);

        AnimateMouth();
        UpdatePacmanShape();
        MoveGhosts();
        //*******************************************************************
        // Todo detect collision with coins
        //pacman.IsTagCollidingWithRemove("tag name") give true
        //if collision and removes the object with the tag
        // increase score by 10
        //*******************************************************************

    }

    // --- Controls ---
    private void RegisterControls()
    {
        GameHelper.RegisterKeyDownAction(Key.Right, () => { movingRight = true; currentDir = PacDir.Right; pacman.MoveRight(pacSpeed); });
        GameHelper.RegisterKeyDownAction(Key.Down, () => { movingDown = true; currentDir = PacDir.Down; pacman.MoveDown(pacSpeed); });
        GameHelper.RegisterKeyDownAction(Key.Left, () => { movingLeft = true; currentDir = PacDir.Left; pacman.MoveLeft(pacSpeed); });
        GameHelper.RegisterKeyDownAction(Key.Up, () => { movingUp = true; currentDir = PacDir.Up; pacman.MoveUp(pacSpeed); });

        GameHelper.RegisterKeyUpAction(Key.Right, () => { movingRight = false; pacman.Stop(); });
        GameHelper.RegisterKeyUpAction(Key.Down, () => { movingDown = false; pacman.Stop(); });
        GameHelper.RegisterKeyUpAction(Key.Left, () => { movingLeft = false; pacman.Stop(); });
        GameHelper.RegisterKeyUpAction(Key.Up, () => { movingUp = false; pacman.Stop(); });
    }

    // --- Level from String Map ---
    private void BuildLevel()
    {
        // Score Anzeige
        scoreText = new TextBlock
        {
            Text = "SCORE: 0",
            Foreground = Brushes.Yellow,
            FontSize = 18,
            FontFamily = new FontFamily("Consolas")
        };
        Canvas.SetLeft(scoreText, 10);
        Canvas.SetTop(scoreText, 10);
        GameField.Children.Add(scoreText);

        // Map Definition
        string[] map =
        {
    "##############################",
    "#............................#",
    "#...............G............#",
    "#............................#",
    "#.....###....###....####.....#",
    "#....#......#.......#...#....#",
    "#....#.......###....####.....#",
    "#....#..........#...#...#....#",
    "#....#......#...#...#...#....#",
    "#.....###....###....####.....#",
    "#............................#",
    "#...G...........P............#",
    "#............................#",
    "#.......G..............G.....#",
    "#............................#",
    "##############################"
};



        double tileSize = 30;

        for (int y = 0; y < map.Length; y++)
        {
            for (int x = 0; x < map[y].Length; x++)
            {
                char c = map[y][x];
                double posX = x * tileSize;
                double posY = y * tileSize + 40;

                switch (c)
                {
                    case '#': // Wand
                        var wall = new Rectangle
                        {
                            Width = tileSize,
                            Height = tileSize,
                            Fill = Brushes.Blue,
                            Tag = "wall"
                        };
                        wall.SetPosition(posX, posY);
                        GameField.Children.Add(wall);
                        break;

                    case '.': // Punkt
                        var dot = new Ellipse
                        {
                            Width = 6,
                            Height = 6,
                            Fill = Brushes.White,
                            Tag = "coin"
                        };
                        dot.SetPosition(posX + (tileSize - 6) / 2, posY + (tileSize - 6) / 2);
                        GameField.Children.Add(dot);
                        break;

                    case 'P': // Pacman Start
                        pacmanStart = new Point(posX, posY);
                        break;

                    case 'G': // Ghost Start
                        ghostStarts.Add(new Point(posX, posY));
                        break;
                }
            }
        }
    }

    // --- Pacman ---
    private void CreatePacman()
    {
        pacman = new Path
        {
            Fill = Brushes.Yellow,
            Tag = "player",
            Width = pacmanRadius * 2,
            Height = pacmanRadius * 2
        };
        GameField.Children.Add(pacman);
        pacman.SetPosition(pacmanStart.X, pacmanStart.Y);
        UpdatePacmanShape();
    }

    private void UpdatePacmanShape()
    {
        double dirAngle = currentDir switch
        {
            PacDir.Right => 0,
            PacDir.Down => 90,
            PacDir.Left => 180,
            PacDir.Up => 270,
            _ => 0
        };

        double startAngle = dirAngle - mouthAngle / 2;
        double endAngle = dirAngle + mouthAngle / 2;

        double startRad = startAngle * Math.PI / 180;
        double endRad = endAngle * Math.PI / 180;

        Point center = new Point(pacmanRadius, pacmanRadius);
        Point start = new Point(center.X + Math.Cos(startRad) * pacmanRadius,
                                center.Y + Math.Sin(startRad) * pacmanRadius);
        Point end = new Point(center.X + Math.Cos(endRad) * pacmanRadius,
                              center.Y + Math.Sin(endRad) * pacmanRadius);

        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            ctx.BeginFigure(center, true, true);
            ctx.LineTo(start, true, true);
            ctx.ArcTo(end, new Size(pacmanRadius, pacmanRadius),
                0, true, SweepDirection.Counterclockwise, true, false);
            ctx.LineTo(center, true, true);
        }
        geom.Freeze();
        pacman.Data = geom;
    }

    private void AnimateMouth()
    {
        if (mouthClosing)
        {
            mouthAngle -= 3;
            if (mouthAngle <= 10) mouthClosing = false;
        }
        else
        {
            mouthAngle += 3;
            if (mouthAngle >= 40) mouthClosing = true;
        }
    }

    // --- Ghosts ---
    private Canvas CreateGhost(Color bodyColor)
    {
        double w = 28;
        double h = 34;
        double baseY = h - 6;

        var geom = new PathGeometry();
        var fig = new PathFigure { StartPoint = new Point(0, baseY) };

        fig.Segments.Add(new ArcSegment(
            new Point(w, baseY),
            new Size(w / 2, h / 1.2),
            0, false, SweepDirection.Clockwise, true));

        int spikes = 4;
        double spikeWidth = w / spikes;
        for (int i = 0; i < spikes; i++)
        {
            double x1 = w - i * spikeWidth - spikeWidth / 2;
            double y1 = baseY;
            double x2 = w - (i + 1) * spikeWidth;
            double y2 = baseY + ((i % 2 == 0) ? 5 : 0);
            fig.Segments.Add(new LineSegment(new Point(x1, y1), true));
            fig.Segments.Add(new LineSegment(new Point(x2, y2), true));
        }

        fig.IsClosed = true;
        geom.Figures.Add(fig);

        var ghostBody = new Path
        {
            Data = geom,
            Fill = new SolidColorBrush(bodyColor),
            Stroke = Brushes.Black,
            StrokeThickness = 1.5
        };

        var leftEye = new Ellipse { Width = 6, Height = 8, Fill = Brushes.White };
        var rightEye = new Ellipse { Width = 6, Height = 8, Fill = Brushes.White };
        var leftPupil = new Ellipse { Width = 3, Height = 3, Fill = Brushes.Blue };
        var rightPupil = new Ellipse { Width = 3, Height = 3, Fill = Brushes.Blue };

        var ghostCanvas = new Canvas { Width = w, Height = h, Tag = "ghost" };
        ghostCanvas.Children.Add(ghostBody);

        Canvas.SetLeft(leftEye, 6); Canvas.SetTop(leftEye, 8);
        Canvas.SetLeft(rightEye, 15); Canvas.SetTop(rightEye, 8);
        Canvas.SetLeft(leftPupil, 8); Canvas.SetTop(leftPupil, 11);
        Canvas.SetLeft(rightPupil, 17); Canvas.SetTop(rightPupil, 11);

        ghostCanvas.Children.Add(leftEye);
        ghostCanvas.Children.Add(rightEye);
        ghostCanvas.Children.Add(leftPupil);
        ghostCanvas.Children.Add(rightPupil);

        GameField.Children.Add(ghostCanvas);
        return ghostCanvas;
    }

    private void CreateGhosts()
    {
        Color[] colors = { Colors.Red, Colors.Pink, Colors.Cyan, Colors.Orange };
        for (int i = 0; i < ghostStarts.Count && i < colors.Length; i++)
        {
            var ghost = CreateGhost(colors[i]);
            ghost.SetPosition(ghostStarts[i].X, ghostStarts[i].Y);
            ghosts.Add(ghost);
        }
    }

    private void MoveGhosts()
    {
        // Pacman-Position holen
        var pacPos = pacman.GetPosition();

        foreach (var ghost in ghosts)
        {
            // Ghost-Position holen
            var ghostPos = ghost.GetPosition();

            // Map-Definition holen (aus BuildLevel kopieren)
            string[] map =
            {
                "##############################",
                "#............................#",
                "#...............G............#",
                "#............................#",
                "#.....###....###....####.....#",
                "#....#......#.......#...#....#",
                "#....#.......###....####.....#",
                "#....#..........#...#...#....#",
                "#....#......#...#...#...#....#",
                "#.....###....###....####.....#",
                "#............................#",
                "#...G...........P............#",
                "#............................#",
                "#.......G..............G.....#",
                "#............................#",
                "##############################"
            };

            // Pfad berechnen
            var path = FindPath(ghostPos, pacPos, map);

            // Wenn Pfad existiert und mindestens ein Schritt vorhanden ist
            if (path.Count > 0)
            {
                // Nächsten Schritt bestimmen
                var next = path[0];
                double speed = 2.0;

                // Richtung berechnen
                double dx = next.X - ghostPos.X;
                double dy = next.Y - ghostPos.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist > 0.01)
                {
                    double step = Math.Min(speed, dist);
                    ghost.SetPosition(ghostPos.X + dx / dist * step, ghostPos.Y + dy / dist * step);
                }
            }

            if (ghost.IsColliding(pacman))
            {
                scoreText.Text = "GAME OVER 💀";
                GameHelper.StopUpdateGame();
            }
        }
    }

    /// <summary>
    /// Berechnet den kürzesten Pfad (BFS) von Start zu Ziel in einer Pac-Man-Map.
    /// '#' = Wand, alles andere begehbar.
    /// </summary>
    private List<Point> FindPath(Point start, Point goal, string[] map, double tileSize = 30, double yOffset = 40)
    {
        int width = map[0].Length;
        int height = map.Length;

        // Richtung: rechts, links, unten, oben
        var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

        // Position -> Gitterkoordinaten
        Point ToGrid(Point p)
        {
            int gx = (int)(p.X / tileSize);
            int gy = (int)((p.Y - yOffset) / tileSize);
            gx = Math.Clamp(gx, 0, width - 1);
            gy = Math.Clamp(gy, 0, height - 1);
            return new Point(gx, gy);
        }

        var startGrid = ToGrid(start);
        var goalGrid = ToGrid(goal);

        var queue = new Queue<Point>();
        var cameFrom = new Dictionary<Point, Point>();
        queue.Enqueue(startGrid);
        cameFrom[startGrid] = startGrid;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Equals(goalGrid))
                break;

            foreach (var (dx, dy) in dirs)
            {
                int nx = (int)current.X + dx;
                int ny = (int)current.Y + dy;

                // Innerhalb Karte und kein Wandfeld?
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    if (map[ny][nx] != '#' && !cameFrom.ContainsKey(new Point(nx, ny)))
                    {
                        var next = new Point(nx, ny);
                        cameFrom[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }
        }

        // Kein Pfad gefunden
        if (!cameFrom.ContainsKey(goalGrid))
            return new List<Point>();

        // Rückverfolgung
        var path = new List<Point>();
        var cur = goalGrid;
        while (!cur.Equals(startGrid))
        {
            path.Add(cur);
            cur = cameFrom[cur];
        }
        path.Reverse();

        // Grid -> Canvas-Koordinaten umwandeln
        return path.Select(p => new Point(p.X * tileSize, p.Y * tileSize + yOffset)).ToList();
    }

}
