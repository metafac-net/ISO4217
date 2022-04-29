using System;

namespace MetaFac.Standards.ISO4217
{
    /// <summary>
    /// Information record for a currency.
    /// </summary>
    public class Iso4217Info
    {
        /// <summary>
        /// The ISO 4217 3-digit number for the currency. Eg. 840 or 156. For more information refer to: https://en.wikipedia.org/wiki/ISO_4217
        /// </summary>
        public short Num { get; }
        /// <summary>
        /// The ISO 4217 3-character code for the currency. Eg. USD or CNY.
        /// </summary>
        public string Code { get; }
        /// <summary>
        /// The full name of the currency. Eg. US Dollar or Yuan Renminbi.
        /// </summary>
        public string FullName { get; }
        /// <summary>
        /// The name of the country using this currency.
        /// </summary>
        public string Country { get; }
        /// <summary>
        /// The number of digits after the decimal separator. For example, USD = 2, JPY = 0.
        /// </summary>
        public int? Decimals { get; }
        /// <summary>
        /// The string for formatting an amount with the correct number of digits after the decimal separator.
        /// </summary>
        public string FormatString { get; }

        private readonly int _minorUnitsPerMajorUnit;

        /// <summary>
        /// Constructs a currency information record.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="code"></param>
        /// <param name="fullName"></param>
        /// <param name="country"></param>
        /// <param name="decimals"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Iso4217Info(short num, string code, string fullName, string country, int? decimals)
        {
            Num = num;
            Code = code;
            FullName = fullName;
            Country = country;
            Decimals = decimals;
            switch (decimals)
            {
                case null:
                case 0:
                    _minorUnitsPerMajorUnit = 1;
                    FormatString = "N0";
                    break;
                case 1:
                    _minorUnitsPerMajorUnit = 10;
                    FormatString = "N1";
                    break;
                case 2:
                    _minorUnitsPerMajorUnit = 100;
                    FormatString = "N2";
                    break;
                case 3:
                    _minorUnitsPerMajorUnit = 1000;
                    FormatString = "N3";
                    break;
                case 4:
                    _minorUnitsPerMajorUnit = 10000;
                    FormatString = "N4";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(decimals), decimals, null);
            }
        }

        /// <summary>
        /// Converts a minor currency unit (eg. cents) to the major currency unit (eg. dollars).
        /// Note that some currencies do not differentiate major and minor currency units
        /// (eg. Japanese Yen).
        /// </summary>
        /// <param name="minorAmount"></param>
        /// <returns></returns>
        public double ToMajorUnits(long minorAmount)
        {
            return 1.0D * minorAmount / _minorUnitsPerMajorUnit;
        }

        /// <summary>
        /// Converts a major currency unit (eg. dollars) to the minor currency unit (eg. cents),
        /// and returns the rounding error.Note that some currencies do not differentiate major 
        /// and minor currency units (eg. Japanese Yen).
        /// </summary>
        /// <param name="majorAmount"></param>
        /// <param name="rounding"></param>
        /// <returns></returns>
        public long ToMinorUnits(double majorAmount, out double rounding)
        {
            long result = Convert.ToInt64(majorAmount * _minorUnitsPerMajorUnit);
            rounding = 1.0D * result / _minorUnitsPerMajorUnit - result;
            return result;
        }
    }
}
