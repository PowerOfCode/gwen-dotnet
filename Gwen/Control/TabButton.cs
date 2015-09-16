using System;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Tab header.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TabButton : Button
    {
        private ControlBase page;
        private TabControl control;

        /// <summary>
        /// Indicates whether the tab is active.
        /// </summary>
        public bool IsActive { get { return page != null && page.IsVisible; } }

        // todo: remove public access
        public TabControl TabControl
        {
            get { return control; }
            set
            {
                if (value == control) return;
                if (control != null)
                    control.OnLoseTab(this);
                control = value;
            }
        }

        /// <summary>
        /// Interior of the tab.
        /// </summary>
        public ControlBase Page { get { return page; } set { page = value; } }

        /// <summary>
        /// Determines whether the control should be clipped to its bounds while rendering.
        /// </summary>
        protected override bool shouldClip
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TabButton(ControlBase parent)
            : base(parent)
        {
            DragAndDrop_SetPackage(true, "TabButtonMove");
            Alignment = Pos.Top | Pos.Left;
            TextPadding = new Padding(5, 3, 3, 3);
            Padding = Padding.Two;
            KeyboardInputEnabled = true;
        }

        public override void DragAndDrop_StartDragging(DragDrop.Package package, int x, int y)
        {
            IsHidden = true;
        }

        public override void DragAndDrop_EndDragging(bool success, int x, int y)
        {
            IsHidden = false;
            IsDepressed = false;
        }

        public override bool DragAndDrop_ShouldStartDrag()
        {
            return control.AllowReorder;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawTabButton(this, IsActive, control.TabStrip.Dock);
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
            onKeyRight(down);
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
            onKeyLeft(down);
            return true;
        }

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyRight(bool down)
        {
            if (down)
            {
                var count = Parent.Children.Count;
                int me = Parent.Children.IndexOf(this);
                if (me + 1 < count)
                {
                    var nextTab = Parent.Children[me + 1];
                    TabControl.OnTabPressed(nextTab, EventArgs.Empty);
                    InputHandler.KeyboardFocus = nextTab;
                }
            }

            return true;
        }

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyLeft(bool down)
        {
            if (down)
            {
                var count = Parent.Children.Count;
                int me = Parent.Children.IndexOf(this);
                if (me - 1 >= 0)
                {
                    var prevTab = Parent.Children[me - 1];
                    TabControl.OnTabPressed(prevTab, EventArgs.Empty);
                    InputHandler.KeyboardFocus = prevTab;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        public override void UpdateColors()
        {
            if (IsActive)
            {
                if (IsDisabled)
                {
                    TextColor = Skin.Colors.Tab.Active.Disabled;
                    return;
                }
                if (IsDepressed)
                {
                    TextColor = Skin.Colors.Tab.Active.Down;
                    return;
                }
                if (IsHovered)
                {
                    TextColor = Skin.Colors.Tab.Active.Hover;
                    return;
                }

                TextColor = Skin.Colors.Tab.Active.Normal;
            }

            if (IsDisabled)
            {
                TextColor = Skin.Colors.Tab.Inactive.Disabled;
                return;
            }
            if (IsDepressed)
            {
                TextColor = Skin.Colors.Tab.Inactive.Down;
                return;
            }
            if (IsHovered)
            {
                TextColor = Skin.Colors.Tab.Inactive.Hover;
                return;
            }

            TextColor = Skin.Colors.Tab.Inactive.Normal;
        }
    }
}
