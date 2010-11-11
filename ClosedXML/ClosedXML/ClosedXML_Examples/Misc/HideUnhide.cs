﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

using System.Drawing;

namespace ClosedXML_Examples.Misc
{
    public class HideUnhide
    {
        #region Variables

        // Public

        // Private


        #endregion

        #region Properties

        // Public

        // Private

        // Override


        #endregion

        #region Events

        // Public

        // Private

        // Override


        #endregion

        #region Methods

        // Public
        public void Create(String filePath)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Hide Unhide");

            ws.Columns(1, 3).Hide();
            ws.Rows(1, 3).Hide();

            ws.Column(2).Unhide();
            ws.Row(2).Unhide();

            wb.SaveAs(filePath);
        }

        // Private

        // Override


        #endregion
    }
}
