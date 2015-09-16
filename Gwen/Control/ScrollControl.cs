using System;
using System.Linq;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Base for controls whose interior can be scrolled.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ScrollControl : ControlBase
    {
        private bool canScrollH;
        private bool canScrollV;
        private bool autoHideBars;

        public int VerticalScroll
        {
            get
            {
                return innerPanel.Y;
            }
        }

        public int HorizontalScroll
        {
            get
            {
                return innerPanel.X;
            }
        }

        public ControlBase InnerPanel
        {
            get
            {
                return innerPanel;
            }
        }

        private readonly ScrollBar verticalScrollBar;
        private readonly ScrollBar horizontalScrollBar;

        /// <summary>
        /// Indicates whether the control can be scrolled horizontally.
        /// </summary>
        public bool CanScrollH { get { return canScrollH; } }

        /// <summary>
        /// Indicates whether the control can be scrolled vertically.
        /// </summary>
        public bool CanScrollV { get { return canScrollV; } }

        /// <summary>
        /// Determines whether the scroll bars should be hidden if not needed.
        /// </summary>
        public bool AutoHideBars { get { return autoHideBars; } set { autoHideBars = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ScrollControl(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = false;

            verticalScrollBar = new VerticalScrollBar(this);
            verticalScrollBar.Dock = Pos.Right;
            verticalScrollBar.BarMoved += vBarMoved;
            canScrollV = true;
            verticalScrollBar.NudgeAmount = 30;

            horizontalScrollBar = new HorizontalScrollBar(this);
            horizontalScrollBar.Dock = Pos.Bottom;
            horizontalScrollBar.BarMoved += hBarMoved;
            canScrollH = true;
            horizontalScrollBar.NudgeAmount = 30;

            innerPanel = new ControlBase(this);
            innerPanel.SetPosition(0, 0);
            innerPanel.Margin = Margin.Five;
            innerPanel.SendToBack();
            innerPanel.MouseInputEnabled = false;

            autoHideBars = false;
        }

        protected bool hScrollRequired
        {
            set
            {
                if (value)
                {
                    horizontalScrollBar.SetScrollAmount(0, true);
                    horizontalScrollBar.IsDisabled = true;
                    if (autoHideBars)
                        horizontalScrollBar.IsHidden = true;
                }
                else
                {
                    horizontalScrollBar.IsHidden = false;
                    horizontalScrollBar.IsDisabled = false;
                }
            }
        }

        protected bool vScrollRequired
        {
            set
            {
                if (value)
                {
                    verticalScrollBar.SetScrollAmount(0, true);
                    verticalScrollBar.IsDisabled = true;
                    if (autoHideBars)
                        verticalScrollBar.IsHidden = true;
                }
                else
                {
                    verticalScrollBar.IsHidden = false;
                    verticalScrollBar.IsDisabled = false;
                }
            }
        }

        /// <summary>
        /// Enables or disables inner scrollbars.
        /// </summary>
        /// <param name="horizontal">Determines whether the horizontal scrollbar should be enabled.</param>
        /// <param name="vertical">Determines whether the vertical scrollbar should be enabled.</param>
        public virtual void EnableScroll(bool horizontal, bool vertical)
        {
            canScrollV = vertical;
            canScrollH = horizontal;
            verticalScrollBar.IsHidden = !canScrollV;
            horizontalScrollBar.IsHidden = !canScrollH;
        }

        public virtual void SetInnerSize(int width, int height)
        {
            innerPanel.SetSize(width, height);
        }

        protected virtual void vBarMoved(ControlBase control, EventArgs args)
        {
            Invalidate();
        }

        protected virtual void hBarMoved(ControlBase control, EventArgs args)
        {
            Invalidate();
        }

        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        /// <param name="oldChildBounds"></param>
        /// <param name="child"></param>
        protected override void onChildBoundsChanged(System.Drawing.Rectangle oldChildBounds, ControlBase child)
        {
            UpdateScrollBars();
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            UpdateScrollBars();
            base.layout(skin);
        }

        /// <summary>
        /// Handler invoked on mouse wheel event.
        /// </summary>
        /// <param name="delta">Scroll delta.</param>
        /// <returns></returns>
        protected override bool onMouseWheeled(int delta)
        {
            if (CanScrollV && verticalScrollBar.IsVisible)
            {
                if (verticalScrollBar.SetScrollAmount(
                    verticalScrollBar.ScrollAmount - verticalScrollBar.NudgeAmount * (delta / 60.0f), true))
                    return true;
            }

            if (CanScrollH && horizontalScrollBar.IsVisible)
            {
                if (horizontalScrollBar.SetScrollAmount(
                    horizontalScrollBar.ScrollAmount - horizontalScrollBar.NudgeAmount * (delta / 60.0f), true))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
#if false

    // Debug render - this shouldn't render ANYTHING REALLY - it should be up to the parent!

    Gwen::Rect rect = GetRenderBounds();
    Gwen::Renderer::Base* render = skin->GetRender();

    render->SetDrawColor( Gwen::Color( 255, 255, 0, 100 ) );
    render->DrawFilledRect( rect );

    render->SetDrawColor( Gwen::Color( 255, 0, 0, 100 ) );
    render->DrawFilledRect( m_InnerPanel->GetBounds() );

    render->RenderText( skin->GetDefaultFont(), Gwen::Point( 0, 0 ), Utility::Format( L"Offset: %i %i", m_InnerPanel->X(), m_InnerPanel->Y() ) );
#endif
        }

        public virtual void UpdateScrollBars()
        {
            if (innerPanel == null)
                return;

            //Get the max size of all our children together
            int childrenWidth = Children.Count > 0 ? Children.Max(x => x.Right) : 0;
            int childrenHeight = Children.Count > 0 ? Children.Max(x => x.Bottom) : 0;

            if (canScrollH)
            {
                innerPanel.SetSize(Math.Max(Width, childrenWidth), Math.Max(Height, childrenHeight));
            }
            else
            {
                innerPanel.SetSize(Width - (verticalScrollBar.IsHidden ? 0 : verticalScrollBar.Width),
                                     Math.Max(Height, childrenHeight));
            }

            float wPercent = Width /
                             (float)(childrenWidth + (verticalScrollBar.IsHidden ? 0 : verticalScrollBar.Width));
            float hPercent = Height /
                             (float)
                             (childrenHeight + (horizontalScrollBar.IsHidden ? 0 : horizontalScrollBar.Height));

            if (canScrollV)
                vScrollRequired = hPercent >= 1;
            else
                verticalScrollBar.IsHidden = true;

            if (canScrollH)
                hScrollRequired = wPercent >= 1;
            else
                horizontalScrollBar.IsHidden = true;


            verticalScrollBar.ContentSize = innerPanel.Height;
            verticalScrollBar.ViewableContentSize = Height - (horizontalScrollBar.IsHidden ? 0 : horizontalScrollBar.Height);


            horizontalScrollBar.ContentSize = innerPanel.Width;
            horizontalScrollBar.ViewableContentSize = Width - (verticalScrollBar.IsHidden ? 0 : verticalScrollBar.Width);

            int newInnerPanelPosX = 0;
            int newInnerPanelPosY = 0;

            if (CanScrollV && !verticalScrollBar.IsHidden)
            {
                newInnerPanelPosY =
                    (int)(
                        -((innerPanel.Height) - Height + (horizontalScrollBar.IsHidden ? 0 : horizontalScrollBar.Height)) *
                        verticalScrollBar.ScrollAmount);
            }
            if (CanScrollH && !horizontalScrollBar.IsHidden)
            {
                newInnerPanelPosX =
                    (int)(
                        -((innerPanel.Width) - Width + (verticalScrollBar.IsHidden ? 0 : verticalScrollBar.Width)) *
                        horizontalScrollBar.ScrollAmount);
            }

            innerPanel.SetPosition(newInnerPanelPosX, newInnerPanelPosY);
        }

        public virtual void ScrollToBottom()
        {
            if (!CanScrollV)
                return;

            UpdateScrollBars();
            verticalScrollBar.ScrollToBottom();
        }

        public virtual void ScrollToTop()
        {
            if (CanScrollV)
            {
                UpdateScrollBars();
                verticalScrollBar.ScrollToTop();
            }
        }

        public virtual void ScrollToLeft()
        {
            if (CanScrollH)
            {
                UpdateScrollBars();
                verticalScrollBar.ScrollToLeft();
            }
        }

        public virtual void ScrollToRight()
        {
            if (CanScrollH)
            {
                UpdateScrollBars();
                verticalScrollBar.ScrollToRight();
            }
        }

        public virtual void DeleteAll()
        {
            innerPanel.DeleteAllChildren();
        }
    }
}
