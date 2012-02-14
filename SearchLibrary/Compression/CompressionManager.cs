/// Copyright 2011 Timothy James
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
namespace IndexLibrary.Compression
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// A static compression class that allows easy compression of strings and byte arrays
    /// </summary>
    [CLSCompliant(true)]
    public static class CompressionManager
    {
        #region Methods

        /// <summary>
        /// Compresses a byte array using the specified compression, and returns a compressed byte array.
        /// </summary>
        /// <param name="input">The input to compress.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <returns>A byte array representing the compressed input string</returns>
        public static byte[] CompressToBytes(string input, CompressionType compressionType)
        {
            return CompressToBytes(input, compressionType, Encoding.Default);
        }

        /// <summary>
        /// Compresses a byte array using the specified compression, and returns a compressed byte array.
        /// </summary>
        /// <param name="input">The input to compress.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <param name="textEncoding">The encoding to apply to the input string.</param>
        /// <returns>A byte array representing the compressed input string</returns>
        public static byte[] CompressToBytes(string input, CompressionType compressionType, Encoding textEncoding)
        {
            if (string.IsNullOrEmpty(input))
                return new byte[0];
            if (textEncoding == null)
                throw new ArgumentNullException("textEncoding", "textEncoding cannot be null");
            byte[] bytes = textEncoding.GetBytes(input);
            return CompressToBytes(bytes, 0, bytes.Length, compressionType);
        }

        /// <summary>
        /// Compresses a byte array using the specified compression, and returns a compressed byte array.
        /// </summary>
        /// <param name="bytes">The input bytes to compress.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <returns>A byte array representing the compressed byte array</returns>
        public static byte[] CompressToBytes(byte[] bytes, CompressionType compressionType)
        {
            return CompressToBytes(bytes, 0, bytes.Length, compressionType);
        }

        /// <summary>
        /// Compresses a byte array using the specified compression, and returns a compressed byte array.
        /// </summary>
        /// <param name="bytes">The input bytes to compress.</param>
        /// <param name="offset">The amount of offset to apply to the byte array before beginning compression.</param>
        /// <param name="length">The length of bytes to compress in the byte array.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <returns>A byte array representing the compressed byte array</returns>
        public static byte[] CompressToBytes(byte[] bytes, int offset, int length, CompressionType compressionType)
        {
            if (bytes == null || bytes.Length == 0)
                return new byte[0];
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset cannot be less than zero");
            if (length > bytes.Length)
                throw new ArgumentOutOfRangeException("length", "length cannot be greater than bytes.length");
            if (length + offset > bytes.Length)
                throw new ArgumentOutOfRangeException("length", "length + offset cannot be greater than bytes.length");

            using (MemoryStream memoryStream = new MemoryStream()) {
                switch (compressionType) {
                    case CompressionType.BZip:
                        using (ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream stream = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(memoryStream)) {
                            stream.Write(bytes, offset, length);
                        }
                        break;
                    case CompressionType.GZip:
                        using (ICSharpCode.SharpZipLib.GZip.GZipOutputStream stream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(memoryStream)) {
                            stream.Write(bytes, offset, length);
                        }
                        break;

                    // case CompressionType.Tar:
                    //    using (ICSharpCode.SharpZipLib.Tar.TarOutputStream stream = new ICSharpCode.SharpZipLib.Tar.TarOutputStream(memoryStream)) {
                    //        ICSharpCode.SharpZipLib.Tar.TarEntry entry = ICSharpCode.SharpZipLib.Tar.TarEntry.CreateTarEntry("TarEntry");
                    //        entry.Size = length;
                    //        stream.PutNextEntry(entry);
                    //        stream.Write(bytes, offset, length);
                    //        stream.IsStreamOwner = false;
                    //        stream.CloseEntry();
                    //    }
                    //    break;
                    case CompressionType.Zip:
                        using (ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream stream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(memoryStream)) {
                            stream.Write(bytes, offset, length);
                        }
                        break;
                    default:
                        return new byte[0];
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Compresses a string using the specified compression, and returns the results as a string.
        /// </summary>
        /// <param name="input">The input to compress.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>A string representing the compressed input string</returns>
        public static string CompressToString(string input, CompressionType compressionType, Encoding outputEncoding)
        {
            if (outputEncoding == null)
                throw new ArgumentNullException("outputEncoding", "outputEncoding cannot be null");
            return outputEncoding.GetString(CompressToBytes(input, compressionType));
        }

        /// <summary>
        /// Compresses a string using the specified compression, and returns the results as a string.
        /// </summary>
        /// <param name="input">The input to compress.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <param name="inputEncoding">The encoding to apply to the input string.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>A string representing the compressed input string</returns>
        public static string CompressToString(string input, CompressionType compressionType, Encoding inputEncoding, Encoding outputEncoding)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            if (inputEncoding == null)
                throw new ArgumentNullException("textEncoding", "textEncoding cannot be null");
            if (outputEncoding == null)
                throw new ArgumentNullException("outputEncoding", "outputEncoding cannot be null");
            return outputEncoding.GetString(CompressToBytes(input, compressionType, inputEncoding));
        }

        /// <summary>
        /// Compresses a byte array using the specified compression, and returns the result as a string.
        /// </summary>
        /// <param name="bytes">The input bytes to compress.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>A string representing the compressed byte array</returns>
        public static string CompressToString(byte[] bytes, CompressionType compressionType, Encoding outputEncoding)
        {
            if (outputEncoding == null)
                throw new ArgumentNullException("outputEncoding", "outputEncoding cannot be null");
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            return outputEncoding.GetString(CompressToBytes(bytes, 0, 0, compressionType));
        }

        /// <summary>
        /// Compresses a byte array using the specified compression, and returns the result as a string.
        /// </summary>
        /// <param name="bytes">The input bytes to compress.</param>
        /// <param name="offset">The amount of offset to apply to the byte array before beginning compression.</param>
        /// <param name="length">The length of bytes to compress in the byte array.</param>
        /// <param name="compressionType">Type of the compression to apply.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>A string representing the compressed byte array</returns>
        public static string CompressToString(byte[] bytes, int offset, int length, CompressionType compressionType, Encoding outputEncoding)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset cannot be less than zero");
            if (length > bytes.Length)
                throw new ArgumentOutOfRangeException("length", "length cannot be greater than bytes.length");
            if (length + offset > bytes.Length)
                throw new ArgumentOutOfRangeException("length", "length + offset cannot be greater than bytes.length");
            if (outputEncoding == null)
                throw new ArgumentNullException("outputEncoding", "outputEncoding cannot be null");

            return outputEncoding.GetString(CompressToBytes(bytes, offset, length, compressionType));
        }

        /// <summary>
        /// Decompresses the specified text, using the specified compression type, input and output encoding
        /// </summary>
        /// <param name="encodedText">The compressed input string.</param>
        /// <param name="compressionType">Type of the compression applied to the input string.</param>
        /// <param name="inputEncoding">The encoding of the input string.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>Returns a string representing the uncompressed verison of the input</returns>
        public static string Decompress(string encodedText, CompressionType compressionType, Encoding inputEncoding, Encoding outputEncoding)
        {
            if (string.IsNullOrEmpty(encodedText))
                return string.Empty;

            var bytes = inputEncoding.GetBytes(encodedText);
            return Decompress(bytes, 0, bytes.Length, compressionType, outputEncoding);
        }

        /// <summary>
        /// Decompresses the specified bytes, using the specified compression type and output encoding
        /// </summary>
        /// <param name="bytes">The bytes to decompress.</param>
        /// <param name="compressionType">Type of the compression applied to the input string.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>Returns a string representing the uncompressed verison of the input</returns>
        public static string Decompress(byte[] bytes, CompressionType compressionType, Encoding outputEncoding)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            return Decompress(bytes, 0, bytes.Length, compressionType, outputEncoding);
        }

        /// <summary>
        /// Decompresses the specified bytes, using the specified compression type and output encoding
        /// </summary>
        /// <param name="bytes">The bytes to decompress.</param>
        /// <param name="offset">The amount of offset to apply to the byte array before beginning decompression.</param>
        /// <param name="length">The length of bytes to decompress in the byte array.</param>
        /// <param name="compressionType">Type of the compression applied to the input string.</param>
        /// <param name="outputEncoding">The output encoding to apply.</param>
        /// <returns>Returns a string representing the uncompressed verison of the input</returns>
        public static string Decompress(byte[] bytes, int offset, int length, CompressionType compressionType, Encoding outputEncoding)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset cannot be less than zero");
            if (length > bytes.Length)
                throw new ArgumentOutOfRangeException("length", "length cannot be greater than bytes.length");
            if (length + offset > bytes.Length)
                throw new ArgumentOutOfRangeException("length", "length + offset cannot be greater than bytes.length");

            using (MemoryStream memoryStream = new MemoryStream(bytes)) {
                Stream stream = null;
                switch (compressionType) {
                    case CompressionType.BZip:
                        stream = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(memoryStream);
                        break;
                    case CompressionType.GZip:
                        stream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(memoryStream);
                        break;

                    // case CompressionType.Tar:
                    //    stream = new ICSharpCode.SharpZipLib.Tar.TarInputStream(memoryStream);
                    //    break;
                    case CompressionType.Zip:
                        stream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(memoryStream);
                        break;
                }

                if (stream != null) {
                    var decoder = outputEncoding.GetDecoder();
                    StringBuilder builder = new StringBuilder();
                    byte[] buffer = new byte[2048];
                    char[] chars = new char[2048];

                    while (true) {
                        int size = stream.Read(buffer, 0, buffer.Length);
                        if (size == 0)
                            break;
                        int totalChars = decoder.GetChars(buffer, 0, size, chars, 0);
                        builder.Append(chars, 0, totalChars);
                    }

                    stream.Dispose();

                    return builder.ToString();
                }
            }

            return string.Empty;
        }

        #endregion Methods
    }
}