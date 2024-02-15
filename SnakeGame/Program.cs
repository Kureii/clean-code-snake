using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using static System.Convert;

namespace Snake
{
    class Program
    {
        private static Int32 screenWidth = WindowWidth = 32;
        private static Int32 screenHeight = WindowHeight = 16;
        private static Random rand = new Random();
        private static Int32 startScore = 5;
        private static Int32 score = startScore;
        private static Int32 oldScore = startScore;
        private static Pixel head = new Pixel(screenWidth / 2, screenHeight / 2, ConsoleColor.Red);
        private static List<Int32> xPosBody = new List<Int32>();
        private static List<Int32> yPosBody = new List<Int32>();
        private static Int32 xPosBerry = rand.Next(1, screenWidth - 2);
        private static Int32 yPosBerry = rand.Next(1, screenHeight - 2);
        private static Direction movement = Direction.RIGHT;
        private static Int32 timer = 0;
        private static Int32 startTime = 100;
        private static Int32 actualTime = startTime;
        private static Int32 maxScore = (WindowWidth - 2) * (WindowHeight - 2);
        private static Int32 middlePoint = maxScore / 2;
        private static Int32 endTime = 30;

        enum Direction
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

        static void Main(string[] args)
        {
            while (GameLoop(timer))
            {
                timer++;
                timer %= actualTime;
                if (oldScore != score)
                {
                    actualTime = CalculateBezierY(0, startTime, maxScore, endTime, score-startScore, score-startScore <= middlePoint);
                    oldScore++;
                }
                
            }

            SetCursorPosition(screenWidth / 5, screenHeight / 2);
            if (score >= maxScore)
            {
                WriteLine($"You win");
                
            }
            else
            {
                WriteLine($"Game over, Score: {score-startScore}");
                
            }
            ReadKey(true);
        }

        private static Int32 CalculateBezierY(Int32 x1, Int32 y1, Int32 x2, Int32 y2, Int32 xInput, bool up)
        {
            double cx1;
            double cy1;
            double cx2;
            double cy2;
            if (up)
            {
                cx1 = x1;
                cy1 = y1 - (y1 - y2) / maxScore * 10;
                cx2 = x2 - (x1 - x2) / maxScore * 10;
                cy2 = y2 + (y1 - y2) / 5;
            }
            else
            {
                cx1 = x1 + (x2 - x1) / 2.5;
                cy1 = y1;
                cx2 = x2;
                cy2 = y2 + (y1 - y2) / 2.5;
            }

            double t = (xInput - x1) / (x2 - x1);
            double xOutput;
            const double epsilon = 0.0001;
            Int32 maxIterations = 100; 
            Int32 i = 0;
            do {
                xOutput = Math.Pow(1 - t, 3) * x1 +
                          3 * Math.Pow(1 - t, 2) * t * (x1 + cx1) +
                          3 * (1 - t) * Math.Pow(t, 2) * (x2 + cx2) +
                          Math.Pow(t, 3) * x2;
                double derivative = -3 * Math.Pow(1 - t, 2) * x1 +
                                    3 * Math.Pow(1 - t, 2) * (x1 + cx1) -
                                    6 * t * (1 - t) * (x1 + cx1) +
                                    6 * t * (1 - t) * (x2 + cx2) -
                                    3 * Math.Pow(t, 2) * (x2 + cx2) +
                                    3 * Math.Pow(t, 2) * x2;
                if (derivative == 0) {
                    break;
                }
                double tNext = t - (xOutput - xInput) / derivative;
                if (Math.Abs(t - tNext) < epsilon) {
                    break;
                }
                t = tNext;
                i++;
            } while (i < maxIterations && Math.Abs(xOutput - xInput) > epsilon);
            
            double yOutput = Math.Pow(1 - t, 3) * y1 +
                             3 * Math.Pow(1 - t, 2) * t * cy1 +
                             3 * (1 - t) * Math.Pow(t, 2) * cy2 +
                             Math.Pow(t, 3) * y2;

            return ToInt32(yOutput);
        }

