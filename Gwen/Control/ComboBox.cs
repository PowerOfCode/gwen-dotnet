using System;
using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// ComboBox control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ComboBox : Button
    {
        private readonly Menu menu;
        private readonly ControlBase button;
        private MenuItem selectedItem;

        /// <summary>
        /// Invoked when the selected item has changed.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> ItemSelected;

        /// <summary>
        /// Indicates whether the combo menu is open.
        /// </summary>
        public bool IsOpen { get { return menu != null && !menu.IsHidden; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ComboBox(ControlBase parent)
            : base(parent)
        {
            SetSize(100, 20);
            menu = new Menu(this);
            menu.IsHidden = true;
            menu.IconMarginDisabled = true;
            menu.IsTabable = false;

            DownArrow arrow = new DownArrow(this);
            button = arrow;

            Alignment = Pos.Left | Pos.CenterV;
            Text = String.Empty;
            Margin = new Margin(3, 0, 0, 0);

            IsTabable = true;
            KeyboardInputEnabled = true;
        }

        /// <summary>
        /// Selected item.
        /// </summary>
        /// <remarks>Not just String property, because items also have internal names.</remarks>
        public MenuItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != null && value.Parent == menu)
                {
                    selectedItem = value;
                    onItemSelected(selectedItem, new ItemSelectedEventArgs(value));
                }
            }
        }

        internal override bool IsMenuComponent
        {
            get { return true; }
        }

        /// <summary>
        /// Adds a new item.
        /// </summary>
        /// <param name="label">Item label (displayed).</param>
        /// <param name="name">Item name.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string label, string name = "", object UserData = null)
        {
            MenuItem item = menu.AddItem(label, String.Empty);
            item.Name = name;
            item.Selected += onItemSelected;
            item.UserData = UserData;

            if (selectedItem == null)
                onItemSelected(item, new ItemSelectedEventArgs(null));

            return item;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawComboBox(this, IsDepressed, IsOpen);
        }

        public override void Disable()
        {
            base.Disable();
            GetCanvas().CloseMenus();
        }

        /// <summary>
        /// Internal Pressed implementation.
        /// </summary>
        protected override void onClicked(int x, int y)
        {
            if (IsOpen)
            {
                GetCanvas().CloseMenus();
                return;
            }

            bool wasMenuHidden = menu.IsHidden;

            GetCanvas().CloseMenus();

            if (wasMenuHidden)
            {
                Open();
            }

			base.onClicked(x, y);
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        public virtual void DeleteAll()
        {
            if (menu != null)
                menu.DeleteAll();
        }

        /// <summary>
        /// Internal handler for item selected event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onItemSelected(ControlBase control, ItemSelectedEventArgs args)
        {
            if (!IsDisabled)
            {
                //Convert selected to a menu item
                MenuItem item = control as MenuItem;
                if (null == item) return;

                selectedItem = item;
                Text = selectedItem.Text;
                menu.IsHidden = true;

                if (ItemSelected != null)
                    ItemSelected.Invoke(this, args);

                Focus();
                Invalidate();
            }
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            button.Position(Pos.Right | Pos.CenterV, 4, 0);
            base.layout(skin);
        }

        /// <summary>
        /// Handler for losing keyboard focus.
        /// </summary>
        protected override void onLostKeyboardFocus()
        {
            TextColor = Color.Black;
        }

        /// <summary>
        /// Handler for gaining keyboard focus.
        /// </summary>
        protected override void onKeyboardFocus()
        {
            //Until we add the blue highlighting again
            TextColor = Color.Black;
        }

        /// <summary>
        /// Opens the combo.
        /// </summary>
        public virtual void Open()
        {
            if (!IsDisabled)
            {
                if (null == menu) return;

                menu.Parent = GetCanvas();
                menu.IsHidden = false;
                menu.BringToFront();

                Point p = LocalPosToCanvas(Point.Empty);

                menu.SetBounds(new Rectangle(p.X, p.Y + Height, Width, menu.Height));
            }
        }

        /// <summary>
        /// Closes the combo.
        /// </summary>
        public virtual void Close()
        {
            if (menu == null)
                return;

            menu.Hide();
        }

        /// <summary>
        /// Handler for Down Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyDown(bool down)
        {
            if (down)
            {
                var it = menu.Children.FindIndex(x => x == selectedItem);
                if (it + 1 < menu.Children.Count)
                    onItemSelected(this, new ItemSelectedEventArgs(menu.Children[it + 1]));
            }
            return true;
        }

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyUp(bool down)
        {
            if (down)
            {
                var it = menu.Children.FindLastIndex(x => x == selectedItem);
                if (it - 1 >= 0)
                    onItemSelected(this, new ItemSelectedEventArgs(menu.Children[it - 1]));
            }
            return true;
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderFocus(Skin.SkinBase skin)
        {

        }

        /// <summary>
        /// Selects the first menu item with the given text it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="label">The label to look for, this is what is shown to the user.</param>
        public void SelectByText(string text)
        {
            foreach (MenuItem item in menu.Children)
            {
                if (item.Text == text)
                {
                    SelectedItem = item;
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
            foreach (MenuItem item in menu.Children)
            {
                if (item.Name == name)
                {
                    SelectedItem = item;
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
            foreach (MenuItem item in menu.Children)
            {
                if (userdata == null)
                {
                    if (item.UserData == null)
                    {
                        SelectedItem = item;
                        return;
                    }
                }
                else if (userdata.Equals(item.UserData))
                {
                    SelectedItem = item;
                    return;
                }
            }
        }
    }
}
