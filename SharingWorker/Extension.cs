﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharingWorker
{
    public static class StringExtension
    {
        public static IEnumerable<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(str))
                yield break;

            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");

            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);
                if (index == -1)
                    break;
                yield return index;
            }
        }

        public static string RemoveEnd(this string input, string suffixToRemove)
        {
            if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }

        public static string ToMd5Hash(this string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }

    public static class FileSizeExtension
    {
        public static string ToFileSize(this long l)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", l);
        }

        class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter)) return this;
                return null;
            }

            private const string fileSizeFormat = "fs";
            private const Decimal OneKiloByte = 1024M;
            private const Decimal OneMegaByte = OneKiloByte * 1024M;
            private const Decimal OneGigaByte = OneMegaByte * 1024M;

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (format == null || !format.StartsWith(fileSizeFormat))
                {
                    return defaultFormat(format, arg, formatProvider);
                }

                if (arg is string)
                {
                    return defaultFormat(format, arg, formatProvider);
                }

                Decimal size;

                try
                {
                    size = Convert.ToDecimal(arg);
                }
                catch (InvalidCastException)
                {
                    return defaultFormat(format, arg, formatProvider);
                }

                string suffix;
                if (size > OneGigaByte)
                {
                    size /= OneGigaByte;
                    suffix = "GB";
                }
                else if (size > OneMegaByte)
                {
                    size /= OneMegaByte;
                    suffix = "MB";
                }
                else if (size > OneKiloByte)
                {
                    size /= OneKiloByte;
                    suffix = "kB";
                }
                else
                {
                    suffix = " B";
                }

                string precision = format.Substring(2);
                if (String.IsNullOrEmpty(precision)) precision = "2";
                return String.Format("{0:N" + precision + "}{1}", size, suffix);

            }

            private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
            {
                IFormattable formattableArg = arg as IFormattable;
                if (formattableArg != null)
                {
                    return formattableArg.ToString(format, formatProvider);
                }
                return arg.ToString();
            }
        }
    }

    public static class EnumerableExtensions
    {
        private static readonly Random Rnd = new Random();

        public static T Random<T>(this IEnumerable<T> input)
        {
            var enumerable = input as IList<T> ?? input.ToList();
            return enumerable.ElementAt(Rnd.Next(enumerable.Count()));
        }
    }
}