        private static bool GameLoop(Int32 timer)
        {
            Thread.Sleep(5);
            if (timer == 0)
            {
                Clear();
                DrawBorders();
                ProcessInput();
                UpdateGameState();
                DrawGame();
            }
            return !IsGameOver();
        }

        private static void DrawBorders()
        {
            ForegroundColor = ConsoleColor.White;
            for (Int32 x = 0; x < screenWidth; x++)
            {
                SetCursorPosition(x, 0);
                Write("■");
                SetCursorPosition(x, screenHeight - 1);
                Write("■");
            }

            for (Int32 y = 0; y < screenHeight; y++)
            {
                SetCursorPosition(0, y);
                Write("■");
                SetCursorPosition(screenWidth - 1, y);
                Write("■");
            }
            WriteLine();
            WriteLine($"score: {score-startScore}");
            WriteLine($"actualTime: {actualTime}");
        }

        private static void ProcessInput()
        {
            if (!KeyAvailable) return;

            var key = ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow when movement != Direction.DOWN:
                    movement = Direction.UP;
                    break;
                case ConsoleKey.DownArrow when movement != Direction.UP:
                    movement = Direction.DOWN;
                    break;
                case ConsoleKey.LeftArrow when movement != Direction.RIGHT:
                    movement = Direction.LEFT;
                    break;
                case ConsoleKey.RightArrow when movement != Direction.LEFT:
                    movement = Direction.RIGHT;
                    break;
            }
        }

        private static void UpdateGameState()
        {
            xPosBody.Add(head.XPos);
            yPosBody.Add(head.YPos);

            switch (movement)
            {
                case Direction.UP:
                    head.YPos--;
                    break;
                case Direction.DOWN:
                    head.YPos++;
                    break;
                case Direction.LEFT:
                    head.XPos--;
                    break;
                case Direction.RIGHT:
                    head.XPos++;
                    break;
            }

            if (head.XPos == xPosBerry && head.YPos == yPosBerry)
            {
                score++;
                do
                {
                    xPosBerry = rand.Next(1, screenWidth - 2);
                    yPosBerry = rand.Next(1, screenHeight - 2);
                } while (xPosBody.Contains(xPosBerry) && yPosBody.Contains(yPosBerry));
                
            }
            else if (xPosBody.Count > score)
            {
                xPosBody.RemoveAt(0);
                yPosBody.RemoveAt(0);
            }
        }

        private static void DrawGame()
        {
            DrawPixel(xPosBerry, yPosBerry, ConsoleColor.Cyan);

            for (Int32 i = 0; i < xPosBody.Count; i++)
            {
                DrawPixel(xPosBody[i], yPosBody[i], ConsoleColor.Green);
            }

            DrawPixel(head.XPos, head.YPos, head.ScreenColor);
        }

        private static void DrawPixel(Int32 x, Int32 y, ConsoleColor color)
        {
            SetCursorPosition(x, y);
            ForegroundColor = color;
            Write("■");
        }

        private static bool IsGameOver()
        {
            if (head.XPos == screenWidth - 1 || head.XPos == 0 || head.YPos == screenHeight - 1 || head.YPos == 0)
                return true;

            for (Int32 i = 0; i < xPosBody.Count; i++)
            {
                if (xPosBody[i] == head.XPos && yPosBody[i] == head.YPos)
                    return true;
            }

            if (score == maxScore)
            {
                return true;
            }

            return false;
        }


        class Pixel
        {
            public Pixel(Int32 xPos, Int32 yPos, ConsoleColor color)
            {
                XPos = xPos;
                YPos = yPos;
                ScreenColor = color;
            }

            public Int32 XPos { get; set; }
            public Int32 YPos { get; set; }
            public ConsoleColor ScreenColor { get; set; }
        }
    }
}
