using System;
using System.Drawing;
using Gwen.ControlInternal;
using Gwen.DragDrop;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Base for dockable containers.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class DockBase : ControlBase
    {
        private DockBase left;
        private DockBase right;
        private DockBase top;
        private DockBase bottom;
        private Resizer sizer;

        // Only CHILD dockpanels have a tabcontrol.
        private DockedTabControl dockedTabControl;

        private bool drawHover;
        private bool dropFar;
        private Rectangle hoverRect;

        // todo: dock events?

        /// <summary>
        /// Control docked on the left side.
        /// </summary>
        public DockBase LeftDock { get { return getChildDock(Pos.Left); } }

        /// <summary>
        /// Control docked on the right side.
        /// </summary>
        public DockBase RightDock { get { return getChildDock(Pos.Right); } }

        /// <summary>
        /// Control docked on the top side.
        /// </summary>
        public DockBase TopDock { get { return getChildDock(Pos.Top); } }

        /// <summary>
        /// Control docked on the bottom side.
        /// </summary>
        public DockBase BottomDock { get { return getChildDock(Pos.Bottom); } }

        public TabControl TabControl { get { return dockedTabControl; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockBase"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public DockBase(ControlBase parent)
            : base(parent)
        {
            Padding = Padding.One;
            SetSize(200, 200);
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeySpace(bool down)
        {
            // No action on space (default button action is to press)
            return false;
        }

        /// <summary>
        /// Initializes an inner docked control for the specified position.
        /// </summary>
        /// <param name="pos">Dock position.</param>
        protected virtual void setupChildDock(Pos pos)
        {
            if (dockedTabControl == null)
            {
                dockedTabControl = new DockedTabControl(this);
                dockedTabControl.TabRemoved += onTabRemoved;
                dockedTabControl.TabStripPosition = Pos.Bottom;
                dockedTabControl.TitleBarVisible = true;
            }

            Dock = pos;

            Pos sizeDir;
            if (pos == Pos.Right) sizeDir = Pos.Left;
            else if (pos == Pos.Left) sizeDir = Pos.Right;
            else if (pos == Pos.Top) sizeDir = Pos.Bottom;
            else if (pos == Pos.Bottom) sizeDir = Pos.Top;
            else throw new ArgumentException("Invalid dock", "pos");

            if (sizer != null)
                sizer.Dispose();
            sizer = new Resizer(this);
            sizer.Dock = sizeDir;
            sizer.ResizeDir = sizeDir;
            sizer.SetSize(2, 2);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {

        }

        /// <summary>
        /// Gets an inner docked control for the specified position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected virtual DockBase getChildDock(Pos pos)
        {
            // todo: verify
            DockBase dock = null;
            switch (pos)
            {
                case Pos.Left:
                    if (left == null)
                    {
                        left = new DockBase(this);
                        left.setupChildDock(pos);
                    }
                    dock = left;
                    break;

                case Pos.Right:
                    if (right == null)
                    {
                        right = new DockBase(this);
                        right.setupChildDock(pos);
                    }
                    dock = right;
                    break;

                case Pos.Top:
                    if (top == null)
                    {
                        top = new DockBase(this);
                        top.setupChildDock(pos);
                    }
                    dock = top;
                    break;

                case Pos.Bottom:
                    if (bottom == null)
                    {
                        bottom = new DockBase(this);
                        bottom.setupChildDock(pos);
                    }
                    dock = bottom;
                    break;
            }

            if (dock != null)
                dock.IsHidden = false;

            return dock;
        }

        /// <summary>
        /// Calculates dock direction from dragdrop coordinates.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Dock direction.</returns>
        protected virtual Pos getDroppedTabDirection(int x, int y)
        {
            int w = Width;
            int h = Height;
            float top = y / (float)h;
            float left = x / (float)w;
            float right = (w - x) / (float)w;
            float bottom = (h - y) / (float)h;
            float minimum = Math.Min(Math.Min(Math.Min(top, left), right), bottom);

            dropFar = (minimum < 0.2f);

            if (minimum > 0.3f)
                return Pos.Fill;

            if (top == minimum && (null == this.top || this.top.IsHidden))
                return Pos.Top;
            if (left == minimum && (null == this.left || this.left.IsHidden))
                return Pos.Left;
            if (right == minimum && (null == this.right || this.right.IsHidden))
                return Pos.Right;
            if (bottom == minimum && (null == this.bottom || this.bottom.IsHidden))
                return Pos.Bottom;

            return Pos.Fill;
        }

        public override bool DragAndDrop_CanAcceptPackage(Package p)
        {
            // A TAB button dropped
            if (p.Name == "TabButtonMove")
                return true;

            // a TAB window dropped
            if (p.Name == "TabWindowMove")
                return true;

            return false;
        }

        public override bool DragAndDrop_HandleDrop(Package p, int x, int y)
        {
            Point pos = CanvasPosToLocal(new Point(x, y));
            Pos dir = getDroppedTabDirection(pos.X, pos.Y);

            DockedTabControl addTo = dockedTabControl;
            if (dir == Pos.Fill && addTo == null)
                return false;

            if (dir != Pos.Fill)
            {
                DockBase dock = getChildDock(dir);
                addTo = dock.dockedTabControl;

                if (!dropFar)
                    dock.BringToFront();
                else
                    dock.SendToBack();
            }

            if (p.Name == "TabButtonMove")
            {
                TabButton tabButton = DragAndDrop.SourceControl as TabButton;
                if (null == tabButton)
                    return false;

                addTo.AddPage(tabButton);
            }

            if (p.Name == "TabWindowMove")
            {
                DockedTabControl tabControl = DragAndDrop.SourceControl as DockedTabControl;
                if (null == tabControl)
                    return false;
                if (tabControl == addTo)
                    return false;

                tabControl.MoveTabsTo(addTo);
            }

            Invalidate();

            return true;
        }

        /// <summary>
        /// Indicates whether the control contains any docked children.
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                if (dockedTabControl != null && dockedTabControl.TabCount > 0) return false;

                if (left != null && !left.IsEmpty) return false;
                if (right != null && !right.IsEmpty) return false;
                if (top != null && !top.IsEmpty) return false;
                if (bottom != null && !bottom.IsEmpty) return false;

                return true;
            }
        }

		protected virtual void onTabRemoved(ControlBase control, EventArgs args)
        {
            doRedundancyCheck();
            doConsolidateCheck();
        }

        protected virtual void doRedundancyCheck()
        {
            if (!IsEmpty) return;

            DockBase pDockParent = Parent as DockBase;
            if (null == pDockParent) return;

            pDockParent.onRedundantChildDock(this);
        }

        protected virtual void doConsolidateCheck()
        {
            if (IsEmpty) return;
            if (null == dockedTabControl) return;
            if (dockedTabControl.TabCount > 0) return;

            if (bottom != null && !bottom.IsEmpty)
            {
                bottom.dockedTabControl.MoveTabsTo(dockedTabControl);
                return;
            }

            if (top != null && !top.IsEmpty)
            {
                top.dockedTabControl.MoveTabsTo(dockedTabControl);
                return;
            }

            if (left != null && !left.IsEmpty)
            {
                left.dockedTabControl.MoveTabsTo(dockedTabControl);
                return;
            }

            if (right != null && !right.IsEmpty)
            {
                right.dockedTabControl.MoveTabsTo(dockedTabControl);
                return;
            }
        }

        protected virtual void onRedundantChildDock(DockBase dock)
        {
            dock.IsHidden = true;
            doRedundancyCheck();
            doConsolidateCheck();
        }

        public override void DragAndDrop_HoverEnter(Package p, int x, int y)
        {
            drawHover = true;
        }

        public override void DragAndDrop_HoverLeave(Package p)
        {
            drawHover = false;
        }

        public override void DragAndDrop_Hover(Package p, int x, int y)
        {
            Point pos = CanvasPosToLocal(new Point(x, y));
            Pos dir = getDroppedTabDirection(pos.X, pos.Y);

            if (dir == Pos.Fill)
            {
                if (null == dockedTabControl)
                {
                    hoverRect = Rectangle.Empty;
                    return;
                }

                hoverRect = InnerBounds;
                return;
            }

            hoverRect = RenderBounds;

            int HelpBarWidth = 0;

            if (dir == Pos.Left)
            {
                HelpBarWidth = (int)(hoverRect.Width * 0.25f);
                hoverRect.Width = HelpBarWidth;
            }

            if (dir == Pos.Right)
            {
                HelpBarWidth = (int)(hoverRect.Width * 0.25f);
                hoverRect.X = hoverRect.Width - HelpBarWidth;
                hoverRect.Width = HelpBarWidth;
            }

            if (dir == Pos.Top)
            {
                HelpBarWidth = (int)(hoverRect.Height * 0.25f);
                hoverRect.Height = HelpBarWidth;
            }

            if (dir == Pos.Bottom)
            {
                HelpBarWidth = (int)(hoverRect.Height * 0.25f);
                hoverRect.Y = hoverRect.Height - HelpBarWidth;
                hoverRect.Height = HelpBarWidth;
            }

            if ((dir == Pos.Top || dir == Pos.Bottom) && !dropFar)
            {
                if (left != null && left.IsVisible)
                {
                    hoverRect.X += left.Width;
                    hoverRect.Width -= left.Width;
                }

                if (right != null && right.IsVisible)
                {
                    hoverRect.Width -= right.Width;
                }
            }

            if ((dir == Pos.Left || dir == Pos.Right) && !dropFar)
            {
                if (top != null && top.IsVisible)
                {
                    hoverRect.Y += top.Height;
                    hoverRect.Height -= top.Height;
                }

                if (bottom != null && bottom.IsVisible)
                {
                    hoverRect.Height -= bottom.Height;
                }
            }
        }

        /// <summary>
        /// Renders over the actual control (overlays).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderOver(Skin.SkinBase skin)
        {
            if (!drawHover)
                return;

            Renderer.RendererBase render = skin.Renderer;
            render.DrawColor = Color.FromArgb(20, 255, 200, 255);
            render.DrawFilledRect(RenderBounds);

            if (hoverRect.Width == 0)
                return;

            render.DrawColor = Color.FromArgb(100, 255, 200, 255);
            render.DrawFilledRect(hoverRect);

            render.DrawColor = Color.FromArgb(200, 255, 200, 255);
            render.DrawLinedRect(hoverRect);
        }
    }
}
