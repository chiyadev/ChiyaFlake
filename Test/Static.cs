using System;
using System.Diagnostics;
using System.Threading;
using ChiyaFlake;
using NUnit.Framework;

namespace Test
{
    [Parallelizable(ParallelScope.All)]
    public class Static
    {
        [Test]
        public void Generate()
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 20; i++)
            {
                Console.WriteLine(Snowflake.New);

                var sleep = i * 50 - stopwatch.Elapsed.TotalMilliseconds;

                if (sleep > 0)
                    Thread.Sleep(TimeSpan.FromMilliseconds(sleep));
            }
        }

        [Test]
        public void GenerateQuick()
        {
            for (var i = 0; i < 20; i++)
                Console.WriteLine(Snowflake.New);
        }
    }
}