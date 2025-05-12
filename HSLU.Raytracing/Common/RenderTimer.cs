using System;
using System.Diagnostics;
using System.Threading;

namespace Common
{
    public class RenderTimer
    {
        private Stopwatch stopwatch = new Stopwatch();
        private int totalScanlines;
        private int completedScanlines;
        private DateTime startTime;
        private object lockObj = new object();

        public RenderTimer(int totalScanlines)
        {
            this.totalScanlines = totalScanlines;
            this.completedScanlines = 0;
        }

        public void Start()
        {
            stopwatch.Start();
            startTime = DateTime.Now;
            Console.WriteLine($"Render started at {startTime}");
        }

        public void IncrementProgress(int scanlines = 1)
        {
            lock (lockObj)
            {
                completedScanlines += scanlines;

                // Progress updates every 1%
                if (completedScanlines % Math.Max(1, totalScanlines / 100) == 0)
                {
                    PrintProgress();
                }
            }
        }

        public void Stop()
        {
            stopwatch.Stop();
            DateTime endTime = DateTime.Now;
            TimeSpan renderTime = stopwatch.Elapsed;

            Console.WriteLine("\n========== Render Completed ==========");
            Console.WriteLine($"Started: {startTime}");
            Console.WriteLine($"Finished: {endTime}");
            Console.WriteLine($"Total time: {FormatTimeSpan(renderTime)}");
            Console.WriteLine($"Average time per scanline: {renderTime.TotalMilliseconds / totalScanlines:F2} ms");
            Console.WriteLine("=======================================");
        }

        private void PrintProgress()
        {
            double percentComplete = (double)completedScanlines / totalScanlines * 100;
            TimeSpan elapsed = stopwatch.Elapsed;

            // Estimate remaining time
            double msPerLine = elapsed.TotalMilliseconds / completedScanlines;
            int remainingLines = totalScanlines - completedScanlines;
            TimeSpan estimatedRemaining = TimeSpan.FromMilliseconds(msPerLine * remainingLines);

            // Estimate completion time
            DateTime estimatedCompletion = DateTime.Now.Add(estimatedRemaining);

            // Output progress
            Console.Write($"\rProgress: {percentComplete:F2}% | " +
                         $"Time: {FormatTimeSpan(elapsed)} | " +
                         $"ETA: {FormatTimeSpan(estimatedRemaining)} | " +
                         $"Estimated completion: {estimatedCompletion.ToString("HH:mm:ss")}     ");
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
            {
                return $"{ts.Hours}h {ts.Minutes:D2}m {ts.Seconds:D2}s";
            }
            else if (ts.TotalMinutes >= 1)
            {
                return $"{ts.Minutes}m {ts.Seconds:D2}s";
            }
            else
            {
                return $"{ts.Seconds}.{ts.Milliseconds / 10:D2}s";
            }
        }
    }
}