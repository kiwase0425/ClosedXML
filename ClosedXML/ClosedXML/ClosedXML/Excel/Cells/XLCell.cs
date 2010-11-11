﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ClosedXML.Excel
{
    internal class XLCell : IXLCell
    {
        XLWorksheet worksheet;
        public XLCell(IXLAddress address, IXLStyle defaultStyle, XLWorksheet worksheet)
        {
            this.Address = address;
            Style = defaultStyle;
            if (Style == null) Style = worksheet.Style;
            this.worksheet = worksheet;
        }

        public IXLAddress Address { get; private set; }
        public String InnerText
        {
            get { return cellValue; }
        }

        public T GetValue<T>() 
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        public String GetString()
        {
            return GetValue<String>();
        }
        public Double GetDouble()
        {
            return GetValue<Double>();
        }
        public Boolean GetBoolean()
        {
            return GetValue<Boolean>();
        }
        public DateTime GetDateTime()
        {
            return GetValue<DateTime>();
        }
        public String GetFormattedString()
        {
            if (dataType == XLCellValues.Boolean)
            {
                return (cellValue != "0").ToString();
            }
            else if (dataType == XLCellValues.DateTime || IsDateFormat())
            {
                String format = GetFormat();
                return DateTime.FromOADate(Double.Parse(cellValue)).ToString(format);
            }
            else if (dataType == XLCellValues.Number)
            {
                String format = GetFormat();
                return Double.Parse(cellValue).ToString(format);
            }
            else
            {
                return cellValue;
            }
        }

        private bool IsDateFormat()
        {
            return (dataType == XLCellValues.Number 
                && String.IsNullOrWhiteSpace(Style.NumberFormat.Format)
                && ((Style.NumberFormat.NumberFormatId >= 14
                    && Style.NumberFormat.NumberFormatId <= 22)
                || (Style.NumberFormat.NumberFormatId >= 45
                    && Style.NumberFormat.NumberFormatId <= 47)));
        }

        private String GetFormat()
        {
            String format;
            if (String.IsNullOrWhiteSpace(Style.NumberFormat.Format))
            {
                var formatCodes = GetFormatCodes();
                format = formatCodes[Style.NumberFormat.NumberFormatId];
            }
            else
            {
                format = Style.NumberFormat.Format;
            }
            return format;
        }

        private Boolean initialized = false;
        private String cellValue = String.Empty;
        public Object Value
        {
            get
            {
                var fA1 = FormulaA1;
                if (!String.IsNullOrWhiteSpace(fA1))
                    return fA1;

                if (dataType == XLCellValues.Boolean)
                {
                    return cellValue != "0";
                }
                else if (dataType == XLCellValues.DateTime)
                {
                    return DateTime.FromOADate(Double.Parse(cellValue));
                }
                else if (dataType == XLCellValues.Number)
                {
                    return Double.Parse(cellValue);
                }
                else
                {
                    return cellValue;
                }
            }
            set
            {
                FormulaA1 = String.Empty;
                String val = value.ToString();
                Double dTest;
                DateTime dtTest;
                Boolean bTest;
                if (initialized)
                {
                    if (dataType == XLCellValues.Boolean)
                    {
                        if (Boolean.TryParse(val, out bTest))
                            val = bTest ? "1" : "0";
                        else if (!(val == "1" || val == "0"))
                            throw new ArgumentException("'" + val + "' is not a Boolean type.");
                    }
                    else if (dataType == XLCellValues.DateTime)
                    {
                        if (DateTime.TryParse(val, out dtTest))
                        {

                            val = dtTest.ToOADate().ToString();
                        }
                        else if (!Double.TryParse(val, out dTest))
                        {
                            throw new ArgumentException("'" + val + "' is not a DateTime type.");
                        }

                        if (Style.NumberFormat.Format == String.Empty && Style.NumberFormat.NumberFormatId == 0)
                            Style.NumberFormat.NumberFormatId = 14;
                    }
                    else if (dataType == XLCellValues.Number)
                    {
                        if (!Double.TryParse(val, out dTest))
                            throw new ArgumentException("'" + val + "' is not a Numeric type.");
                        
                    }
                }
                else
                {
                    if (val.Length > 0 && val.Substring(0, 1) == "'")
                    {
                        val = val.Substring(1, val.Length - 1);
                        dataType = XLCellValues.Text;
                    }
                    else if (Double.TryParse(val, out dTest))
                    {
                        dataType = XLCellValues.Number;
                    }
                    else if (DateTime.TryParse(val, out dtTest))
                    {
                        dataType = XLCellValues.DateTime;
                        Style.NumberFormat.NumberFormatId = 14;
                        val = dtTest.ToOADate().ToString();
                    }
                    else if (Boolean.TryParse(val, out bTest))
                    {
                        dataType = XLCellValues.Boolean;
                        val = bTest ? "1" : "0";
                    }
                    else
                    {
                        dataType = XLCellValues.Text;
                    }
                }
                cellValue = val;
            }
        }

        #region IXLStylized Members

        private IXLStyle style;
        public IXLStyle Style
        {
            get
            {
                return style;
            }
            set
            {
                style = new XLStyle(null, value);
            }
        }

        public IEnumerable<IXLStyle> Styles
        {
            get
            {
                UpdatingStyle = true;
                yield return style;
                UpdatingStyle = false;
            }
        }

        public Boolean UpdatingStyle { get; set; }

        #endregion

        private XLCellValues dataType;
        public XLCellValues DataType
        {
            get
            {
                return dataType;
            }
            set
            {
                initialized = true;
                if (cellValue.Length > 0)
                {
                    if (value == XLCellValues.Boolean)
                    {
                        Boolean bTest;
                        if (Boolean.TryParse(cellValue, out bTest))
                            cellValue = bTest ? "1" : "0";
                        else
                            cellValue = cellValue == "0" || String.IsNullOrEmpty(cellValue) ? "0" : "1";
                    }
                    else if (value == XLCellValues.DateTime)
                    {
                        DateTime dtTest;
                        Double dblTest;
                        if (DateTime.TryParse(cellValue, out dtTest))
                        {
                            cellValue = dtTest.ToOADate().ToString();
                        }
                        else if (Double.TryParse(cellValue, out dblTest))
                        {
                            cellValue = dblTest.ToString();
                        }
                        else
                        {
                            throw new ArgumentException("Cannot set data type to DateTime because '" + cellValue + "' is not recognized as a date.");
                        }

                        if (Style.NumberFormat.Format == String.Empty && Style.NumberFormat.NumberFormatId == 0)
                            Style.NumberFormat.NumberFormatId = 14;
                    }
                    else if (value == XLCellValues.Number)
                    {
                        cellValue = Double.Parse(cellValue).ToString();
                        //if (Style.NumberFormat.Format == String.Empty )
                        //    Style.NumberFormat.NumberFormatId = 0;
                    }
                    else
                    {
                        if (dataType == XLCellValues.Boolean)
                        {
                            cellValue = (cellValue != "0").ToString();
                        }
                        else if (dataType == XLCellValues.Number)
                        {
                            cellValue = Double.Parse(cellValue).ToString(Style.NumberFormat.Format);
                        }
                        else if (dataType == XLCellValues.DateTime)
                        {
                            cellValue = DateTime.FromOADate(Double.Parse(cellValue)).ToString(Style.NumberFormat.Format);
                        }
                    }
                }
                dataType = value;
            }
        }

        public void Clear()
        {
            worksheet.Range(Address, Address).Clear();
        }
        public void Delete(XLShiftDeletedCells shiftDeleteCells)
        {
            worksheet.Range(Address, Address).Delete(shiftDeleteCells);
        }

        private static Dictionary<Int32, String> formatCodes;
        private static Dictionary<Int32, String> GetFormatCodes()
        {
            if (formatCodes == null)
            {
                var fCodes = new Dictionary<Int32, String>();
                fCodes.Add(0, "");
                fCodes.Add(1, "0");
                fCodes.Add(2, "0.00");
                fCodes.Add(3, "#,##0");
                fCodes.Add(4, "#,##0.00");
                fCodes.Add(9, "0%");
                fCodes.Add(10, "0.00%");
                fCodes.Add(11, "0.00E+00");
                fCodes.Add(12, "# ?/?");
                fCodes.Add(13, "# ??/??");
                fCodes.Add(14, "MM-dd-yy");
                fCodes.Add(15, "d-MMM-yy");
                fCodes.Add(16, "d-MMM");
                fCodes.Add(17, "MMM-yy");
                fCodes.Add(18, "h:mm AM/PM");
                fCodes.Add(19, "h:mm:ss AM/PM");
                fCodes.Add(20, "h:mm");
                fCodes.Add(21, "h:mm:ss");
                fCodes.Add(22, "M/d/yy h:mm");
                fCodes.Add(37, "#,##0 ;(#,##0)");
                fCodes.Add(38, "#,##0 ;[Red](#,##0)");
                fCodes.Add(39, "#,##0.00;(#,##0.00)");
                fCodes.Add(40, "#,##0.00;[Red](#,##0.00)");
                fCodes.Add(45, "mm:ss");
                fCodes.Add(46, "[h]:mm:ss");
                fCodes.Add(47, "mmss.0");
                fCodes.Add(48, "##0.0E+0");
                fCodes.Add(49, "@");
                formatCodes = fCodes;
            }
            return formatCodes;
        }

        private String formulaA1;
        public String FormulaA1
        {
            get { return formulaA1; }
            set 
            { 
                formulaA1 = value;
                formulaR1C1 = String.Empty;
            }
        }

        private String formulaR1C1;
        public String FormulaR1C1
        {
            get 
            {
                if (String.IsNullOrWhiteSpace(formulaR1C1))
                    formulaR1C1 = GetFormulaR1C1(FormulaA1);

                return formulaR1C1; 
            }
            set 
            { 
                formulaR1C1 = value;
                FormulaA1 = GetFormulaA1(value);
            }
        }

        private String GetFormulaR1C1(String value)
        {
            return GetFormula(value, FormulaConversionType.A1toR1C1);
        }

        private String GetFormulaA1(String value)
        {
            return GetFormula(value, FormulaConversionType.R1C1toA1);
        }

        private enum FormulaConversionType { A1toR1C1, R1C1toA1 };
        private static Regex a1Regex = new Regex(@"\$?[a-zA-Z]{1,3}\$?\d+");
        private static Regex r1c1Regex = new Regex(@"[Rr]\[?-?\d*\]?[Cc]\[?-?\d*\]?");
        private String GetFormula(String value, FormulaConversionType conversionType)
        {
            if (String.IsNullOrWhiteSpace(value))
                return String.Empty;

            Regex regex = conversionType == FormulaConversionType.A1toR1C1 ? a1Regex : r1c1Regex;

            var sb = new StringBuilder();
            var lastIndex = 0;
            var matches = regex.Matches(value);
            foreach (var i in Enumerable.Range(0, matches.Count))
            {
                var m = matches[i];
                sb.Append(value.Substring(lastIndex, m.Index - lastIndex));
                
                if (conversionType == FormulaConversionType.A1toR1C1)
                    sb.Append(GetR1C1Address(m.Value));
                else
                    sb.Append(GetA1Address(m.Value));

                lastIndex = m.Index + m.Value.Length;
            }
            if (lastIndex < value.Length)
                sb.Append(value.Substring(lastIndex));

            var retVal = sb.ToString();
            return retVal;
        }

        private String GetA1Address(String r1c1Address)
        {
            var addressToUse = r1c1Address.ToUpper();

            var rowPart = addressToUse.Substring(0, addressToUse.IndexOf("C"));
            String rowToReturn;
            if (rowPart == "R")
            {
                rowToReturn = Address.RowNumber.ToString();
            }
            else
            {
                var bIndex = rowPart.IndexOf("[");
                if (bIndex >= 0)
                    rowToReturn = (Address.RowNumber + Int32.Parse(rowPart.Substring(bIndex + 1, rowPart.Length - bIndex - 1))).ToString();
                else
                    rowToReturn = "$" + rowPart.Substring(1);
            }

            var cIndex = addressToUse.IndexOf("C");
            String columnToReturn;
            if (cIndex == addressToUse.Length - 1)
            {
                columnToReturn = Address.ColumnLetter;
            }
            else
            {
                var columnPart = addressToUse.Substring(cIndex);
                var bIndex = columnPart.IndexOf("[");
                if (bIndex >= 0)
                    columnToReturn = XLAddress.GetColumnLetterFromNumber(
                        Address.ColumnNumber + Int32.Parse(columnPart.Substring(bIndex + 1, columnPart.Length - bIndex - 2)));
                else
                    columnToReturn = "$" + XLAddress.GetColumnLetterFromNumber(Int32.Parse(columnPart.Substring(1)));
            }

            var retAddress = columnToReturn + rowToReturn;
            return retAddress;
        }

        private String GetR1C1Address(String a1Address)
        {
            var address = new XLAddress(a1Address);

            String rowPart;
            var rowDiff = address.RowNumber - Address.RowNumber;
            if (rowDiff != 0 || address.FixedRow)
            {
                if (address.FixedRow)
                    rowPart = String.Format("R{0}", address.RowNumber);
                else
                    rowPart = String.Format("R[{0}]", rowDiff);
            }
            else
                rowPart = "R";

            String columnPart;
            var columnDiff = address.ColumnNumber - Address.ColumnNumber;
            if (columnDiff != 0 || address.FixedColumn)
            {
                if(address.FixedColumn)
                    columnPart = String.Format("C{0}", address.ColumnNumber);
                else
                    columnPart = String.Format("C[{0}]", columnDiff);
            }
            else
                columnPart = "C";

            return rowPart + columnPart;
        }
    }
}
