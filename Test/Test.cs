using System;
using System.Diagnostics;
using System.Threading;
using ChiyaFlake;
using NUnit.Framework;

namespace Test
{
    public class Tests
    {
        [Test]
        public void Generate()
        {
            var snowflake = new SnowflakeInstance();

            Print(snowflake);
        }

        [Test]
        public void SmallEpoch()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.MinValue);

            Print(snowflake);
        }

        [Test]
        public void LargeEpoch()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow);

            Print(snowflake);
        }

        [Test]
        public void Hour()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddHours(-1));

            Print(snowflake);
        }

        [Test]
        public void Day()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddDays(-1));

            Print(snowflake);
        }

        [Test]
        public void Month()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddMonths(-1));

            Print(snowflake);
        }

        [Test]
        public void Year()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddYears(-1));

            Print(snowflake);
        }

        [Test]
        public void In20Years()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddYears(-20));

            Print(snowflake);
        }

        [Test]
        public void In50Years()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddYears(-50));

            Print(snowflake);
        }

        [Test]
        public void In100Years()
        {
            var snowflake = new SnowflakeInstance(null, DateTimeOffset.UtcNow.AddYears(-100));

            Print(snowflake);
        }

        static void Print(ISnowflake snowflake)
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 20; i++)
            {
                Console.WriteLine(snowflake.New);

                var sleep = i * 50 - stopwatch.Elapsed.TotalMilliseconds;

                if (sleep > 0)
                    Thread.Sleep(TimeSpan.FromMilliseconds(sleep));
            }
        }
    }
}