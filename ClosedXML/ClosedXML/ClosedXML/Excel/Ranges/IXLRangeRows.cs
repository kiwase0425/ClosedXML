﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClosedXML.Excel
{
    public interface IXLRangeRows: IEnumerable<IXLRangeRow>, IXLStylized
    {
        void Clear();
        void Add(IXLRangeRow range);
        String FormulaA1 { set; }
        String FormulaR1C1 { set; }
    }
}
