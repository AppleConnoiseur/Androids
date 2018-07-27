using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Androids
{
    /// <summary>
    /// Convenience class for quickly calculating user interface elements.
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// Calculates and creates a row rectangle.
        /// </summary>
        /// <param name="inRect">Input rect</param>
        /// <param name="rowHeight">Row height</param>
        /// <param name="row">Row number</param>
        /// <returns>Resulting rect</returns>
        public static Rect GetRowRect(Rect inRect, float rowHeight, int row)
        {
            float rowOffset = inRect.y + (rowHeight * row);

            Rect outRect = new Rect(inRect.x, rowOffset, inRect.width, rowHeight);

            return outRect;
        }
    }
}
