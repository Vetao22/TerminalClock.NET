using System;
using System.Threading;
using System.Threading.Tasks;
using TerminalClock.NET;

namespace TerminalClockDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure custom clock center position (CenterX = 35, CenterY = 10, Radius = 7)
            var clockEngine = new ClockEngine(centerX: 35, centerY: 10, radius: 7);
            
            using var cts = new CancellationTokenSource();

            // Handle ESC key to exit cleanly
            var inputTask = Task.Run(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        cts.Cancel();
                        break;
                    }
                    Thread.Sleep(100);
                }
            });

            // Start live clock loop
            await clockEngine.RunClockAsync(cts.Token);

            Console.WriteLine("TerminalClock session terminated. Have a great day!");
        }
    }
}