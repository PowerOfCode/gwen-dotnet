using System;
using System.Drawing;
using System.Linq;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Popup menu.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Menu : ScrollControl
    {
        private bool disableIconMargin;
        private bool deleteOnClose;

        internal override bool IsMenuComponent { get { return true; } }

        public bool IconMarginDisabled { get { return disableIconMargin; } set { disableIconMargin = value; } }

        /// <summary>
        /// Determines whether the menu should be disposed on close.
        /// </summary>
        public bool DeleteOnClose { get { return deleteOnClose; } set { deleteOnClose = value; } }

        /// <summary>
        /// Determines whether the menu should open on mouse hover.
        /// </summary>
        protected virtual bool shouldHoverOpenMenu { get { return true; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Menu(ControlBase parent)
            : base(parent)
        {
            SetBounds(0, 0, 10, 10);
            Padding = Padding.Two;
            IconMarginDisabled = false;

            AutoHideBars = true;
            EnableScroll(false, true);
            DeleteOnClose = false;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawMenu(this, IconMarginDisabled);
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderUnder(Skin.SkinBase skin)
        {
            base.renderUnder(skin);
            skin.DrawShadow(this);
        }

        /// <summary>
        ///  Opens the menu.
        /// </summary>
        /// <param name="pos">Unused.</param>
        public void Open(Pos pos)
        {
            IsHidden = false;
            BringToFront();
            Point mouse = Input.InputHandler.MousePosition;
            SetPosition(mouse.X, mouse.Y);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            int childrenHeight = Children.Sum(child => child != null ? child.Height : 0);

            if (Y + childrenHeight > GetCanvas().Height)
                childrenHeight = GetCanvas().Height - Y;

            SetSize(Width, childrenHeight);

            base.layout(skin);
        }

        /// <summary>
        /// Adds a new menu item.
        /// </summary>
        /// <param name="text">Item text.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string text)
        {
            return AddItem(text, String.Empty);
        }

        /// <summary>
        /// Adds a new menu item.
        /// </summary>
        /// <param name="text">Item text.</param>
        /// <param name="iconName">Icon texture name.</param>
        /// <param name="accelerator">Accelerator for this item.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string text, string iconName, string accelerator = "")
        {
            MenuItem item = new MenuItem(this);
            item.Padding = Padding.Four;
            item.SetText(text);
            item.SetImage(iconName);
            item.SetAccelerator(accelerator);

            onAddItem(item);

            return item;
        }

        /// <summary>
        /// Add item handler.
        /// </summary>
        /// <param name="item">Item added.</param>
        protected virtual void onAddItem(MenuItem item)
        {
            item.TextPadding = new Padding(IconMarginDisabled ? 0 : 24, 0, 16, 0);
            item.Dock = Pos.Top;
            item.SizeToContents();
            item.Alignment = Pos.CenterV | Pos.Left;
            item.HoverEnter += onHoverItem;

            // Do this here - after Top Docking these values mean nothing in layout
            int w = item.Width + 10 + 32;
            if (w < Width) w = Width;
            SetSize(w, Height);
        }

        /// <summary>
        /// Closes all submenus.
        /// </summary>
        public virtual void CloseAll()
        {
            //System.Diagnostics.Debug.Print("Menu.CloseAll: {0}", this);
            Children.ForEach(child => { if (child is MenuItem) (child as MenuItem).CloseMenu(); });
        }

        /// <summary>
        /// Indicates whether any (sub)menu is open.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMenuOpen()
        {
            return Children.Any(child => { if (child is MenuItem) return (child as MenuItem).IsMenuOpen; return false; });
        }

        /// <summary>
        /// Mouse hover handler.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onHoverItem(ControlBase control, EventArgs args)
        {
            if (!shouldHoverOpenMenu) return;

            MenuItem item = control as MenuItem;
            if (null == item) return;
            if (item.IsMenuOpen) return;

            CloseAll();
            item.OpenMenu();
        }

        /// <summary>
        /// Closes the current menu.
        /// </summary>
        public virtual void Close()
        {
            //System.Diagnostics.Debug.Print("Menu.Close: {0}", this);
            IsHidden = true;
            if (DeleteOnClose)
            {
                DelayedDelete();
            }
        }

        /// <summary>
        /// Closes all submenus and the current menu.
        /// </summary>
        public override void CloseMenus()
        {
            //System.Diagnostics.Debug.Print("Menu.CloseMenus: {0}", this);
            base.CloseMenus();
            CloseAll();
            Close();
        }

        /// <summary>
        /// Adds a divider menu item.
        /// </summary>
        public virtual void AddDivider()
        {
            MenuDivider divider = new MenuDivider(this);
            divider.Dock = Pos.Top;
            divider.Margin = new Margin(IconMarginDisabled ? 0 : 24, 0, 4, 0);
        }

		public override bool SizeToChildren(bool width = true, bool height = true)
		{
			base.SizeToChildren(width, height);
			if (width) {
				int MaxWidth = this.Width;
				foreach (ControlBase child in Children) {
					if (child.Width > MaxWidth) {
						MaxWidth = child.Width;
					}
				}
				this.SetSize(MaxWidth, Height);
			}
			return true;
		}
    }
}
