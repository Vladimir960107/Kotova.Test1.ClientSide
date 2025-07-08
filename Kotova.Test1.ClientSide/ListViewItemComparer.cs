using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    public class ListViewItemComparer : IComparer
    {
        private int col;
        private bool ascending;

        public ListViewItemComparer()
        {
            col = 0;
            ascending = true;
        }

        public ListViewItemComparer(int column, bool ascending)
        {
            col = column;
            this.ascending = ascending;
        }

        public int Compare(object x, object y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return ascending ? -1 : 1;
            if (y == null) return ascending ? 1 : -1;

            ListViewItem listViewX = (ListViewItem)x;
            ListViewItem listViewY = (ListViewItem)y;

            string textX = col < listViewX.SubItems.Count ? listViewX.SubItems[col].Text : "";
            string textY = col < listViewY.SubItems.Count ? listViewY.SubItems[col].Text : "";

            int result;

            // Special handling for different column types
            switch (col)
            {
                case 4: // Personnel Number - numeric comparison if possible
                    if (int.TryParse(textX, out int numX) && int.TryParse(textY, out int numY))
                    {
                        result = numX.CompareTo(numY);
                    }
                    else
                    {
                        result = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
                    }
                    break;

                default: // Text comparison for other columns
                    result = string.Compare(textX, textY, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            return ascending ? result : -result;
        }
    }
}
