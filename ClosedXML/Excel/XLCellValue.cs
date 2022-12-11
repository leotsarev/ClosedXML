﻿using System;
using System.Diagnostics;
using System.Globalization;
using ClosedXML.Excel.CalcEngine;
using ClosedXML.Extensions;

namespace ClosedXML.Excel
{
    /// <summary>
    /// A value of a single cell. It contains a value a specific <see cref="Type"/>.
    /// Structure provides following group of methods:
    /// <list type="bullet">
    ///   <item><c>Is*</c> properties to check type (<see cref="IsNumber"/>, <see cref="IsBlank"/>...)</item>
    ///   <item><c>Get*</c> methods that return the stored value or throw <see cref="InvalidCastException"/> for incorrect type.</item>
    ///   <item>Explicit operators to convert <c>XLCellValue</c> to a concrete type. It is an equivalent of <c>Get*</c> methods.</item>
    ///   <item><c>TryConvert</c> methods to try to get value of a specific type, even if the value is of a different type.</item>
    /// </list>
    /// </summary>
    [DebuggerDisplay("{Type} {_text != null ? (object)_text : (object)_value}")]
    public readonly struct XLCellValue : IEquatable<XLCellValue>, IEquatable<Blank>, IEquatable<Boolean>, IEquatable<Double>, IEquatable<String>, IEquatable<XLError>, IEquatable<DateTime>, IEquatable<TimeSpan>, IEquatable<int>
    {
        private readonly double _value;
        private readonly string _text;

        private XLCellValue(Blank value) : this()
        {
            Type = XLDataType.Blank;
        }

        private XLCellValue(bool logical) : this()
        {
            Type = XLDataType.Boolean;
            _value = logical ? 1d : 0d;
        }

        private XLCellValue(double number) : this()
        {
            if (Double.IsNaN(number) || Double.IsInfinity(number))
                throw new ArgumentException("Value can't be NaN or infinity.", nameof(number));

            Type = XLDataType.Number;
            _value = number;
        }

        private XLCellValue(string text) : this()
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length > 32767)
                throw new ArgumentOutOfRangeException(nameof(text), "Cells can hold a maximum of 32,767 characters.");

