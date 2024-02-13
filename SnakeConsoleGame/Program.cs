using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static System.Console;
using System.Data;
using System.Dynamic;
using System.Diagnostics;
using System.Windows.Markup;
using System.Xml.Linq;


namespace Snake
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializePlayingBoard();

            Berry berry = Berry.CreateBerry();
            Snake snake = Snake.Create();


            while (true)
            {
                Clear();
                DrawBorder(WindowWidth, WindowHeight);


                if (snake.IsBerryEaten(berry))
                {
                    snake.IncrementScore();
                    berry = Berry.CreateBerry();
                }


                DrawPixel(snake.Head);
                DrawPixel(berry.SpawnPosition);
                snake.DrawBody();

                var stopWatch = Stopwatch.StartNew();
                while (stopWatch.ElapsedMilliseconds <= snake.MovementSpeed)
                {
                    snake.ChangeDirection(ReadNextMovement(snake.CurrentDirection));
                }


                snake.Body.Add(new Pixel(snake.Head.PositionX, snake.Head.PositionY, ConsoleColor.Green));
                snake.MoveToCurrentlySetDirection();


                if (snake.Body.Count > snake.Score)
                {
                    snake.Body.RemoveAt(0);
                }


                ForegroundColor = ConsoleColor.White;
                if (CheckIfGameOver(snake))
                {
                    break;
                }
            }
            SetCursorPosition(WindowWidth / 5, WindowHeight / 2);
            WriteLine("Game over, Score: " + snake.Score);
            SetCursorPosition(WindowWidth / 5, WindowHeight / 2 + 1);
            ReadKey();
        }


        private static bool CheckIfGameOver(Snake snake)
        {
            return snake.IsCollidedWithBody() || snake.IsCollidedWithBorder();
        }


        private static void InitializePlayingBoard()
        {
            WindowHeight = 16;
            WindowWidth = 32;
        }

        private static void DrawPixel(Pixel pixel)
        {
            SetCursorPosition(pixel.PositionX, pixel.PositionY);
            ForegroundColor = pixel.ScreenColor;
            Write("■");
        }


        static Direction ReadNextMovement(Direction movement)
        {
            if (!KeyAvailable)
                return movement;
            var key = ReadKey(true).Key;


            switch (key)
            {
                case ConsoleKey.UpArrow when movement != Direction.Down:
                    return Direction.Up;
                case ConsoleKey.DownArrow when movement != Direction.Up:
                    return Direction.Down;
                case ConsoleKey.LeftArrow when movement != Direction.Right:
                    return Direction.Left;
                case ConsoleKey.RightArrow when movement != Direction.Left:
                    return Direction.Right;
                default:
                    return movement;
            }
        }



        static void DrawBorder(int screenWidth, int screenHeight)
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


        class Pixel
        {
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public ConsoleColor ScreenColor { get; set; }

            public Pixel(int xPos, int yPos, ConsoleColor color)
            {
                PositionX = xPos;
                PositionY = yPos;
                ScreenColor = color;
            }

        }


        class Berry
        {
            public Pixel SpawnPosition { get; set; }


            public static Berry CreateBerry()
            {
                return new Berry();
            }


            private Berry()
            {
                SpawnPosition = GetRandomSpawnPosition();
            }


            private Pixel GetRandomSpawnPosition()
            {
                Random randomNumber = new Random();
                Pixel randomPosition = new Pixel(randomNumber.Next(1, WindowWidth - 2), randomNumber.Next(1, WindowHeight - 2), ConsoleColor.Cyan);
                return randomPosition;
            }
        }


        enum Direction
        {
            Up,
            Down,
            Right,
            Left
        }


        class Snake
        {
            public Pixel Head { get; set; }
            public List<Pixel> Body { get; set; }
            public int MovementSpeed { get; set; }
            public Direction CurrentDirection { get; set; }
            public int Score { get; set; }


            private Snake()
            {
                Head = new Pixel(WindowWidth / 2, WindowHeight / 2, ConsoleColor.Red);
                Body = new List<Pixel>();
                CurrentDirection = GetRandomDirection();
                Score = 5;
                MovementSpeed = 500;
            }


            private Direction GetRandomDirection()
            {
                Array value = Enum.GetValues(typeof(Direction));
                Random random = new Random();
                Direction randomDirection = (Direction)value.GetValue(random.Next(value.Length));
                return randomDirection;
            }


            public void DrawBody()
            {
                foreach (Pixel bodyPart in Body)
                {
                    bodyPart.ScreenColor = ConsoleColor.Green;
                    DrawPixel(bodyPart);
                }
            }


            public static Snake Create()
            {
                return new Snake();
            }


            public bool IsBerryEaten(Berry berry)
            {
                return (berry.SpawnPosition.PositionX == Head.PositionX && berry.SpawnPosition.PositionY == Head.PositionY);
            }


            public bool IsCollidedWithBorder()
            {
                return (Head.PositionX == WindowWidth - 1 || Head.PositionX == 0 || Head.PositionY == WindowHeight - 1 || Head.PositionY == 0);
            }


            public bool IsCollidedWithBody()
            {
                for (int i = 0; i < Body.Count(); i++)
                {
                    if (Body[i].PositionX == Head.PositionX && Body[i].PositionY == Head.PositionY)
                    {
                        return true;
                    }
                }
                return false;
            }


            public void ChangeDirection(Direction newDirection)
            {
                CurrentDirection = newDirection;
            }


            public void MoveToCurrentlySetDirection()
            {
                switch (CurrentDirection)
                {
                    case Direction.Up:
                        Head.PositionY--;
                        break;
                    case Direction.Down:
                        Head.PositionY++;
                        break;
                    case Direction.Left:
                        Head.PositionX--;
                        break;
                    case Direction.Right:
                        Head.PositionX++;
                        break;
                }
            }


            public void IncrementScore()
            {
                Score++;
            }
        }
    }
}