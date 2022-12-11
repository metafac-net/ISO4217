using FluentAssertions;
using Multiformats.Base;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace MetaFac.Standard.RFC4648.Tests
{
    public enum CodecLibrary
    {
        System_Buffers_Text,
        Multiformats_Base__,
        MetaFac1_Standard__,
        MetaFac2_Standard__,
        // todo
        //MetaFac3_Standard__
    }

    public class CodecReferenceTests
    {
        private static string SystemBuffersTextEncoder(ReadOnlySpan<byte> source, Span<byte> target)
        {
            var status = Base64.EncodeToUtf8(source, target, out var bytesConsumed, out int bytesWritten, true);
            status.Should().Be(OperationStatus.Done);
#if NET5_0_OR_GREATER
            string result = Encoding.UTF8.GetString(target.Slice(0, bytesWritten));
#else
            string result = Encoding.UTF8.GetString(target.Slice(0, bytesWritten).ToArray());
#endif
            return result.Trim('=').Replace('+', '-').Replace('/', '_');
        }

        [Theory]
        [InlineData(CodecLibrary.System_Buffers_Text, 0, 0, 0, "AAAA")]
        [InlineData(CodecLibrary.Multiformats_Base__, 0, 0, 0, "uAAAA")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 0, 0, 0, "AAAA")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 0, 0, 0, "AAAA")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 0, 0, 0, "AAAA")]
        [InlineData(CodecLibrary.System_Buffers_Text, 1, 1, 1, "AQEB")]
        [InlineData(CodecLibrary.Multiformats_Base__, 1, 1, 1, "uAQEB")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 1, 1, 1, "AQEB")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 1, 1, 1, "AQEB")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 1, 1, 1, "AQEB")]
        [InlineData(CodecLibrary.System_Buffers_Text, 254, 254, 254, "_v7-")]
        [InlineData(CodecLibrary.Multiformats_Base__, 254, 254, 254, "u_v7-")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 254, 254, 254, "_v7-")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 254, 254, 254, "_v7-")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 254, 254, 254, "_v7-")]
        [InlineData(CodecLibrary.System_Buffers_Text, 255, 255, 255, "____")]
        [InlineData(CodecLibrary.Multiformats_Base__, 255, 255, 255, "u____")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 255, 255, 255, "____")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 255, 255, 255, "____")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 255, 255, 255, "____")]
        public void Base64UrlComparison3(CodecLibrary codec, byte inp0, byte inp1, byte inp2, string expected)
        {
            byte[] input = new byte[] { inp0, inp1, inp2 };

            switch (codec)
            {
                case CodecLibrary.System_Buffers_Text:
                    {
                        Span<byte> target = stackalloc byte[4];
                        string result = SystemBuffersTextEncoder(input, target);
                        result.Should().Be(expected);
                    }
                    break;
                case CodecLibrary.Multiformats_Base__:
                    {
                        string result = Multibase.Encode(MultibaseEncoding.Base64Url, input);
                        result.Should().Be(expected);
                    }
                    break;
                case CodecLibrary.MetaFac1_Standard__:
                    {
                        string result = Base64UrlEncoder.EncodeBase64Url(input);
                        result.Should().Be(expected);
                    }
                    break;
                case CodecLibrary.MetaFac2_Standard__:
                    {
                        string result = Base64UrlEncoder2.EncodeBase64Url(input);
                        result.Should().Be(expected);
                    }
                    break;
                //case CodecLibrary.MetaFac3_Standard__:
                //    {
                //        string result = Base64UrlEncoder3.EncodeBase64Url(input);
                //        result.Should().Be(expected);
                //    }
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codec), codec, null);
            }
        }

        [Theory]
        [InlineData(CodecLibrary.System_Buffers_Text, 0, 0, "AAA")]
        [InlineData(CodecLibrary.Multiformats_Base__, 0, 0, "uAAA")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 0, 0, "AAA")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 0, 0, "AAA")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 0, 0, "AAA")]
        [InlineData(CodecLibrary.System_Buffers_Text, 1, 1, "AQE")]
        [InlineData(CodecLibrary.Multiformats_Base__, 1, 1, "uAQE")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 1, 1, "AQE")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 1, 1, "AQE")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 1, 1, "AQE")]
        [InlineData(CodecLibrary.System_Buffers_Text, 254, 254, "_v4")]
        [InlineData(CodecLibrary.Multiformats_Base__, 254, 254, "u_v4")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 254, 254, "_v4")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 254, 254, "_v4")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 254, 254, "_v4")]
        [InlineData(CodecLibrary.System_Buffers_Text, 255, 255, "__8")]
        [InlineData(CodecLibrary.Multiformats_Base__, 255, 255, "u__8")]
        [InlineData(CodecLibrary.MetaFac1_Standard__, 255, 255, "__8")]
        [InlineData(CodecLibrary.MetaFac2_Standard__, 255, 255, "__8")]
        //[InlineData(CodecLibrary.MetaFac3_Standard__, 255, 255, "__8")]
        public void Base64UrlComparison2(CodecLibrary codec, byte inp0, byte inp1, string expected)
        {
            byte[] input = new byte[] { inp0, inp1 };

            switch (codec)
            {
                case CodecLibrary.System_Buffers_Text:
                    {
                        Span<byte> target = stackalloc byte[4];
                        string result = SystemBuffersTextEncoder(input, target);
                        result.Should().Be(expected);
                    }
                    break;
                case CodecLibrary.Multiformats_Base__:
                    {
                        string result = Multibase.Encode(MultibaseEncoding.Base64Url, input);
                        result.Should().Be(expected);
                    }
                    break;
                case CodecLibrary.MetaFac1_Standard__:
                    {
                        string result = Base64UrlEncoder.EncodeBase64Url(input);
                        result.Should().Be(expected);
                    }
                    break;
                case CodecLibrary.MetaFac2_Standard__:
                    {
                        string result = Base64UrlEncoder2.EncodeBase64Url(input);
                        result.Should().Be(expected);
                    }
                    break;
                //case CodecLibrary.MetaFac3_Standard__:
                //    {
                //        string result = Base64UrlEncoder3.EncodeBase64Url(input);
                //        result.Should().Be(expected);
                //    }
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codec), codec, null);
            }
        }
    }
}