﻿//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

using EsentLib.Api;

namespace EsentLib
{
    /// <summary>Static utility methods.</summary>
    internal static class Util
    {
        /// <summary>Compare two byte arrays to see if they have the same content.</summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        /// <param name="offset">The offset to start comparing at.</param>
        /// <param name="count">The number of bytes to compare.</param>
        /// <returns>True if the arrays are equal, false otherwise.</returns>
        public static bool ArrayEqual(IList<byte> a, IList<byte> b, int offset, int count)
        {
            if (null == a || null == b) { return ReferenceEquals(a, b); }
            for (int i = 0; i < count; ++i) {
                if (a[offset + i] != b[offset + i]) {return false; }
            }
            return true;
        }

        /// <summary>Compares two objects with ContentEquals. If both are null, there are considered
        /// equal.</summary>
        /// <typeparam name="T">A type that implements IContentEquatable.</typeparam>
        /// <param name="left">First object to compare.</param>
        /// <param name="right">Second object to compare.</param>
        /// <returns>Whether the two objects are equal.</returns>
        public static bool ObjectContentEquals<T>(T left, T right)
            where T : class, IContentEquatable<T>
        {
            return (null == left || null == right)
                ? ReferenceEquals(left, right)
                : left.ContentEquals(right);
        }

        /// <summary>Compares two objects with ContentEquals. If both are null, there are
        /// considered equal.</summary>
        /// <typeparam name="T">A type that implements IContentEquatable.</typeparam>
        /// <param name="left">First object to compare.</param>
        /// <param name="right">Second object to compare.</param>
        /// <param name="length">The number of entries to compare.</param>
        /// <returns>Whether the two objects are equal.</returns>
        public static bool ArrayObjectContentEquals<T>(T[] left, T[] right, int length)
            where T : class, IContentEquatable<T>
        {
            if (null == left || null == right) { return ReferenceEquals(left, right); }
            for (int i = 0; i < length; ++i) {
                if (!ObjectContentEquals(left[i], right[i])) { return false; }
            }
            // All the individual members are equal, all of the elements of the arrays are
            // equal, so they must be equal!
            return true;
        }

        /// <summary>Clone an array of objects.</summary>
        /// <typeparam name="T">The type of object in the array.</typeparam>
        /// <param name="value">The values to clone.</param>
        /// <returns>A clone of the values.</returns>
        public static T[] DeepCloneArray<T>(T[] value) where T : class, IDeepCloneable<T>
        {
            if (null == value) { return null; }
            T[] clone = new T[value.Length];
            for (int i = 0; i < clone.Length; ++i) {
                clone[i] = (null == value[i]) ? null : value[i].DeepClone();
            }
            return clone;
        }

        /// <summary>Given a list of hash codes calculate a hash of the hashes.</summary>
        /// <param name="hashes">The sub hash codes.</param>
        /// <returns>A hash of the hash codes.</returns>
        public static int CalculateHashCode(IEnumerable<int> hashes)
        {
            int hash = 0;
            foreach (int h in hashes) {
                hash ^= h;
                unchecked { hash *= 33; }
            }
            return hash;
        }

        /// <summary>
        /// Add a trailing directory separator character to the string.
        /// </summary>
        /// <param name="dir">The directory.</param>
        /// <returns>The directory with a separator character added (if necesary).</returns>
        public static string AddTrailingDirectorySeparator(string dir)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                var sepChars = new[] { LibraryHelpers.DirectorySeparatorChar, LibraryHelpers.AltDirectorySeparatorChar };
                return string.Concat(dir.TrimEnd(sepChars), LibraryHelpers.DirectorySeparatorChar);
            }

            return dir;
        }

        /// <summary>
        /// Converts a unicode string to a null-terminated Ascii byte array.
        /// </summary>
        /// <param name="value">The unicode string to be converted.</param>
        /// <returns>The byte array with a null-terminated Ascii representation of the given string.</returns>
        public static byte[] ConvertToNullTerminatedAsciiByteArray(string value)
        {
            if (value == null)
            {
                return null;
            }

            byte[] output = new byte[value.Length + 1];

            LibraryHelpers.EncodingASCII.GetBytes(value, 0, value.Length, output, 0);
            output[output.Length - 1] = (byte)0;

            return output;
        }

        /// <summary>
        /// Converts a unicode string to a null-terminated Unicode byte array.
        /// </summary>
        /// <param name="value">The unicode string to be converted.</param>
        /// <returns>The byte array with a null-terminated Unicode representation of the given string.</returns>
        public static byte[] ConvertToNullTerminatedUnicodeByteArray(string value)
        {
            if (value == null)
            {
                return null;
            }

            int byteArrayLength = Encoding.Unicode.GetByteCount(value); 

            byte[] output = new byte[byteArrayLength + 2];

            Encoding.Unicode.GetBytes(value, 0, value.Length, output, 0);
            output[output.Length - 2] = (byte)0;
            output[output.Length - 1] = (byte)0;

            return output;
        }
    }
}