            Type = XLDataType.Text;
            _text = text;
        }

        private XLCellValue(XLError error) : this()
        {
            if (error < XLError.CellReference || error > XLError.NumberInvalid)
                throw new ArgumentOutOfRangeException(nameof(error));

            Type = XLDataType.Error;
            _value = (double)error;
        }

        private XLCellValue(DateTime dateTime) : this()
        {
            Type = XLDataType.DateTime;
            _value = dateTime.ToSerialDateTime();
        }

        private XLCellValue(TimeSpan timeSpan) : this()
        {
            Type = XLDataType.TimeSpan;
            _value = timeSpan.ToSerialDateTime();
        }

        private XLCellValue(XLDataType type, double value) : this()
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
                throw new ArgumentException("Value can't be NaN or infinity.", nameof(value));
            Type = type;
            _value = value;
        }

        /// <summary>
        /// Type of the value.
        /// </summary>
        public XLDataType Type { get; }

        /// <summary>
        /// Is the type of value <c>Blank</c>?
        /// </summary>
        public bool IsBlank => Type == XLDataType.Blank;

        /// <summary>
        /// Is the type of value <see cref="XLDataType.Boolean"/>?
        /// </summary>
        public bool IsBoolean => Type == XLDataType.Boolean;

        /// <summary>
        /// Is the type of value <see cref="XLDataType.Number"/>?
        /// </summary>
        public bool IsNumber => Type == XLDataType.Number;

        /// <summary>
        /// Is the type of value <see cref="XLDataType.Text"/>?
        /// </summary>
        public bool IsText => Type == XLDataType.Text;

        /// <summary>
        /// Is the type of value <see cref="XLDataType.Error"/>?
        /// </summary>
        public bool IsError => Type == XLDataType.Error;

        /// <summary>
        /// Is the type of value <see cref="XLDataType.DateTime"/>?
        /// </summary>
        public bool IsDateTime => Type == XLDataType.DateTime;

        /// <summary>
        /// Is the type of value <see cref="XLDataType.TimeSpan"/>?
        /// </summary>
        public bool IsTimeSpan => Type == XLDataType.TimeSpan;

        /// <summary>
        /// Is the value <see cref="XLDataType.Number"/> or <see cref="XLDataType.DateTime"/> or <see cref="XLDataType.TimeSpan"/>.
        /// </summary>
        public bool IsUnifiedNumber => IsNumber || IsDateTime || IsTimeSpan;

        public static implicit operator XLCellValue(Blank blank) => new(blank);
        public static implicit operator XLCellValue(bool logical) => new(logical);
        public static implicit operator XLCellValue(double number) => new(number);
        public static implicit operator XLCellValue(string text) => new(text);
        public static implicit operator XLCellValue(XLError error) => new(error);
        public static implicit operator XLCellValue(DateTime dateTime) => new(dateTime);
        public static implicit operator XLCellValue(TimeSpan timeSpan) => new(timeSpan);

        /// <inheritdoc cref="GetBlank"/>
        public static explicit operator Blank(XLCellValue value) => value.GetBlank();

        /// <inheritdoc cref="GetBoolean"/>
        public static explicit operator Boolean(XLCellValue value) => value.GetBoolean();

        /// <inheritdoc cref="GetNumber"/>
        public static explicit operator Double(XLCellValue value) => value.GetNumber();

        /// <inheritdoc cref="GetText"/>
        public static explicit operator String(XLCellValue value) => value.GetText();

        /// <inheritdoc cref="GetError"/>
        public static explicit operator XLError(XLCellValue value) => value.GetError();

        /// <inheritdoc cref="GetDateTime"/>
        public static implicit operator DateTime(XLCellValue value) => value.GetDateTime();

        /// <inheritdoc cref="GetTimeSpan"/>
        public static implicit operator TimeSpan(XLCellValue value) => value.GetTimeSpan();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.Blank"/>,
        /// return <see cref="Blank.Value"/>, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public Blank GetBlank() => IsBlank ? Blank.Value : throw new InvalidCastException();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.Boolean"/>,
        /// return logical, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public Boolean GetBoolean() => IsBoolean ? _value != 0d : throw new InvalidCastException();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.Number"/>,
        /// return number, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public Double GetNumber() => IsNumber ? _value : throw new InvalidCastException();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.Text"/>,
        /// return text, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public String GetText() => IsText ? _text : throw new InvalidCastException();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.Error"/>,
        /// return error, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public XLError GetError() => IsError ? (XLError)_value : throw new InvalidCastException();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.DateTime"/>,
        /// return date time, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public DateTime GetDateTime() => IsDateTime ? _value.ToSerialDateTime() : throw new InvalidCastException();

        /// <summary>
        /// If the value is of type <see cref="XLDataType.TimeSpan"/>,
        /// return time span, otherwise throw <see cref="InvalidCastException"/>.
        /// </summary>
        public TimeSpan GetTimeSpan() => IsTimeSpan ? _value.ToSerialTimeSpan() : throw new InvalidCastException();

        internal static XLCellValue FromSerialDateTime(double serialDateTime) => new(XLDataType.DateTime, serialDateTime);

        internal static XLCellValue FromSerialTimeSpan(double serialTimeSpan) => new(XLDataType.TimeSpan, serialTimeSpan);

        /// <summary>
        /// Get a number, either directly from type number or from serialized time span
        /// or serialized date time.
        /// </summary>
        /// <exception cref="InvalidCastException">If type is not <see cref="XLDataType.Number"/> or
        /// <see cref="XLDataType.DateTime"/> or <see cref="XLDataType.TimeSpan"/>.</exception>
        public double GetUnifiedNumber()
        {
            if (IsUnifiedNumber)
                return _value;

            throw new InvalidCastException("Value is not a number.");
        }

        internal Object ToObject()
        {
            return Type switch
            {
                XLDataType.Blank => null,
                XLDataType.Boolean => GetBoolean(),
                XLDataType.Number => GetNumber(),
                XLDataType.Text => GetText(),
                XLDataType.Error => GetError(),
                XLDataType.DateTime => GetDateTime(),
                XLDataType.TimeSpan => GetTimeSpan(),
                _ => throw new InvalidCastException()
            };
        }

        /// <summary>
        /// Return text representation of a value in current culture.
        /// </summary>
        public override string ToString() => ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Return text representation of a value in the specified culture.
        /// </summary>
        public string ToString(CultureInfo culture) =>
            Type switch
            {
                XLDataType.Blank => string.Empty,
                XLDataType.Boolean => GetBoolean() ? "TRUE" : "FALSE",
                XLDataType.Number => _value.ToString(culture),
                XLDataType.Text => _text,
                XLDataType.Error => GetError().ToDisplayString(),
                XLDataType.DateTime => GetDateTime().ToString(culture),
                XLDataType.TimeSpan => GetTimeSpan().ToExcelString(culture),
                _ => throw new InvalidOperationException()
            };

        public bool Equals(XLCellValue other)
        {
            return Type == other.Type && _value.Equals(other._value) && _text == other._text;
        }

        public bool Equals(Blank other)
        {
            return IsBlank;
        }

        public bool Equals(bool other)
        {
            return IsBoolean && GetBoolean() == other;
        }

        public bool Equals(double other)
        {
            return IsNumber && _value.Equals(other);
        }

        public bool Equals(string other)
        {
            return IsText && _text == other;
        }

        public bool Equals(XLError other)
        {
            return IsError && GetError() == other;
        }

        public bool Equals(DateTime other)
        {
            return IsDateTime && GetDateTime() == other;
        }

        public bool Equals(TimeSpan other)
        {
            return IsTimeSpan && GetTimeSpan() == other;
        }

        public bool Equals(int other)
        {
            return Equals((double)other);
        }

        public override bool Equals(object obj)
        {
            return obj is XLCellValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _value.GetHashCode();
                hashCode = (hashCode * 397) ^ (_text != null ? _text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }

        /// <summary>
        /// Get a value, if it is a <see cref="XLDataType.Text"/>.
        /// </summary>
        /// <returns>True if value was retrieved, false otherwise.</returns>
        public bool TryGetText(out string value)
        {
            if (IsText)
            {
                value = _text;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Try to convert the value to a <see cref="XLDataType.Blank"/> and return it.
        /// Method succeeds, when value is 
        /// <list type="bullet">
        ///   <item>Type <see cref="XLDataType.Blank"/>.</item>
        ///   <item>Type <see cref="XLDataType.Text"/> and the text is empty.</item>
        /// </list>
        /// </summary>
        public bool TryConvert(out Blank value)
        {
            var isBlankLike = IsBlank || (IsText && GetText().Length == 0);
            if (isBlankLike)
            {
                value = Blank.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Try to convert the value to a <see cref="XLDataType.Boolean"/> and return it.
        /// Method succeeds, when value is 
        /// <list type="bullet">
        ///   <item>Type <see cref="XLDataType.Boolean"/>.</item>
        ///   <item>Type <see cref="XLDataType.Number"/>, then the value of <c>0</c> means <c>false</c> and any other value is <c>true</c>.</item>
        ///   <item>Type <see cref="XLDataType.Text"/> and the value is <c>TRUE</c> or <c>FALSE</c> (case insensitive). Note that calc engine
        ///   generally doesn't coerce text to logical (e.g. arithmetic operations), though it happens in most function arguments (e.g.
        ///   <c>IF</c> or <c>AND</c>).</item>
        /// </list>
        /// </summary>
        public bool TryConvert(out Boolean value)
        {
            switch (Type)
            {
                case XLDataType.Boolean:
                    value = GetBoolean();
                    return true;
                case XLDataType.Number:
                    value = GetNumber() != 0;
                    return true;
                case XLDataType.Text when String.Equals(GetText(), "TRUE", StringComparison.OrdinalIgnoreCase):
                    value = true;
                    return true;
                case XLDataType.Text when String.Equals(GetText(), "FALSE", StringComparison.OrdinalIgnoreCase):
                    value = false;
                    return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Try to convert the value to a <see cref="XLDataType.Number"/> and return it.
        /// <list type="bullet">
        ///   <item>Double value - return the value.</item>
        ///   <item>Boolean value - return the <c>0</c> for <c>TRUE</c> and <c>1</c> for <c>FALSE</c>.</item>
        ///   <item>Text value - use the <c>VALUE</c> semantic to convert a text to a number.</item>
        ///   <item>DateTime value - return the serial date time number.</item>
        ///   <item>TimeSpan value - return the serial time span value.</item>
        /// </list>
        /// </summary>
        /// <remarks>Note that the coercion is current culture specific (e.g. decimal separators can differ).</remarks>
        /// <param name="value">The converted value. Result is never <c>infinity</c> or <c>NaN</c>.</param>
        /// <param name="culture">The culture used to convert the value for texts.</param>
        public bool TryConvert(out Double value, CultureInfo culture)
        {
            switch (Type)
            {
                case XLDataType.Number:
                case XLDataType.DateTime:
                case XLDataType.TimeSpan:
                    value = GetUnifiedNumber();
                    return true;
                case XLDataType.Boolean:
                    value = GetBoolean() ? 1 : 0;
                    return true;
                case XLDataType.Text:
                    {
                        var coercionResult = ScalarValue.TextToNumber(GetText(), culture);
                        if (coercionResult.TryPickT0(out var number, out _))
                        {
                            value = number;
                            return true;
                        }

                        break;
                    }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Try to convert the value to a <see cref="DateTime"/> and return it.
        /// Method succeeds, when value is 
        /// <list type="bullet">
        ///   <item>Type <see cref="XLDataType.DateTime"/>.</item>
        ///   <item>Type <see cref="XLDataType.Number"/> and when the number is interpreted is a serial date time, it falls within the DateTime range.</item>
        ///   <item>Type <see cref="XLDataType.TimeSpan"/> and when the number is interpreted is a serial date time, it falls within the DateTime range.</item>
        /// </list>
        /// </summary>
        public bool TryConvert(out DateTime value)
        {
            if (IsUnifiedNumber)
            {
                var number = GetUnifiedNumber();
                if (number.IsValidOADateNumber())
                {
                    value = number.ToSerialDateTime();
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Try to convert the value to a <see cref="TimeSpan"/> and return it.
        /// Method succeeds, when value is 
        /// <list type="bullet">
        ///   <item>Type <see cref="XLDataType.TimeSpan"/>.</item>
        ///   <item>Type <see cref="XLDataType.Number"/>, the number is interpreted is a time span date time.</item>
        ///   <item>Type <see cref="XLDataType.Text"/> and it can be parsed as a time span (accepts even hours over 24 hours).</item>
        /// </list>
        /// </summary>
        /// <param name="value">The converted value.</param>
        /// <param name="culture">The culture used to get time and decimal separators.</param>
        public bool TryConvert(out TimeSpan value, CultureInfo culture)
        {
            if (IsTimeSpan)
            {
                value = GetTimeSpan();
                return true;
            }

            if (IsNumber)
            {
                value = GetUnifiedNumber().ToSerialTimeSpan();
                return true;
            }

            if (IsText && TimeSpanParser.TryParseTime(GetText(), culture, out var ts))
            {
                value = ts;
                return true;
            }

            value = default;
            return false;
        }
    }
}