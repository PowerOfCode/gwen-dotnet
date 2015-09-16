using System;
using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Menu item.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class MenuItem : Button
    {
        private bool onStrip;
        private bool checkable;
        private bool isChecked;
        private Menu menu;
        private ControlBase submenuArrow;
        private Label accelerator;

        /// <summary>
        /// Indicates whether the item is on a menu strip.
        /// </summary>
        public bool IsOnStrip { get { return onStrip; } set { onStrip = value; } }

        /// <summary>
        /// Determines if the menu item is checkable.
        /// </summary>
        public bool IsCheckable { get { return checkable; } set { checkable = value; } }

        /// <summary>
        /// Indicates if the parent menu is open.
        /// </summary>
        public bool IsMenuOpen { get { if (menu == null) return false; return !menu.IsHidden; } }

        /// <summary>
        /// Gets or sets the check value.
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (value == isChecked)
                    return;

                isChecked = value;

                if (CheckChanged != null)
                    CheckChanged.Invoke(this, EventArgs.Empty);

                if (value)
                {
                    if (Checked != null)
						Checked.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (UnChecked != null)
						UnChecked.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the parent menu.
        /// </summary>
        public Menu Menu
        {
            get
            {
                if (null == menu)
                {
                    menu = new Menu(GetCanvas());
                    menu.IsHidden = true;

                    if (!onStrip)
                    {
                        if (submenuArrow != null)
                            submenuArrow.Dispose();
                        submenuArrow = new RightArrow(this);
                        submenuArrow.SetSize(15, 15);
                    }

                    Invalidate();
                }

                return menu;
            }
        }

        /// <summary>
        /// Invoked when the item is selected.
        /// </summary>
		public event GwenEventHandler<ItemSelectedEventArgs> Selected;

        /// <summary>
        /// Invoked when the item is checked.
        /// </summary>
		public event GwenEventHandler<EventArgs> Checked;

        /// <summary>
        /// Invoked when the item is unchecked.
        /// </summary>
		public event GwenEventHandler<EventArgs> UnChecked;

        /// <summary>
        /// Invoked when the item's check value is changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> CheckChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MenuItem(ControlBase parent)
            : base(parent)
        {
			AutoSizeToContents = true;
			onStrip = false;
            IsTabable = false;
            IsCheckable = false;
            IsChecked = false;

            accelerator = new Label(this);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawMenuItem(this, IsMenuOpen, checkable ? isChecked : false);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            if (submenuArrow != null)
            {
                submenuArrow.Position(Pos.Right | Pos.CenterV, 4, 0);
            }
            base.layout(skin);
        }

        /// <summary>
        /// Internal OnPressed implementation.
        /// </summary>
        protected override void onClicked(int x, int y)
        {
            if (menu != null)
            {
                ToggleMenu();
            }
            else if (!onStrip)
            {
                IsChecked = !IsChecked;
                if (Selected != null)
					Selected.Invoke(this, new ItemSelectedEventArgs(this));
                GetCanvas().CloseMenus();
            }
            base.onClicked(x, y);
        }

        /// <summary>
        /// Toggles the menu open state.
        /// </summary>
        public void ToggleMenu()
        {
            if (IsMenuOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        /// <summary>
        /// Opens the menu.
        /// </summary>
        public void OpenMenu()
        {
            if (null == menu) return;

            menu.IsHidden = false;
            menu.BringToFront();

            Point p = LocalPosToCanvas(Point.Empty);

            // Strip menus open downwards
            if (onStrip)
            {
                menu.SetPosition(p.X, p.Y + Height + 1);
            }
            // Submenus open sidewards
            else
            {
                menu.SetPosition(p.X + Width, p.Y);
            }

            // TODO: Option this.
            // TODO: Make sure on screen, open the other side of the
            // parent if it's better...
        }

        /// <summary>
        /// Closes the menu.
        /// </summary>
        public void CloseMenu()
        {
            if (null == menu) return;
            menu.Close();
            menu.CloseAll();
        }

        public override void SizeToContents()
        {
            base.SizeToContents();
            if (accelerator != null)
            {
                accelerator.SizeToContents();
                Width = Width + accelerator.Width;
            }
        }

        public MenuItem SetAction(GwenEventHandler<EventArgs> handler)
        {
            if (accelerator != null)
            {
                AddAccelerator(accelerator.Text, handler);
            }

            Selected += handler;
            return this;
        }

        public void SetAccelerator(string acc)
        {
            if (accelerator != null)
            {
                //m_Accelerator.DelayedDelete(); // to prevent double disposing
                accelerator = null;
            }

            if (acc == String.Empty)
                return;

            accelerator = new Label(this);
            accelerator.Dock = Pos.Right;
            accelerator.Alignment = Pos.Right | Pos.CenterV;
            accelerator.Text = acc;
            accelerator.Margin = new Margin(0, 0, 16, 0);
            // todo
        }
    }
}
