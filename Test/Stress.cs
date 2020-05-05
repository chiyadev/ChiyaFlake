using System;
using System.Collections.Concurrent;
using System.Threading;
using ChiyaFlake;
using NUnit.Framework;

namespace Test
{
    [Parallelizable(ParallelScope.All)]
    public class Stress
    {
        const double _duration = 5;

        [Test]
        public void StressOneInstance()
        {
            var snowflake = new SnowflakeInstance();

            Run(() => snowflake.Timestamp);
        }

        [Test]
        public void StressMultiThread()
            => Run(() => Snowflake.Timestamp);

        static void Run(Func<long> timestamp)
        {
            var end = DateTime.Now.AddSeconds(_duration);

            var threads = new Thread[Environment.ProcessorCount];

            var cache = new ConcurrentDictionary<long, object>();

            var duplicate = 0L;

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    DateTime now;

                    do
                    {
                        now = DateTime.Now;

                        var x = timestamp();

                        if (!cache.TryAdd(x, null))
                        {
                            duplicate = x;
                            end       = now;
                            break;
                        }

                        // prune if cache gets too large
                        if (cache.Count > 10000000)
                            cache.Clear();
                    }
                    while (now < end);
                });

                threads[i].Start();
            }

            foreach (var thread in threads)
                thread.Join();

            Assert.Zero(duplicate, $"Duplicate snowflake: {duplicate}");
        }
    }
}