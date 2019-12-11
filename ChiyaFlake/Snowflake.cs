using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace ChiyaFlake
{
    public static class Snowflake
    {
        static readonly ThreadLocal<ISnowflake> _snowflake = new ThreadLocal<ISnowflake>(() => new SnowflakeInstance());

        /// <summary>
        /// Generates a short time-based string using <see cref="SnowflakeInstance"/>.
        /// </summary>
        public static string New => _snowflake.Value.New;
    }

    public interface ISnowflake
    {
        string New { get; }
    }

    public class SnowflakeInstance : ISnowflake
    {
        readonly long _id;

        readonly TimeSpan _watchOffset;
        readonly Stopwatch _watch = Stopwatch.StartNew();

        /// <summary>
        /// Initializes a new <see cref="Snowflake"/> instance with generator ID and epoch time.
        /// </summary>
        /// <param name="id">6-bit generator ID (0 to 63).</param>
        /// <param name="epoch">Epoch time.</param>
        public SnowflakeInstance(byte? id = null, DateTimeOffset? epoch = null)
        {
            id    = id ?? (byte) (RandomByte() % 64);
            epoch = epoch ?? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (id.Value >= 64)
                throw new ArgumentOutOfRangeException(nameof(id), id, $"{nameof(id)} must be in the range [0, 63].");

            _id          = id.Value;
            _watchOffset = DateTime.UtcNow - epoch.Value.ToUniversalTime();
        }

        static byte RandomByte()
        {
            using (var rand = new RNGCryptoServiceProvider())
            {
                var buffer = new byte[1];
                rand.GetBytes(buffer);

                return buffer[0];
            }
        }

        long _lastTimestamp;

        /// <summary>
        /// Timestamp with millisecond accuracy from year 2000 which avoids conflicts.
        /// </summary>
        long Timestamp
        {
            get
            {
                long original, current;

                do
                {
                    original = _lastTimestamp;

                    var now = (long) (_watchOffset + _watch.Elapsed).TotalMilliseconds;

                    current = Math.Max(now, original + 1);
                }
                while (Interlocked.CompareExchange(ref _lastTimestamp, current, original) > original);

                current = (current << 6) | _id;

                return current;
            }
        }

        /// <summary>
        /// Generates a short time-based snowflake string.
        /// </summary>
        public string New
        {
            get
            {
                var buffer = BitConverter.GetBytes(Timestamp);
                Array.Reverse(buffer);

                var offset = 0;

                while (buffer[offset] == 0)
                    offset++;

                return Convert.ToBase64String(buffer, offset, buffer.Length - offset)
                              .TrimEnd('=')
                              .Replace("/", "_")
                              .Replace("+", "-");
            }
        }
    }
}