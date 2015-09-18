using System;
using System.Drawing;
using System.Linq;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// Single table row.
	/// </summary>
	public class TableRow : ControlBase
	{
		public TableRow(ControlBase parent)
			: base(parent)
		{
		}

		protected override void layout(Gwen.Skin.SkinBase skin)
		{
			Table parent = Parent as Table;

            Height = 0;
            Width = 0;

            // Fill ColumnWidths List
			if (parent.Children[0] == this)
			{
				parent.ColumnWidths.Clear();
                for (int columnIndex = 0; columnIndex < Children.Count; columnIndex++)
                {
                    int colWidth = 0;

                    for (int rowIndex = 0; rowIndex < parent.Children.Count; rowIndex++)
                    {
                        ControlBase element = parent.Children[rowIndex].Children[columnIndex];

                        colWidth = Math.Max(element.Width + element.Margin.Width(), colWidth);
                    }

                    parent.ColumnWidths.Add(colWidth);
                }
			}

            // Set Width and Height of this row
            foreach (var child in Children)
            {
                int height = child.Height + child.Margin.Height();

                if (height > Height)
                    Height = height;
            }

            Width = parent.ColumnWidths.Sum();

            // Set cell positions
			int shift = 0;
            if (Children.Count != 0)
            {
                ControlBase child = Children[0];

                shift = child.Margin.Left;
                child.SetPosition(shift, child.Margin.Top);

                for (int i = 1; i < Children.Count; i++)
                {
                    child = Children[i];

                    shift += parent.ColumnWidths[i - 1];

                    child.SetPosition(shift, child.Margin.Top);
                }
            }

			base.layout(skin);
		}
	}
}
