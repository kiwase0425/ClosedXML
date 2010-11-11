﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClosedXML.Excel
{
    public interface IXLRangeColumns: IEnumerable<IXLRangeColumn>, IXLStylized
    {
        void Clear();
        void Add(IXLRangeColumn range);
        String FormulaA1 { set; }
        String FormulaR1C1 { set; }
    }
}
