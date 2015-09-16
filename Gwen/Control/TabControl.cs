using System;
using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Control with multiple tabs that can be reordered and dragged.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TabControl : ControlBase
    {
        private readonly TabStrip tabStrip;
        private readonly ScrollBarButton[] scroll;
        private TabButton currentButton;
        private int scrollOffset;

        /// <summary>
        /// Invoked when a tab has been added.
        /// </summary>
		public event GwenEventHandler<EventArgs> TabAdded;

        /// <summary>
        /// Invoked when a tab has been removed.
        /// </summary>
		public event GwenEventHandler<EventArgs> TabRemoved;

        /// <summary>
        /// Determines if tabs can be reordered by dragging.
        /// </summary>
        public bool AllowReorder { get { return tabStrip.AllowReorder; } set { tabStrip.AllowReorder = value; } }

        /// <summary>
        /// Currently active tab button.
        /// </summary>
        public TabButton CurrentButton { get { return currentButton; } }

        /// <summary>
        /// Current tab strip position.
        /// </summary>
        public Pos TabStripPosition { get { return tabStrip.StripPosition; }set { tabStrip.StripPosition = value; } }

        /// <summary>
        /// Tab strip.
        /// </summary>
        public TabStrip TabStrip { get { return tabStrip; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TabControl(ControlBase parent)
            : base(parent)
        {
            scroll = new ScrollBarButton[2];
            scrollOffset = 0;

            tabStrip = new TabStrip(this);
            tabStrip.StripPosition = Pos.Top;

            // Make this some special control?
            scroll[0] = new ScrollBarButton(this);
            scroll[0].SetDirectionLeft();
            scroll[0].Clicked += scrollPressedLeft;
            scroll[0].SetSize(14, 16);

            scroll[1] = new ScrollBarButton(this);
            scroll[1].SetDirectionRight();
            scroll[1].Clicked += scrollPressedRight;
            scroll[1].SetSize(14, 16);

            innerPanel = new TabControlInner(this);
            innerPanel.Dock = Pos.Fill;
            innerPanel.SendToBack();

            IsTabable = false;
        }

        /// <summary>
        /// Adds a new page/tab.
        /// </summary>
        /// <param name="label">Tab label.</param>
        /// <param name="page">Page contents.</param>
        /// <returns>Newly created control.</returns>
        public TabButton AddPage(string label, ControlBase page = null)
        {
            if (null == page)
            {
                page = new ControlBase(this);
            }
            else
            {
                page.Parent = this;
            }

            TabButton button = new TabButton(tabStrip);
            button.SetText(label);
            button.Page = page;
            button.IsTabable = false;

            AddPage(button);
            return button;
        }

        /// <summary>
        /// Adds a page/tab.
        /// </summary>
        /// <param name="button">Page to add. (well, it's a TabButton which is a parent to the page).</param>
        public void AddPage(TabButton button)
        {
            ControlBase page = button.Page;
            page.Parent = this;
            page.IsHidden = true;
            page.Margin = new Margin(6, 6, 6, 6);
            page.Dock = Pos.Fill;

            button.Parent = tabStrip;
            button.Dock = Pos.Left;
            button.SizeToContents();
            if (button.TabControl != null)
                button.TabControl.unsubscribeTabEvent(button);
            button.TabControl = this;
            button.Clicked += OnTabPressed;

            if (null == currentButton)
            {
                button.Press();
            }

            if (TabAdded != null)
                TabAdded.Invoke(this, EventArgs.Empty);

            Invalidate();
        }

        private void unsubscribeTabEvent(TabButton button)
        {
            button.Clicked -= OnTabPressed;
        }

        /// <summary>
        /// Handler for tab selection.
        /// </summary>
        /// <param name="control">Event source (TabButton).</param>
		internal virtual void OnTabPressed(ControlBase control, EventArgs args)
        {
            TabButton button = control as TabButton;
            if (null == button) return;

            ControlBase page = button.Page;
            if (null == page) return;

            if (currentButton == button)
                return;

            if (null != currentButton)
            {
                ControlBase page2 = currentButton.Page;
                if (page2 != null)
                {
                    page2.IsHidden = true;
                }
                currentButton.Redraw();
                currentButton = null;
            }

            currentButton = button;

            page.IsHidden = false;

            tabStrip.Invalidate();
            Invalidate();
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void postLayout(Skin.SkinBase skin)
        {
            base.postLayout(skin);
            handleOverflow();
        }

        /// <summary>
        /// Handler for tab removing.
        /// </summary>
        /// <param name="button"></param>
        internal virtual void OnLoseTab(TabButton button)
        {
            if (currentButton == button)
                currentButton = null;

            //TODO: Select a tab if any exist.

            if (TabRemoved != null)
				TabRemoved.Invoke(this, EventArgs.Empty);

            Invalidate();
        }

        /// <summary>
        /// Number of tabs in the control.
        /// </summary>
        public int TabCount { get { return tabStrip.Children.Count; } }

        private void handleOverflow()
        {
            Point TabsSize = tabStrip.GetChildrenSize();

            // Only enable the scrollers if the tabs are at the top.
            // This is a limitation we should explore.
            // Really TabControl should have derivitives for tabs placed elsewhere where we could specialize
            // some functions like this for each direction.
            bool needed = TabsSize.X > Width && tabStrip.Dock == Pos.Top;

            scroll[0].IsHidden = !needed;
            scroll[1].IsHidden = !needed;

            if (!needed) return;

            scrollOffset = Util.Clamp(scrollOffset, 0, TabsSize.X - Width + 32);

#if false
    //
    // This isn't frame rate independent.
    // Could be better. Get rid of m_ScrollOffset and just use m_TabStrip.GetMargin().left ?
    // Then get a margin animation type and do it properly!
    // TODO!
    //
        m_TabStrip.SetMargin( Margin( Gwen::Approach( m_TabStrip.GetMargin().left, m_iScrollOffset * -1, 2 ), 0, 0, 0 ) );
        InvalidateParent();
#else
            tabStrip.Margin = new Margin(scrollOffset*-1, 0, 0, 0);
#endif

            scroll[0].SetPosition(Width - 30, 5);
            scroll[1].SetPosition(scroll[0].Right, 5);
        }

        protected virtual void scrollPressedLeft(ControlBase control, EventArgs args)
        {
            scrollOffset -= 120;
        }

        protected virtual void scrollPressedRight(ControlBase control, EventArgs args)
        {
            scrollOffset += 120;
        }
    }
}
