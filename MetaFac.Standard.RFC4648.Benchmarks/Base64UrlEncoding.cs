using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers.Text;
using System.Text;
using Multiformats.Base;
using BenchmarkDotNet.Order;

namespace MetaFac.Standard.RFC4648.Benchmarks
{
    //[SimpleJob(RuntimeMoniker.Net48)]
    //[SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class Base64UrlEncoding
    {
        private const string plainText = "123 quick brown foxes jump over the 456 lazy dogs.";

        private readonly byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        [Benchmark(Baseline = true)]
        public int System_Base64Std_SpanOnly()
        {
            ReadOnlySpan<byte> source = plainBytes.AsSpan(); //.Slice(0, Length);
            Span<byte> target = stackalloc byte[100];
            var status = Base64.EncodeToUtf8(source, target, out var bytesConsumed, out var bytesWritten, true);
            return bytesWritten;
        }

        [Benchmark]
        public int System_Base64Url_SpanOnly()
        {
            ReadOnlySpan<byte> source = plainBytes.AsSpan(); //.Slice(0, Length);
            Span<byte> target = stackalloc byte[100];
            var status = Base64.EncodeToUtf8(source, target, out var bytesConsumed, out var bytesWritten, true);
            // now replace +/ chars with -_ and trim trailing =
            for (int i = 0; i < bytesWritten; i++)
            {
                byte b = target[i];
                switch ((char)b)
                {
                    case '+':
                        target[i] = (byte)('-');
                        break;
                    case '/':
                        target[i] = (byte)('_');
                        break;
                    case '=':
                        bytesWritten = i;
                        return bytesWritten;
                    default:
                        break;
                }
            }
            return bytesWritten;
        }

        [Benchmark]
        public int MetaFac1_Base64Url_SpanOnly()
        {
            ReadOnlySpan<byte> source = plainBytes.AsSpan(); //.Slice(0, Length);
            Span<byte> target = stackalloc byte[100];
            Base64UrlEncoder.EncodeToUtf8(source, target, out var bytesWritten);
            return bytesWritten;
        }

        //[Benchmark]
        //public int MetaFac2_Base64Url_SpanOnly()
        //{
        //    ReadOnlySpan<byte> source = plainBytes.AsSpan(); //.Slice(0, Length);
        //    Span<byte> target = stackalloc byte[100];
        //    Base64UrlEncoder2.EncodeToUtf8(source, target, out var bytesWritten);
        //    return bytesWritten;
        //}

        //[Benchmark]
        //public int MetaFac3_Base64Url_SpanOnly()
        //{
        //    ReadOnlySpan<byte> source = plainBytes.AsSpan(); //.Slice(0, Length);
        //    Span<byte> target = stackalloc byte[100];
        //    Base64UrlEncoder3.EncodeToUtf8(source, target, out int bytesWritten);
        //    return bytesWritten;
        //}

    }
}