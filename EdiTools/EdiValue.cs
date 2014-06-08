using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EdiTools
{
    /// <summary>
    /// Represents an EDI object containing a single value, and provides methods for parsing and formatting EDI data.
    /// </summary>
    public abstract class EdiValue
    {
        /// <summary>
        /// Gets or sets the value of this EDI object.
        /// </summary>
        public abstract string Value { get; set; }

        /// <summary>
        /// Gets a DateTime containing the value of this EDI object parsed as a date.
        /// </summary>
        public DateTime DateValue
        {
            get
            {
                string stripped = Regex.Replace(Value, "[^0-9]", string.Empty);
                string format;
                if (stripped.Length == 6)
                    format = "yyMMdd";
                else if (stripped.Length == 8)
                    format = "yyyyMMdd";
                else
                    throw new FormatException();
                return DateTime.ParseExact(stripped, format, null);
            }
        }

        /// <summary>
        /// Gets a DateTime containing the value of this EDI object parsed as a time.
        /// </summary>
        public DateTime TimeValue
        {
            get
            {
                string stripped = Regex.Replace(Value, "[^0-9]", string.Empty);
                string format;
                if (stripped.Length == 4)
                    format = "HHmm";
                else if (stripped.Length >= 6)
                    format = "HHmmss".PadRight(stripped.Length, 'f');
                else
                    throw new FormatException();
                return DateTime.ParseExact(stripped, format, null);
            }
        }

        /// <summary>
        /// Gets a Decimal containing the value of this EDI object parsed as a real.
        /// </summary>
        public decimal RealValue
        {
            get
            {
                string stripped = Regex.Replace(Value, "[^-0-9.]", ".");
                return decimal.Parse(stripped);
            }
        }

        /// <summary>
        /// Gets an ISO 8601-formatted date containing the value of this EDI object.
        /// </summary>
        public string IsoDate
        {
            get { return DateValue.ToString("yyyy-MM-dd"); }
        }

        /// <summary>
        /// Gets an ISO 8601-formatted time containing the value of this EDI object.
        /// </summary>
        public string IsoTime
        {
            get
            {
                string stripped = Regex.Replace(Value, "[^0-9]", string.Empty);
                if (stripped.Length == 4)
                    return DateTime.ParseExact(stripped, "HHmm", null).ToString("HH:mm");
                if (stripped.Length == 6)
                    return DateTime.ParseExact(stripped, "HHmmss", null).ToString("HH:mm:ss");
                if (stripped.Length > 6)
                {
                    string fractionFormat = string.Empty.PadLeft(stripped.Length - 6, 'f');
                    DateTime time = DateTime.ParseExact(stripped, "HHmmss" + fractionFormat, null);
                    return time.ToString("HH:mm:ss." + fractionFormat);
                }
                throw new FormatException();
            }
        }

        /// <summary>
        /// Formats a DateTime as an EDI date of a specified length.
        /// </summary>
        /// <param name="length">The desired length of the returned string.</param>
        /// <param name="value">A DateTime containing the date to format.</param>
        /// <returns>A string containing an EDI-formatted date.</returns>
        public static string Date(int length, DateTime value)
        {
            switch (length)
            {
                case 6:
                    return value.ToString("yyMMdd");
                case 8:
                    return value.ToString("yyyyMMdd");
                default:
                    throw new ArgumentOutOfRangeException("length");
            }
        }

        /// <summary>
        /// Formats a DateTime as an EDI time of a specified length.
        /// </summary>
        /// <param name="length">The desired length of the returned string.</param>
        /// <param name="value">A DateTime containing the time to format.</param>
        /// <returns>A string containing an EDI-formatted time.</returns>
        public static string Time(int length, DateTime value)
        {
            if (length == 4)
                return value.ToString("HHmm");
            if (length >= 6)
            {
                string format = "HHmmss".PadRight(length, 'f');
                return value.ToString(format);
            }
            throw new ArgumentOutOfRangeException("length");
        }

        /// <summary>
        /// Formats a Decimal as an EDI numeric with a specified number of decimal places.
        /// </summary>
        /// <param name="decimals">The number of decimal places in the returned string.</param>
        /// <param name="value">A number to format.</param>
        /// <returns>A string containing an EDI-formatted numeric.</returns>
        public static string Numeric(int decimals, decimal value)
        {
            string formatted = Math.Abs(value).ToString("f" + decimals).Replace(".", string.Empty).TrimStart('0');
            if (formatted == string.Empty)
                return "0";
            if (value < 0)
                return "-" + formatted;
            return formatted;
        }

        /// <summary>
        /// Formats a Decimal as an EDI real.
        /// </summary>
        /// <param name="value">A number to format.</param>
        /// <returns>A string containing an EDI-formatted real.</returns>
        public static string Real(decimal value)
        {
            string formatted = value.ToString(CultureInfo.InvariantCulture);
            if (formatted.Contains("."))
                formatted = formatted.TrimEnd('0').TrimEnd('.');
            return formatted;
        }

        /// <summary>
        /// Gets a Decimal containing the value of this EDI object parsed as a numeric.
        /// </summary>
        /// <param name="decimals">The number of decimal places in the value.</param>
        /// <returns>The Decimal value of this EDI object.</returns>
        public decimal NumericValue(int decimals)
        {
            string stripped = Regex.Replace(Value, "[^-0-9]", string.Empty);
            string paddedToDecimals = stripped.PadLeft(decimals + 1, '0');
            int decimalIndex = paddedToDecimals.Length - decimals;
            string withDecimal = paddedToDecimals.Substring(0, decimalIndex) + "." +
                                 paddedToDecimals.Substring(decimalIndex);
            return decimal.Parse(withDecimal);
        }
    }
}