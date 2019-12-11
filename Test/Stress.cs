using System;
using System.Collections.Concurrent;
using System.Threading;
using ChiyaFlake;
using NUnit.Framework;

namespace Test
{
    public class Stress
    {
        const double _duration = 2;

        [Test]
        public void StressOneInstance()
        {
            var snowflake = new SnowflakeInstance();

            Run(() => snowflake.New);
        }

        [Test]
        public void StressMultiThread()
        {
            var id        = 0;
            var snowflake = new ThreadLocal<ISnowflake>(() => new SnowflakeInstance((byte) (Interlocked.Increment(ref id) % 64)));

            Run(() => snowflake.Value.New);
        }

        static void Run(Func<string> generate)
        {
            var end = DateTime.Now.AddSeconds(_duration);

            var threads = new Thread[Environment.ProcessorCount];

            var cache = new ConcurrentDictionary<string, object>();

            var duplicate = null as string;

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    DateTime now;

                    do
                    {
                        now = DateTime.Now;

                        var x = generate();

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

            Assert.Null(duplicate, $"Duplicate snowflake: {duplicate}");
        }
    }
}