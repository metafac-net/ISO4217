using System.IO;
using System.Runtime.CompilerServices;
using System;
using System.Text;

namespace MetaFac.Standard.RFC4648
{
    /// <summary>
    /// Base 64 URL codec.
    /// </summary>
    public static class Base64UrlEncoder
    {
        private static readonly char[] EncodeMap =
            {
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P',
                'Q','R','S','T','U','V','W','X','Y','Z','a','b','c','d','e','f',
                'g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v',
                'w','x','y','z','0','1','2','3','4','5','6','7','8','9','-','_'
            };

        private static int[] BuildDecodeMap()
        {
            int[] result = new int[128];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = -1;
            }
            for (int i = 0; i < EncodeMap.Length; i++)
            {
                char ch = EncodeMap[i];
                int pos = ch & 0x7F;
                result[pos] = i;
            }
            return result;
        }

        private static readonly int[] DecodeMap = BuildDecodeMap();

        /// <summary>
        /// Returns the encoding alphabet.
        /// </summary>
        /// <returns></returns>
        public static ReadOnlySpan<char> GetEncodeMap() => EncodeMap;

        /// <summary>
        /// Returns the decoding map.
        /// </summary>
        /// <returns></returns>
        public static ReadOnlySpan<int> GetDecodeMap() => DecodeMap;

