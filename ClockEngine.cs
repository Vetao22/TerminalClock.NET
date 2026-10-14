using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsciiArt.NET;

namespace TerminalClock.NET
{
    /// <summary>
    /// High-performance terminal clock engine rendering both digital ASCII banners and trigonometric analog clock faces.
    /// </summary>
    public sealed class ClockEngine
    {
        private readonly int _centerX;
        private readonly int _centerY;
        private readonly int _radius;

        /// <summary>
        /// Initializes a new instance of the ClockEngine with custom center coordinates and radius for the analog face.
        /// </summary>
        public ClockEngine(int centerX = 30, int centerY = 12, int radius = 8)
        {
            _centerX = centerX;
            _centerY = centerY;
            _radius = Math.Max(3, radius);
        }

        /// <summary>
        /// Runs the live clock loop in the terminal until cancelled.
        /// </summary>
        public async Task RunClockAsync(CancellationToken cancellationToken = default)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Clear();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    DateTime now = DateTime.Now;
                    RenderFrame(now);

                    // Update every second smoothly without flickering
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Graceful exit
            }
            finally
            {
                Console.ResetColor();
                Console.CursorVisible = true;
                Console.Clear();
            }
        }

        private void RenderFrame(DateTime time)
        {
            int screenWidth = Math.Max(80, Console.WindowWidth);
            int screenHeight = Math.Max(25, Console.WindowHeight);

            var buffer = new char[screenHeight, screenWidth];

            // Clear buffer with spaces
            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    buffer[y, x] = ' ';
                }
            }

            // 1. Render Analog Clock Face based on custom center coordinates
            DrawAnalogClock(buffer, time);

            // 2. Render Digital Time Banner using AsciiArt.NET below the analog face
            string digitalTimeString = time.ToString("HH:mm:ss");
            string asciiBanner = AsciiEngine.Render(digitalTimeString, scale: 1);
            
            DrawAsciiBanner(buffer, asciiBanner, startX: Math.Max(2, _centerX - 20), startY: _centerY + _radius + 3);

            // Output buffer to console starting at (0,0) to prevent flickering
            var sb = new StringBuilder();
            sb.AppendLine(" === TerminalClock.NET - Live Terminal Clock (Press ESC to Exit) ===");

            for (int y = 0; y < screenHeight - 1; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    sb.Append(buffer[y, x]);
                }
                sb.AppendLine();
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
        }

        private void DrawAnalogClock(char[,] buffer, DateTime time)
        {
            int height = buffer.GetLength(0);
            int width = buffer.GetLength(1);

            // Draw clock border circle markers
            for (int angle = 0; angle < 360; angle += 30)
            {
                double rad = angle * Math.PI / 180.0;
                int x = (int)Math.Round(_centerX + (_radius * 2 * Math.Cos(rad))); // Adjust for terminal character aspect ratio
                int y = (int)Math.Round(_centerY + (_radius * Math.Sin(rad)));

                if (y >= 0 && y < height && x >= 0 && x < width)
                {
                    buffer[y, x] = '·';
                }
            }

            // Center point marker
            if (_centerY >= 0 && _centerY < height && _centerX >= 0 && _centerX < width)
            {
                buffer[_centerY, _centerX] = 'O';
            }

            // Calculate hand angles
            double secondRad = (time.Second * 6 - 90) * Math.PI / 180.0;
            double minuteRad = (time.Minute * 6 - 90) * Math.PI / 180.0;
            double hourRad = ((time.Hour % 12) * 30 + time.Minute * 0.5 - 90) * Math.PI / 180.0;

            // Draw Hands (Hour, Minute, Second)
            DrawHand(buffer, _centerX, _centerY, hourRad, _radius * 0.5, 'H', ConsoleColor.Gray);
            DrawHand(buffer, _centerX, _centerY, minuteRad, _radius * 0.75, 'M', ConsoleColor.White);
            DrawHand(buffer, _centerX, _centerY, secondRad, _radius * 0.9, '.', ConsoleColor.Green);
        }

        private void DrawHand(char[,] buffer, int cx, int cy, double rad, double length, char symbol, ConsoleColor color)
        {
            int height = buffer.GetLength(0);
            int width = buffer.GetLength(1);

            int steps = (int)length;
            for (int i = 1; i <= steps; i++)
            {
                double currentLength = i;
                int x = (int)Math.Round(cx + (currentLength * 2 * Math.Cos(rad))); // Aspect ratio compensation
                int y = (int)Math.Round(cy + (currentLength * Math.Sin(rad)));

                if (y >= 0 && y < height && x >= 0 && x < width)
                {
                    buffer[y, x] = symbol;
                }
            }
        }

        private void DrawAsciiBanner(char[,] buffer, string banner, int startX, int startY)
        {
            int height = buffer.GetLength(0);
            int width = buffer.GetLength(1);

            string[] lines = banner.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int r = 0; r < lines.Length; r++)
            {
                string line = lines[r];
                for (int c = 0; c < line.Length; c++)
                {
                    int targetY = startY + r;
                    int targetX = startX + c;

                    if (targetY >= 0 && targetY < height && targetX >= 0 && targetX < width)
                    {
                        buffer[targetY, targetX] = line[c];
                    }
                }
            }
        }
    }
}