using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// Base class for multi-column tables.
	/// </summary>
	public class Table : ControlBase
	{
		public IEnumerable<TableRow> Rows
        { 
            get { return Children.Cast<TableRow>(); }
            set { Children = (List<ControlBase>)value.Cast<ControlBase>(); }
        }

		public List<int> ColumnWidths = new List<int>();

		public Table(ControlBase parent)
			: base(parent)
		{
		}

		public override void AddChild(ControlBase child)
		{
            if (child.GetType() != typeof(TableRow))
                throw new InvalidOperationException("Only TableRows can be added to a Table!");
			
			base.AddChild(child);
		}

		protected override void layout(Gwen.Skin.SkinBase skin)
		{
            Width = ColumnWidths.Sum();

            int maxRowHeight = 0;
            int maxRowHeightMargin = 0;

            // Get maximum row height
            foreach (var row in Children)
            {
                row.Width = Width;
                maxRowHeight = Math.Max(row.Height, maxRowHeight);
                maxRowHeightMargin = Math.Max(row.Height + row.Margin.Height(), maxRowHeightMargin);
            }

            maxRowHeight = Math.Max(maxRowHeight, maxRowHeightMargin);

            int fullHeight = 0;

            // Get full height
            foreach (var row in Children)
            {
                row.SetPosition(row.Margin.Left, fullHeight + row.Margin.Top);
                fullHeight += maxRowHeight;
            }

            Height = fullHeight;
			
			base.layout(skin);
		}
		
	}
}
