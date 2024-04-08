using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using static System.Console;
using System.Xml.Linq;
namespace Snake
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                GameManager gameManager = new GameManager();
                gameManager.RunGame();
                WriteLine("Press Enter to play again...");
            } while (ReadKey().Key == ConsoleKey.Enter);
        }
    }

    public class GameManager
    {
        private SnakeGame snakeGame;
        private ConsoleRenderer renderer;

        public GameManager()
        {
            snakeGame = new SnakeGame(32, 16);
            renderer = new ConsoleRenderer();
        }

        public void RunGame()
        {
            snakeGame.Initialize();
            while (!snakeGame.IsGameOver)
            {
                snakeGame.ProcessInput();
                snakeGame.Update();
                renderer.Render(snakeGame);
                Thread.Sleep(100);
            }
            renderer.RenderGameOver(snakeGame);
        }
    }

    public class SnakeGame
    {
        private Snake snake;
        private Berry berry;
        private int width;
        private int height;
        private Random random;
        private Stopwatch stopwatch;
        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

        public SnakeGame(int width, int height)
        {
            this.width = width;
            this.height = height;
            random = new Random();
            stopwatch = new Stopwatch();
        }

        public void Initialize()
        {
            snake = new Snake(width / 2, height / 2);
            berry = GenerateBerry();
            Score = 0;
            IsGameOver = false;
            stopwatch.Start();
        }

        public void ProcessInput()
        {
            
            Direction newDirection = Direction.None;
            if (KeyAvailable)
            {
                ConsoleKeyInfo key = ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        newDirection = Direction.Up;
                        break;
                    case ConsoleKey.DownArrow:
                        newDirection = Direction.Down;
                        break;
                    case ConsoleKey.LeftArrow:
                        newDirection = Direction.Left;
                        break;
                    case ConsoleKey.RightArrow:
                        newDirection = Direction.Right;
                        break;
                }

                snake.ChangeDirection(newDirection);
            }
            
        }

        public void Update()
        {

            if (stopwatch.ElapsedMilliseconds >= snake.MovementSpeed)
            {
                snake.Move();
                stopwatch.Restart();
            }

            if (snake.CollidesWith(berry))
            {
                snake.AddBerryToSnakeBody();
                berry = GenerateBerry();
                Score++;
            }
            
            IsGameOver = CheckIfGameOver(snake);
            
        }


        public bool CheckIfGameOver(Snake snake)
        {
            return (snake.CollidesWithSelf() || snake.CollidesWithBorder(width, height));
        }


        public Berry GenerateBerry()
        {
            int x = random.Next(1, width - 1);
            int y = random.Next(1, height - 1);
            return new Berry(x, y);
        }

        public IEnumerable<Pixel> GetGameObjects()
        {
            yield return snake.Head;
            foreach (var part in snake.Body)
                yield return part;
            yield return berry;
        }

        public int GetScreenWidth()
        {
            return width;
        }

        public int GetScreenHeight()
        {
            return height;
        }


    }

    public class Snake
    {
        private LinkedList<Pixel> body;
        public Pixel Head { get; private set; }
        public IEnumerable<Pixel> Body { get { return body; } }
        public Direction Direction { get; private set; }
        public int MovementSpeed { get; private set; }
        public string HeadShape { get; set; }
        public string BodyShape { get; set; }


        public Snake(int x, int y)
        {
            HeadShape = "O";
            Head = new Pixel(x, y, ConsoleColor.Red, HeadShape);
            BodyShape = "=";
            body = InitializeBody(2, BodyShape);
            Direction = Direction.Right;
            MovementSpeed = 200;
        }

        private LinkedList<Pixel> InitializeBody(int initialBodyLength, string shape)
        {
            LinkedList<Pixel> newBody = new LinkedList<Pixel>();

            for (int i = 0; i < initialBodyLength; i++)
            {
                int offsetX = i + 1;
                newBody.AddLast(new Pixel(Head.PositionX - offsetX, Head.PositionY, ConsoleColor.Green, shape));
            }

            return newBody;
        }

        public void ChangeDirection(Direction direction)
        {
           Direction = direction;
        }

        public void Move()
        {
            Pixel newHead = Head.GetNeighbor(Direction);
            Pixel bodyPart = new Pixel(Head.PositionX, Head.PositionY, ConsoleColor.Green, BodyShape);
            Head = newHead;
            body.AddFirst(bodyPart);
            body.RemoveLast();
        }

        public bool CollidesWith(Pixel pixel)
        {
            return Head.Equals(pixel);
        }

        public bool CollidesWithSelf()
        {
            return body.Any(p => p.Equals(Head));
        }

        public bool CollidesWithBorder(int width, int height)
        {
            return Head.PositionX == 0 || Head.PositionX == width - 1 ||
                   Head.PositionY == 0 || Head.PositionY == height - 1;
        }

        public void AddBerryToSnakeBody()
        {
            Pixel lastBodyPart = body.Last();
            Pixel newBodyPart = new Pixel(lastBodyPart.PositionX, lastBodyPart.PositionY, ConsoleColor.Green, BodyShape);
            body.AddLast(newBodyPart);
            
        }
    }

    public class Pixel
    {
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public ConsoleColor Color { get; set; }
        public string Shape { get; set; }

        public Pixel(int x, int y, ConsoleColor color, string shape)
        {
            PositionX = x;
            PositionY = y;
            Color = color;
            Shape = shape;
        }

        public Pixel GetNeighbor(Direction direction)
        {
            int x = PositionX;
            int y = PositionY;
            switch (direction)
            {
                case Direction.Up:
                    y--;
                    break;
                case Direction.Down:
                    y++;
                    break;
                case Direction.Left:
                    x--;
                    break;
                case Direction.Right:
                    x++;
                    break;
            }
            return new Pixel(x, y, ConsoleColor.Red, Shape);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Pixel))
                return false;
            var other = (Pixel)obj;
            return PositionX == other.PositionX && PositionY == other.PositionY;
        }


    }

    public class Berry : Pixel
    {
        public Berry(int x, int y) : base(x, y, ConsoleColor.White, "@") { }
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public class ConsoleRenderer
    {
        public void Render(SnakeGame game)
        {
            Clear();
            DrawBorder(game.GetScreenWidth(), game.GetScreenHeight());
            foreach (var obj in game.GetGameObjects())
            {
                SetCursorPosition(obj.PositionX, obj.PositionY);
                ForegroundColor = obj.Color;
                Write(obj.Shape);
            }
        }

        public void RenderGameOver(SnakeGame game)
        {

            SetCursorPosition(game.GetScreenWidth() / 5, game.GetScreenHeight() / 2);
            WriteLine("Game over, Score: " + game.Score);
            SetCursorPosition(game.GetScreenWidth() / 5, game.GetScreenHeight() / 2 + 1);
            ReadKey();
        }

        public static void DrawBorder(int screenWidth, int screenHeight)
        {
            for (int i = 0; i < screenWidth; i++)
            {
                SetCursorPosition(i, 0);
                Write("■");
                SetCursorPosition(i, screenHeight - 1);
                Write("■");
            }

            for (int i = 0; i < screenHeight; i++)
            {
                SetCursorPosition(0, i);
                Write("■");
                SetCursorPosition(screenWidth - 1, i);
                Write("■");
            }
        }
    }
}