        /// <summary>
        /// Encodes a 24-bit (8 bytes) tuple to 4 characaters.
        /// </summary>
        /// <param name="byte0"></param>
        /// <param name="byte1"></param>
        /// <param name="byte2"></param>
        /// <param name="encodeMap"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (char, char, char, char) EncodeTuple(int byte0, int byte1, int byte2, ReadOnlySpan<char> encodeMap)
        {
            // convert 3 * 8-bit to 4 * 6-bit
            //    orig0       orig1       orig2
            //  01001101  | 01100001  | 01101110
            // 01 00 11 01|01 10 00 01|01 10 11 10
            // 01 00 11+01|01 10+00 01|01+10 11 10
            // 01 00 11+01 01 10+00 01 01+10 11 10
            //  010011 + 010110 + 000101 + 101110
            //   hex0     hex1     hex2     hex3

            int hex0 = (byte0 >> 2) & 0x3F;
            int hex1 = ((byte0 & 0x03) << 4) | ((byte1 >> 4) & 0x0F);
            int hex2 = ((byte1 & 0x0F) << 2) | ((byte2 >> 6) & 0x03);
            int hex3 = (byte2 >> 0) & 0x3F;

            return (encodeMap[hex0], encodeMap[hex1], encodeMap[hex2], encodeMap[hex3]);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidInputChar(char ch)
        {
            throw new InvalidDataException($"Input contained invalid char: '{ch}'");
        }

        /// <summary>
        /// Decodes a 4 character (24 bits) tuple to 3 octets.
        /// </summary>
        /// <param name="ch0"></param>
        /// <param name="ch1"></param>
        /// <param name="ch2"></param>
        /// <param name="ch3"></param>
        /// <param name="decodeMap"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int, int, int) DecodeTuple(char ch0, char ch1, char ch2, char ch3, ReadOnlySpan<int> decodeMap)
        {
            int hex0 = decodeMap[ch0]; if (hex0 < 0) ThrowInvalidInputChar(ch0);
            int hex1 = decodeMap[ch1]; if (hex1 < 0) ThrowInvalidInputChar(ch1);
            int hex2 = decodeMap[ch2]; if (hex2 < 0) ThrowInvalidInputChar(ch2);
            int hex3 = decodeMap[ch3]; if (hex3 < 0) ThrowInvalidInputChar(ch3);

            int b0 = (hex0 << 2) | (hex1 >> 4);
            int b1 = ((hex1 & 0x0F) << 4) | (hex2 >> 2);
            int b2 = ((hex2 & 0x03) << 6) | (hex3);

            return (b0, b1, b2);
        }

        /// <summary>
        /// Encodes source span of bytes to an output span of characters.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="charsWritten"></param>
        public static void EncodeToUtf8(ReadOnlySpan<byte> source, Span<byte> target, out int charsWritten)
        {
            ReadOnlySpan<char> encodeMap = EncodeMap;

            charsWritten = 0;

            while (source.Length >= 3)
            {
                // encode 3 * 8-bits to 4 * 6-bits
                (char ch0, char ch1, char ch2, char ch3) = EncodeTuple(source[0], source[1], source[2], encodeMap);

                target[charsWritten++] = (byte)ch0;
                target[charsWritten++] = (byte)ch1;
                target[charsWritten++] = (byte)ch2;
                target[charsWritten++] = (byte)ch3;

                source = source.Slice(3);
            }

            if (source.Length == 2)
            {
                // encode 2 bytes
                (char ch0, char ch1, char ch2, char _) = EncodeTuple(source[0], source[1], 0, encodeMap);

                target[charsWritten++] = (byte)ch0;
                target[charsWritten++] = (byte)ch1;
                target[charsWritten++] = (byte)ch2;
            }
            else if (source.Length == 1)
            {
                // encode 1 bytes
                (char ch0, char ch1, char _, char _) = EncodeTuple(source[0], 0, 0, encodeMap);

                target[charsWritten++] = (byte)ch0;
                target[charsWritten++] = (byte)ch1;
            }

        }

        /// <summary>
        /// Encodes source span of bytes to an output string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string EncodeBase64Url(ReadOnlySpan<byte> source)
        {
            int targetSize = ((source.Length + 3) / 3) * 4;
            Span<byte> target = stackalloc byte[targetSize];
            EncodeToUtf8(source, target, out int bytesWritten);
#if NET5_0_OR_GREATER
            string result = Encoding.UTF8.GetString(target.Slice(0, bytesWritten));
#else
            string result = Encoding.UTF8.GetString(target.Slice(0, bytesWritten).ToArray());
#endif
            return result;
        }

        /// <summary>
        /// Decodes an input string to a byte array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] DecodeBase64Url(string input)
        {
            Span<byte> target = stackalloc byte[input.Length];
#if NET5_0_OR_GREATER
            int bytesDecoded = DecodeBase64Url(input, target);
#else
            int bytesDecoded = DecodeBase64Url(input.AsSpan(), target);
#endif
            return target.Slice(0, bytesDecoded).ToArray();
        }

        /// <summary>
        /// Decodes source span of characters to an output span of bytes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int DecodeBase64Url(ReadOnlySpan<char> input, Span<byte> output)
        {
            if (input.Length % 4 == 1)
            {
                // should not happen!
                throw new ArgumentException($"Invalid input length: {input.Length}", nameof(input));
            }

            ReadOnlySpan<int> decodeMap = DecodeMap;

            int bytesDecoded = 0;

            while (input.Length >= 4)
            {
                // decode 4 * 6-bits to 3 * 8-bits
                (int out0, int out1, int out2) = Base64UrlEncoder.DecodeTuple(input[0], input[1], input[2], input[3], decodeMap);

                output[bytesDecoded++] = (byte)out0;
                output[bytesDecoded++] = (byte)out1;
                output[bytesDecoded++] = (byte)out2;

                input = input.Slice(4);
            }

            if (input.Length == 3)
            {
                // decode 3 chars to 2 bytes
                (int out0, int out1, int _) = Base64UrlEncoder.DecodeTuple(input[0], input[1], input[2], 'A', decodeMap);

                output[bytesDecoded++] = (byte)out0;
                output[bytesDecoded++] = (byte)out1;
            }
            else if (input.Length == 2)
            {
                // decode 2 chars to 1 byte
                (int out0, int _, int _) = Base64UrlEncoder.DecodeTuple(input[0], input[1], 'A', 'A', decodeMap);

                output[bytesDecoded++] = (byte)out0;
            }
            else if (input.Length == 1)
            {
                // cannot happen!
            }

            return bytesDecoded;
        }
    }
}