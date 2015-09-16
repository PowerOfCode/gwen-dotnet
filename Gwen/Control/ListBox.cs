using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gwen.Control.Layout;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// ListBox control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ListBox : ScrollControl
    {
        private readonly Table table;
        private readonly List<ListBoxRow> selectedRows;

        private bool multiSelect;
        private bool isToggle;
        private bool sizeToContents;
        private Pos oldDock; // used while autosizing

        /// <summary>
        /// Determines whether multiple rows can be selected at once.
        /// </summary>
        public bool AllowMultiSelect
        {
            get { return multiSelect; }
            set
            {
                multiSelect = value;
                if (value)
                    IsToggle = true;
            }
        }

        /// <summary>
        /// Determines whether rows can be unselected by clicking on them again.
        /// </summary>
        public bool IsToggle { get { return isToggle; } set { isToggle = value; } }

        /// <summary>
        /// Number of rows in the list box.
        /// </summary>
        public int RowCount { get { return table.RowCount; } }

        /// <summary>
        /// Returns specific row of the ListBox.
        /// </summary>
        /// <param name="index">Row index.</param>
        /// <returns>Row at the specified index.</returns>
        public ListBoxRow this[int index] { get { return table[index] as ListBoxRow; } }

        /// <summary>
        /// List of selected rows.
        /// </summary>
        public IEnumerable<TableRow> SelectedRows { get { return selectedRows; } }

        /// <summary>
        /// First selected row (and only if list is not multiselectable).
        /// </summary>
        public ListBoxRow SelectedRow
        {
            get
            {
                if (selectedRows.Count == 0)
                    return null;
                return selectedRows[0];
            }
            set
            {
                if (table.Children.Contains(value))
                {
                    if (AllowMultiSelect)
                    {
                        SelectRow(value, false);
                    }
                    else
                    {
                        SelectRow(value, true);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the selected row number.
        /// </summary>
        public int SelectedRowIndex
        {
            get
            {
                var selected = SelectedRow;
                if (selected == null)
                    return -1;
                return table.GetRowIndex(selected);
            }
            set
            {
                SelectRow(value);
            }
        }

        /// <summary>
        /// Column count of table rows.
        /// </summary>
        public int ColumnCount { get { return table.ColumnCount; } set { table.ColumnCount = value; Invalidate(); } }

        /// <summary>
        /// Invoked when a row has been selected.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> RowSelected;

        /// <summary>
        /// Invoked whan a row has beed unselected.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> RowUnselected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ListBox(ControlBase parent)
            : base(parent)
        {
            selectedRows = new List<ListBoxRow>();

			MouseInputEnabled = true;
            EnableScroll(false, true);
            AutoHideBars = true;
            Margin = Margin.One;

            table = new Table(this);
            table.Dock = Pos.Fill;
            table.ColumnCount = 1;
            table.BoundsChanged += tableResized;

            multiSelect = false;
            isToggle = false;
        }

        /// <summary>
        /// Selects the specified row by index.
        /// </summary>
        /// <param name="index">Row to select.</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRow(int index, bool clearOthers = false)
        {
            if (index < 0 || index >= table.RowCount)
                return;

            SelectRow(table.Children[index], clearOthers);
        }

        /// <summary>
        /// Selects the specified row(s) by text.
        /// </summary>
        /// <param name="rowText">Text to search for (exact match).</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRows(string rowText, bool clearOthers = false)
        {
            var rows = table.Children.OfType<ListBoxRow>().Where(x => x.Text == rowText);
            foreach (ListBoxRow row in rows)
            {
                SelectRow(row, clearOthers);
            }
        }

        /// <summary>
        /// Selects the specified row(s) by regex text search.
        /// </summary>
        /// <param name="pattern">Regex pattern to search for.</param>
        /// <param name="regexOptions">Regex options.</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRowsByRegex(string pattern, RegexOptions regexOptions = RegexOptions.None, bool clearOthers = false)
        {
            var rows = table.Children.OfType<ListBoxRow>().Where(x => Regex.IsMatch(x.Text, pattern));
            foreach (ListBoxRow row in rows)
            {
                SelectRow(row, clearOthers);
            }
        }

        /// <summary>
        /// Slelects the specified row.
        /// </summary>
        /// <param name="control">Row to select.</param>
        /// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
        public void SelectRow(ControlBase control, bool clearOthers = false)
        {
            if (!AllowMultiSelect || clearOthers)
                UnselectAll();

            ListBoxRow row = control as ListBoxRow;
            if (row == null)
                return;

            // TODO: make sure this is one of our rows!
            row.IsSelected = true;
            selectedRows.Add(row);
            if (RowSelected != null)
				RowSelected.Invoke(this, new ItemSelectedEventArgs(row));
        }

        /// <summary>
        /// Removes the all rows from the ListBox
        /// </summary>
        /// <param name="idx">Row index.</param>
        public void RemoveAllRows()
        {
            table.DeleteAllChildren();
        }

        /// <summary>
        /// Removes the specified row by index.
        /// </summary>
        /// <param name="idx">Row index.</param>
        public void RemoveRow(int idx)
        {
            table.RemoveRow(idx); // this calls Dispose()
        }

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="label">Row text.</param>
        /// <returns>Newly created control.</returns>
        public ListBoxRow AddRow(string label)
        {
            return AddRow(label, String.Empty);
        }

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="label">Row text.</param>
        /// <param name="name">Internal control name.</param>
        /// <returns>Newly created control.</returns>
        public ListBoxRow AddRow(string label, string name)
        {
            return AddRow(label, name, null);
        }

        /// <summary>
        /// Adds a new row.
        /// </summary>
        /// <param name="label">Row text.</param>
        /// <param name="name">Internal control name.</param>
        /// <param name="UserData">User data for newly created row</param>
        /// <returns>Newly created control.</returns>
        public ListBoxRow AddRow(string label, string name, Object UserData)
        {
            ListBoxRow row = new ListBoxRow(this);
            table.AddRow(row);

            row.SetCellText(0, label);
            row.Name = name;
            row.UserData = UserData;

            row.Selected += onRowSelected;

            table.SizeToContents(Width);

            return row;
        }

        /// <summary>
        /// Sets the column width (in pixels).
        /// </summary>
        /// <param name="column">Column index.</param>
        /// <param name="width">Column width.</param>
        public void SetColumnWidth(int column, int width)
        {
            table.SetColumnWidth(column, width);
            Invalidate();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawListBox(this);
        }

        /// <summary>
        /// Deselects all rows.
        /// </summary>
        public virtual void UnselectAll()
        {
            foreach (ListBoxRow row in selectedRows)
            {
                row.IsSelected = false;
                if (RowUnselected != null)
					RowUnselected.Invoke(this, new ItemSelectedEventArgs(row));
            }
            selectedRows.Clear();
        }

        /// <summary>
        /// Unselects the specified row.
        /// </summary>
        /// <param name="row">Row to unselect.</param>
        public void UnselectRow(ListBoxRow row)
        {
            row.IsSelected = false;
            selectedRows.Remove(row);

            if (RowUnselected != null)
                RowUnselected.Invoke(this, new ItemSelectedEventArgs(row));
        }

        /// <summary>
        /// Handler for the row selection event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void onRowSelected(ControlBase control, ItemSelectedEventArgs args)
        {
            // [omeg] changed default behavior
            bool clear = false;// !InputHandler.InputHandler.IsShiftDown;
			ListBoxRow row = args.SelectedItem as ListBoxRow;
            if (row == null)
                return;

            if (row.IsSelected)
            {
                if (IsToggle)
                    UnselectRow(row);
            }
            else
            {
                SelectRow(row, clear);
            }
        }

        /// <summary>
        /// Removes all rows.
        /// </summary>
        public virtual void Clear()
        {
            UnselectAll();
            table.RemoveAll();
        }

        public void SizeToContents()
        {
            sizeToContents = true;
            // docking interferes with autosizing so we disable it until sizing is done
            oldDock = table.Dock;
            table.Dock = Pos.None;
            table.SizeToContents(0); // autosize without constraints
        }

        private void tableResized(ControlBase control, EventArgs args)
        {
            if (sizeToContents)
            {
                SetSize(table.Width, table.Height);
                sizeToContents = false;
                table.Dock = oldDock;
                Invalidate();
            }
        }

        /// <summary>
        /// Selects the first menu item with the given text it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="label">The label to look for, this is what is shown to the user.</param>
        public void SelectByText(string text)
        {
            foreach (ListBoxRow item in table.Children)
            {
                if (item.Text == text)
                {
                    SelectedRow = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Selects the first menu item with the given internal name it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="name">The internal name to look for. To select by what is displayed to the user, use "SelectByText".</param>
        public void SelectByName(string name)
        {
            foreach (ListBoxRow item in table.Children)
            {
                if (item.Name == name)
                {
                    SelectedRow = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Selects the first menu item with the given user data it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="userdata">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
        /// If null is passed in, it will look for null/unset UserData.</param>
        public void SelectByUserData(object userdata)
        {
            foreach (ListBoxRow item in table.Children)
            {
                if (userdata == null)
                {
                    if (item.UserData == null)
                    {
                        SelectedRow = item;
                        return;
                    }
                }
                else if (userdata.Equals(item.UserData))
                {
                    SelectedRow = item;
                    return;
                }
            }
        }
    }
}
