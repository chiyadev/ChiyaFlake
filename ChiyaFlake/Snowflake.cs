using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace ChiyaFlake
{
    /// <summary><a href="https://github.com/chiyadev/ChiyaFlake">ChiyaFlake</a> static snowflake generator.</summary>
    /// <remarks>This class uses <see cref="SnowflakeInstance"/> bound to the caller's thread.</remarks>
    public static class Snowflake
    {
        static readonly ThreadLocal<ISnowflake> _snowflake = new ThreadLocal<ISnowflake>(() => new SnowflakeInstance());

        /// <inheritdoc cref="ISnowflake.Timestamp"/>
        public static long Timestamp => _snowflake.Value.Timestamp;

        /// <inheritdoc cref="ISnowflake.New"/>
        public static string New => _snowflake.Value.New;
    }

    /// <summary><a href="https://github.com/chiyadev/ChiyaFlake">ChiyaFlake</a> snowflake generator interface.</summary>
    public interface ISnowflake
    {
        /// <summary>Gets the timestamp value used for snowflake generation.</summary>
        long Timestamp { get; }

        /// <summary>Gets a short base64-encoded URL-safe snowflake string generated using <see cref="Timestamp"/>.</summary>
        string New { get; }
    }

    /// <summary><a href="https://github.com/chiyadev/ChiyaFlake">ChiyaFlake</a> snowflake generator instance.</summary>
    public sealed class SnowflakeInstance : ISnowflake
    {
        readonly long _id;
        readonly TimeSpan _offset;
        readonly Stopwatch _watch = Stopwatch.StartNew();

        /// <summary>Initializes a new <see cref="Snowflake"/> instance with generator ID and epoch time.</summary>
        /// <param name="id">6-bit generator ID (0 to 63), or null to use a cryptographically random ID.</param>
        /// <param name="epoch">Epoch time, or null to use 2000/01/01.</param>
        public SnowflakeInstance(byte? id = null, DateTimeOffset? epoch = null)
        {
            id    = id ?? (byte) (RandomByte() % 64);
            epoch = epoch ?? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (id.Value >= 64) throw new ArgumentOutOfRangeException(nameof(id), id, $"{nameof(id)} must be in the range [0, 63].");

            _id     = id.Value;
            _offset = DateTime.UtcNow - epoch.Value.ToUniversalTime();
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

        /// <inheritdoc cref="ISnowflake.Timestamp"/>
        public long Timestamp
        {
            get
            {
                long original, current;

                do
                {
                    var now = (long) (_offset + _watch.Elapsed).TotalMilliseconds;

                    original = _lastTimestamp;
                    current  = Math.Max(now, original + 1);
                }
                while (Interlocked.CompareExchange(ref _lastTimestamp, current, original) > original);

                return (current << 6) | _id;
            }
        }

        /// <inheritdoc cref="ISnowflake.New"/>
        public string New
        {
            get
            {
                var buffer = BitConverter.GetBytes(Timestamp);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);

                var offset = Array.FindIndex(buffer, x => x != 0);

                return Convert.ToBase64String(buffer, offset, buffer.Length - offset).TrimEnd('=').Replace("/", "_").Replace("+", "-");
            }
        }
    }
}