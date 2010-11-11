﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ClosedXML.Excel
{
    internal class XLRange: XLRangeBase, IXLRange
    {
        public XLRange(XLRangeParameters xlRangeParameters): base(xlRangeParameters.RangeAddress)
        {
            Worksheet = xlRangeParameters.Worksheet;
            Worksheet.RangeShiftedRows += new RangeShiftedRowsDelegate(Worksheet_RangeShiftedRows);
            Worksheet.RangeShiftedColumns += new RangeShiftedColumnsDelegate(Worksheet_RangeShiftedColumns);
            this.defaultStyle = new XLStyle(this, xlRangeParameters.DefaultStyle);
        }

        void Worksheet_RangeShiftedColumns(XLRange range, int columnsShifted)
        {
            if (!RangeAddress.IsInvalid && !range.RangeAddress.IsInvalid)
            {
                if (columnsShifted < 0
                    // all columns
                    && RangeAddress.FirstAddress.ColumnNumber >= range.RangeAddress.FirstAddress.ColumnNumber
                    && RangeAddress.LastAddress.ColumnNumber <= range.RangeAddress.FirstAddress.ColumnNumber - columnsShifted
                    // all rows
                    && RangeAddress.FirstAddress.RowNumber >= range.RangeAddress.FirstAddress.RowNumber
                    && RangeAddress.LastAddress.RowNumber <= range.RangeAddress.LastAddress.RowNumber
                    )
                {
                    ((XLRangeAddress)RangeAddress).IsInvalid = true;
                }
                else
                {
                    if (range.RangeAddress.FirstAddress.RowNumber <= RangeAddress.FirstAddress.RowNumber
                        && range.RangeAddress.LastAddress.RowNumber >= RangeAddress.LastAddress.RowNumber)
                    {
                        if (range.RangeAddress.FirstAddress.ColumnNumber <= RangeAddress.FirstAddress.ColumnNumber)
                            RangeAddress.FirstAddress = new XLAddress(RangeAddress.FirstAddress.RowNumber, RangeAddress.FirstAddress.ColumnNumber + columnsShifted);

                        if (range.RangeAddress.FirstAddress.ColumnNumber <= RangeAddress.LastAddress.ColumnNumber)
                            RangeAddress.LastAddress = new XLAddress(RangeAddress.LastAddress.RowNumber, RangeAddress.LastAddress.ColumnNumber + columnsShifted);
                    }
                }
            }
        }

        void Worksheet_RangeShiftedRows(XLRange range, int rowsShifted)
        {
            if (!RangeAddress.IsInvalid && !range.RangeAddress.IsInvalid)
            {
                if (rowsShifted < 0
                    // all columns
                    && RangeAddress.FirstAddress.ColumnNumber >= range.RangeAddress.FirstAddress.ColumnNumber
                    && RangeAddress.LastAddress.ColumnNumber <= range.RangeAddress.FirstAddress.ColumnNumber
                    // all rows
                    && RangeAddress.FirstAddress.RowNumber >= range.RangeAddress.FirstAddress.RowNumber
                    && RangeAddress.LastAddress.RowNumber <= range.RangeAddress.LastAddress.RowNumber - rowsShifted
                    )
                {
                    ((XLRangeAddress)RangeAddress).IsInvalid = true;
                }
                else
                {
                    if (range.RangeAddress.FirstAddress.ColumnNumber <= RangeAddress.FirstAddress.ColumnNumber
                        && range.RangeAddress.LastAddress.ColumnNumber >= RangeAddress.LastAddress.ColumnNumber)
                    {
                        if (range.RangeAddress.FirstAddress.RowNumber <= RangeAddress.FirstAddress.RowNumber)
                            RangeAddress.FirstAddress = new XLAddress(RangeAddress.FirstAddress.RowNumber + rowsShifted, RangeAddress.FirstAddress.ColumnNumber);

                        if (range.RangeAddress.FirstAddress.RowNumber <= RangeAddress.LastAddress.RowNumber)
                            RangeAddress.LastAddress = new XLAddress(RangeAddress.LastAddress.RowNumber + rowsShifted, RangeAddress.LastAddress.ColumnNumber);
                    }
                }
            }
        }

        #region IXLRange Members

        public IXLRangeColumn FirstColumn()
        {
            return this.Column(1);
        }
        public IXLRangeColumn LastColumn()
        {
            return this.Column(this.ColumnCount());
        }
        public IXLRangeColumn FirstColumnUsed()
        {
            var firstColumn = this.RangeAddress.FirstAddress.ColumnNumber;
            var columnCount = this.ColumnCount();
            Int32 minColumnUsed = Int32.MaxValue;
            Int32 minColumnInCells = Int32.MaxValue;
            if (this.Worksheet.Internals.CellsCollection.Any(c => c.Key.ColumnNumber >= firstColumn && c.Key.ColumnNumber <= columnCount))
                minColumnInCells = this.Worksheet.Internals.CellsCollection
                    .Where(c => c.Key.ColumnNumber >= firstColumn && c.Key.ColumnNumber <= columnCount).Select(c => c.Key.ColumnNumber).Min();

            Int32 minCoInColumns = Int32.MaxValue;
            if (this.Worksheet.Internals.ColumnsCollection.Any(c => c.Key >= firstColumn && c.Key <= columnCount))
                minCoInColumns = this.Worksheet.Internals.ColumnsCollection
                    .Where(c => c.Key >= firstColumn && c.Key <= columnCount).Select(c => c.Key).Min();

            minColumnUsed = minColumnInCells < minCoInColumns ? minColumnInCells : minCoInColumns;

            if (minColumnUsed == Int32.MaxValue)
                return null;
            else
                return this.Column(minColumnUsed);
        }
        public IXLRangeColumn LastColumnUsed()
        {
            var firstColumn = this.RangeAddress.FirstAddress.ColumnNumber;
            var columnCount = this.ColumnCount();
            Int32 maxColumnUsed = 0;
            Int32 maxColumnInCells = 0;
            if (this.Worksheet.Internals.CellsCollection.Any(c => c.Key.ColumnNumber >= firstColumn && c.Key.ColumnNumber <= columnCount))
                maxColumnInCells = this.Worksheet.Internals.CellsCollection
                    .Where(c => c.Key.ColumnNumber >= firstColumn && c.Key.ColumnNumber <= columnCount).Select(c => c.Key.ColumnNumber).Max();

            Int32 maxCoInColumns = 0;
            if (this.Worksheet.Internals.ColumnsCollection.Any(c => c.Key >= firstColumn && c.Key <= columnCount))
                maxCoInColumns = this.Worksheet.Internals.ColumnsCollection
                    .Where(c => c.Key >= firstColumn && c.Key <= columnCount).Select(c => c.Key).Max();

            maxColumnUsed = maxColumnInCells > maxCoInColumns ? maxColumnInCells : maxCoInColumns;

            if (maxColumnUsed == 0)
                return null;
            else
                return this.Column(maxColumnUsed);
        }

        public IXLRangeRow FirstRow()
        {
            return this.Row(1);
        }
        public IXLRangeRow LastRow()
        {
            return this.Row(this.RowCount());
        }
        public IXLRangeRow FirstRowUsed()
        {
            var firstRow = this.RangeAddress.FirstAddress.RowNumber;
            var rowCount = this.RowCount();
            Int32 minRowUsed = Int32.MaxValue;
            Int32 minRowInCells = Int32.MaxValue;
            if (this.Worksheet.Internals.CellsCollection.Any(c => c.Key.RowNumber >= firstRow && c.Key.RowNumber <= rowCount))
                minRowInCells = this.Worksheet.Internals.CellsCollection
                    .Where(c => c.Key.RowNumber >= firstRow && c.Key.RowNumber <= rowCount).Select(c => c.Key.RowNumber).Min();

            Int32 minRoInRows = Int32.MaxValue;
            if (this.Worksheet.Internals.RowsCollection.Any(r => r.Key >= firstRow && r.Key <= rowCount))
                minRoInRows = this.Worksheet.Internals.RowsCollection
                    .Where(r => r.Key >= firstRow && r.Key <= rowCount).Select(r => r.Key).Min();

            minRowUsed = minRowInCells < minRoInRows ? minRowInCells : minRoInRows;

            if (minRowUsed == Int32.MaxValue)
                return null;
            else
                return this.Row(minRowUsed);
        }
        public IXLRangeRow LastRowUsed()
        {
            var firstRow = this.RangeAddress.FirstAddress.RowNumber;
            var rowCount = this.RowCount();
            Int32 maxRowUsed = 0;
            Int32 maxRowInCells = 0;
            if (this.Worksheet.Internals.CellsCollection.Any(c => c.Key.RowNumber >= firstRow && c.Key.RowNumber <= rowCount))
                maxRowInCells = this.Worksheet.Internals.CellsCollection
                    .Where(c => c.Key.RowNumber >= firstRow && c.Key.RowNumber <= rowCount).Select(c => c.Key.RowNumber).Max();

            Int32 maxRoInRows = 0;
            if (this.Worksheet.Internals.RowsCollection.Any(r => r.Key >= firstRow && r.Key <= rowCount))
                maxRoInRows = this.Worksheet.Internals.RowsCollection
                    .Where(r => r.Key >= firstRow && r.Key <= rowCount).Select(r => r.Key).Max();

            maxRowUsed = maxRowInCells > maxRoInRows ? maxRowInCells : maxRoInRows;

            if (maxRowUsed == 0)
                return null;
            else
                return this.Row(maxRowUsed);
        }

        public IXLRangeRow Row(Int32 row)
        {
            IXLAddress firstCellAddress = new XLAddress(RangeAddress.FirstAddress.RowNumber + row - 1, RangeAddress.FirstAddress.ColumnNumber);
            IXLAddress lastCellAddress = new XLAddress(RangeAddress.FirstAddress.RowNumber + row - 1, RangeAddress.LastAddress.ColumnNumber);
            return new XLRangeRow(
                new XLRangeParameters(new XLRangeAddress(firstCellAddress, lastCellAddress), 
                    Worksheet, 
                    Worksheet.Style));
                
        }
        public IXLRangeColumn Column(Int32 column)
        {
            IXLAddress firstCellAddress = new XLAddress(RangeAddress.FirstAddress.RowNumber, RangeAddress.FirstAddress.ColumnNumber + column - 1);
            IXLAddress lastCellAddress = new XLAddress(RangeAddress.LastAddress.RowNumber, RangeAddress.FirstAddress.ColumnNumber + column - 1);
            return new XLRangeColumn(
                new XLRangeParameters(new XLRangeAddress(firstCellAddress, lastCellAddress),
                    Worksheet,
                    Worksheet.Style));
        }
        public IXLRangeColumn Column(String column)
        {
            return this.Column(XLAddress.GetColumnNumberFromLetter(column));
        }

        public IXLRangeColumns Columns()
        {
            var retVal = new XLRangeColumns(Worksheet);
            foreach (var c in Enumerable.Range(1, this.ColumnCount()))
            {
                retVal.Add(this.Column(c));
            }
            return retVal;
        }
        public IXLRangeColumns Columns(Int32 firstColumn, Int32 lastColumn)
        {
            var retVal = new XLRangeColumns(Worksheet);

            for (var co = firstColumn; co <= lastColumn; co++)
            {
                retVal.Add(this.Column(co));
            }
            return retVal;
        }
        public IXLRangeColumns Columns(String firstColumn, String lastColumn)
        {
            return this.Columns(XLAddress.GetColumnNumberFromLetter(firstColumn), XLAddress.GetColumnNumberFromLetter(lastColumn));
        }
        public IXLRangeColumns Columns(String columns)
        {
            var retVal = new XLRangeColumns(Worksheet);
            var columnPairs = columns.Split(',');
            foreach (var pair in columnPairs)
            {
                String firstColumn;
                String lastColumn;
                if (pair.Contains(':'))
                {
                    var columnRange = pair.Split(':');
                    firstColumn = columnRange[0];
                    lastColumn = columnRange[1];
                }
                else
                {
                    firstColumn = pair;
                    lastColumn = pair;
                }

                Int32 tmp;
                if (Int32.TryParse(firstColumn, out tmp))
                    foreach (var col in this.Columns(Int32.Parse(firstColumn), Int32.Parse(lastColumn)))
                    {
                        retVal.Add(col);
                    }
                else
                    foreach (var col in this.Columns(firstColumn, lastColumn))
                    {
                        retVal.Add(col);
                    }
            }
            return retVal;
        }

        public IXLRangeRows Rows()
        {
            var retVal = new XLRangeRows(Worksheet);
            foreach (var r in Enumerable.Range(1, this.RowCount()))
            {
                retVal.Add(this.Row(r));
            }
            return retVal;
        }
        public IXLRangeRows Rows(Int32 firstRow, Int32 lastRow)
        {
            var retVal = new XLRangeRows(Worksheet);

            for (var ro = firstRow; ro <= lastRow; ro++)
            {
                retVal.Add(this.Row(ro));
            }
            return retVal;
        }
        public IXLRangeRows Rows(String rows)
        {
            var retVal = new XLRangeRows(Worksheet);
            var rowPairs = rows.Split(',');
            foreach (var pair in rowPairs)
            {
                String firstRow;
                String lastRow;
                if (pair.Contains(':'))
                {
                    var rowRange = pair.Split(':');
                    firstRow = rowRange[0];
                    lastRow = rowRange[1];
                }
                else
                {
                    firstRow = pair;
                    lastRow = pair;
                }
                foreach (var row in this.Rows(Int32.Parse(firstRow), Int32.Parse(lastRow)))
                {
                    retVal.Add(row);
                }
            }
            return retVal;
        }

        public void Transpose(XLTransposeOptions transposeOption)
        {
            var rowCount = this.RowCount();
            var columnCount = this.ColumnCount();
            var squareSide = rowCount > columnCount ? rowCount : columnCount;

            var firstCell = FirstCell();
            var lastCell = LastCell();

            MoveOrClearForTranspose(transposeOption, rowCount, columnCount);
            TransposeMerged();
            TransposeRange(squareSide);
            this.RangeAddress.LastAddress = new XLAddress(
                firstCell.Address.RowNumber + columnCount - 1,
                firstCell.Address.ColumnNumber + rowCount - 1);
            if (rowCount > columnCount)
            {
                var rng = Worksheet.Range(
                    this.RangeAddress.LastAddress.RowNumber + 1,
                    this.RangeAddress.FirstAddress.ColumnNumber,
                    this.RangeAddress.LastAddress.RowNumber + (rowCount - columnCount),
                    this.RangeAddress.LastAddress.ColumnNumber);
                rng.Delete(XLShiftDeletedCells.ShiftCellsUp);
            }
            else if (columnCount > rowCount)
            {
                var rng = Worksheet.Range(
                    this.RangeAddress.FirstAddress.RowNumber,
                    this.RangeAddress.LastAddress.ColumnNumber + 1,
                    this.RangeAddress.LastAddress.RowNumber,
                    this.RangeAddress.LastAddress.ColumnNumber + (columnCount - rowCount));
                rng.Delete(XLShiftDeletedCells.ShiftCellsLeft);
            }

            foreach (var c in this.Range(1,1,columnCount, rowCount).Cells())
            {
                var border = new XLBorder(this, c.Style.Border);
                c.Style.Border.TopBorder = border.LeftBorder;
                c.Style.Border.TopBorderColor = border.LeftBorderColor;
                c.Style.Border.LeftBorder = border.TopBorder;
                c.Style.Border.LeftBorderColor = border.TopBorderColor;
                c.Style.Border.RightBorder = border.BottomBorder;
                c.Style.Border.RightBorderColor = border.BottomBorderColor;
                c.Style.Border.BottomBorder = border.RightBorder;
                c.Style.Border.BottomBorderColor = border.RightBorderColor;
            }
        }

        private void TransposeRange(int squareSide)
        {
            var cellsToInsert = new Dictionary<IXLAddress, XLCell>();
            var cellsToDelete = new List<IXLAddress>();
            XLRange rngToTranspose = (XLRange)Worksheet.Range(
                this.RangeAddress.FirstAddress.RowNumber,
                this.RangeAddress.FirstAddress.ColumnNumber,
                this.RangeAddress.FirstAddress.RowNumber + squareSide,
                this.RangeAddress.FirstAddress.ColumnNumber + squareSide);

            foreach (var c in rngToTranspose.Cells())
            {
                var newKey = new XLAddress(c.Address.ColumnNumber, c.Address.RowNumber);
                var newCell = new XLCell(newKey, c.Style, Worksheet);
                newCell.Value = c.Value;
                newCell.DataType = c.DataType;
                cellsToInsert.Add(newKey, newCell);
                cellsToDelete.Add(c.Address);
            }
            cellsToDelete.ForEach(c => this.Worksheet.Internals.CellsCollection.Remove(c));
            cellsToInsert.ForEach(c => this.Worksheet.Internals.CellsCollection.Add(c.Key, c.Value));
        }

        private void TransposeMerged()
        {
            List<String> mergeToDelete = new List<String>();
            List<String> mergeToInsert = new List<String>();
            foreach (var merge in Worksheet.Internals.MergedCells)
            {
                if (this.ContainsRange(merge))
                {
                    mergeToDelete.Add(merge);
                    String[] arrRange = merge.Split(':');
                    var firstAddress = new XLAddress(arrRange[0]);
                    var lastAddress = new XLAddress(arrRange[1]);
                    var newLastAddress = new XLAddress(lastAddress.ColumnNumber, lastAddress.RowNumber);
                    mergeToInsert.Add(firstAddress.ToString() + ":" + newLastAddress.ToString());
                }
            }
            mergeToDelete.ForEach(m => this.Worksheet.Internals.MergedCells.Remove(m));
            mergeToInsert.ForEach(m => this.Worksheet.Internals.MergedCells.Add(m));
        }

        private void MoveOrClearForTranspose(XLTransposeOptions transposeOption, int rowCount, int columnCount)
        {
            if (transposeOption == XLTransposeOptions.MoveCells)
            {
                if (rowCount > columnCount)
                {
                    this.InsertColumnsAfter(rowCount - columnCount);
                }
                else if (columnCount > rowCount)
                {
                    this.InsertRowsBelow(columnCount - rowCount);
                }
            }
            else
            {
                if (rowCount > columnCount)
                {
                    var toMove = columnCount - rowCount;
                    var rngToClear = Worksheet.Range(
                        this.RangeAddress.FirstAddress.RowNumber,
                        columnCount + 1,
                        this.RangeAddress.LastAddress.RowNumber,
                        columnCount + toMove);
                    rngToClear.Clear();
                }
                else if (columnCount > rowCount)
                {
                    var toMove = rowCount - columnCount;
                    var rngToClear = Worksheet.Range(
                        rowCount + 1,
                        this.RangeAddress.FirstAddress.ColumnNumber,
                        rowCount + toMove,
                        this.RangeAddress.LastAddress.ColumnNumber);
                    rngToClear.Clear();
                }
            }
        }

        #endregion

    }
}
