using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Base control class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ControlBase : IDisposable
    {
        /// <summary>
        /// Delegate used for all control event handlers.
        /// </summary>
        /// <param name="control">Event source.</param>
        /// <param name="args" >Additional arguments. May be empty (EventArgs.Empty).</param>
		public delegate void GwenEventHandler<in T>(ControlBase sender,T arguments) where T : System.EventArgs;

        private bool disposed;

        private ControlBase parent;

        /// <summary>
        /// This is the panel's actual parent - most likely the logical
        /// parent's InnerPanel (if it has one). You should rarely need this.
        /// </summary>
        private ControlBase actualParent;

        /// <summary>
        /// If the innerpanel exists our children will automatically become children of that
        /// instead of us - allowing us to move them all around by moving that panel (useful for scrolling etc).
        /// </summary>
        protected ControlBase innerPanel;

        private ControlBase toolTip;

        private Skin.SkinBase skin;

        private Rectangle bounds;
        private Rectangle renderBounds;
        private Rectangle innerBounds;
        private Padding padding;
        private Margin margin;

        private string name;

        private bool restrictToParent;
        private bool disabled;
        private bool hidden;
        private bool mouseInputEnabled;
        private bool keyboardInputEnabled;
        private bool drawBackground;

        private Pos dock;

        private Cursor cursor;

        private bool tabable;

        private bool needsLayout;
        private bool cacheTextureDirty;
        private bool cacheToTexture;

        private Package dragAndDrop_Package;

        private object userData;

        private bool drawDebugOutlines;

        /// <summary>
        /// Real list of children.
        /// </summary>
        private List<ControlBase> children { get; set; }

        /// <summary>
        /// Invoked when mouse pointer enters the control.
        /// </summary>
        public event GwenEventHandler<EventArgs> HoverEnter;

        /// <summary>
        /// Invoked when mouse pointer leaves the control.
        /// </summary>
        public event GwenEventHandler<EventArgs> HoverLeave;

        /// <summary>
        /// Invoked when control's bounds have been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> BoundsChanged;

        /// <summary>
        /// Invoked when the control has been left-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> Clicked;

        /// <summary>
        /// Invoked when the control has been double-left-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> DoubleClicked;

        /// <summary>
        /// Invoked when the control has been right-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> RightClicked;

        /// <summary>
        /// Invoked when the control has been double-right-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> DoubleRightClicked;

        /// <summary>
        /// Returns true if any on click events are set.
        /// </summary>
        internal bool ClickEventAssigned
        {
            get
            {
                return Clicked != null || RightClicked != null || DoubleClicked != null || DoubleRightClicked != null;
            }
        }

        /// <summary>
        /// Accelerator map.
        /// </summary>
        private readonly Dictionary<string, GwenEventHandler<EventArgs>> accelerators;

        public const int MaxCoord = 4096;
        // added here from various places in code

        /// <summary>
        /// Logical list of children. If InnerPanel is not null, returns InnerPanel's children.
        /// </summary>
        [JsonProperty]
        public List<ControlBase> Children
        {
            get
            {
                if (innerPanel != null)
                    return innerPanel.Children;
                return children;
            }
            set
            {
                if (value != null)
                    foreach (var item in value)
                    {
                        item.Parent = this;
                    }
            }
        }

        /// <summary>
        /// The logical parent. It's usually what you expect, the control you've parented it to.
        /// </summary>
        public ControlBase Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                if (parent != null)
                {
                    parent.RemoveChild(this, false);
                }

                parent = value;
                actualParent = null;

                if (parent != null)
                {
                    parent.AddChild(this);
                }
            }
        }

        // todo: ParentChanged event?

        /// <summary>
        /// Dock position.
        /// </summary>
        [JsonProperty]
        public Pos Dock
        {
            get { return dock; }
            set
            {
                if (dock == value)
                    return;

                dock = value;

                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Current skin.
        /// </summary>
        public Skin.SkinBase Skin
        {
            get
            {
                if (skin != null)
                    return skin;
                if (parent != null)
                    return parent.Skin;
                if (Defaults.Skin != null)
                    return Defaults.Skin;

                throw new InvalidOperationException("GetSkin: null");
            }
        }

        /// <summary>
        /// Current tooltip.
        /// </summary>
        [JsonProperty]
        public ControlBase ToolTip
        {
            get { return toolTip; }
            set
            {
                toolTip = value;
                if (toolTip != null)
                {
                    toolTip.Parent = this;
                    toolTip.IsHidden = true;
                }
            }
        }

        /// <summary>
        /// Indicates whether this control is a menu component.
        /// </summary>
        internal virtual bool IsMenuComponent
        {
            get
            {
                if (parent == null)
                    return false;
                return parent.IsMenuComponent;
            }
        }

        /// <summary>
        /// Determines whether the control should be clipped to its bounds while rendering.
        /// </summary>
        protected virtual bool shouldClip { get { return true; } }

        /// <summary>
        /// Current padding - inner spacing.
        /// </summary>
        [JsonProperty]
        public Padding Padding
        {
            get { return padding; }
            set
            {
                if (padding == value)
                    return;

                padding = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Current margin - outer spacing.
        /// </summary>
        [JsonProperty]
        public Margin Margin
        {
            get { return margin; }
            set
            {
                if (margin == value)
                    return;

                margin = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Indicates whether the control is on top of its parent's children.
        /// </summary>
        public virtual bool IsOnTop { get { return this == Parent.children.First(); } }
        // todo: validate

        /// <summary>
        /// User data associated with the control.
        /// </summary>
        public object UserData { get { return userData; } set { userData = value; } }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public virtual bool IsHovered { get { return InputHandler.HoveredControl == this; } }

        /// <summary>
        /// Indicates whether the control has focus.
        /// </summary>
        public bool HasFocus { get { return InputHandler.KeyboardFocus == this; } }

        /// <summary>
        /// Indicates whether the control is disabled.
        /// </summary>
        [JsonProperty]
        public bool IsDisabled { get { return disabled; } set { disabled = value; } }

        /// <summary>
        /// Indicates whether the control is hidden.
        /// </summary>
        [JsonProperty]
        public virtual bool IsHidden
        {
            get { return hidden; }
            set
            {
                if (value == hidden)
                    return;
                hidden = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Determines whether the control's position should be restricted to parent's bounds.
        /// </summary>
        public bool RestrictToParent { get { return restrictToParent; } set { restrictToParent = value; } }

        /// <summary>
        /// Determines whether the control receives mouse input events.
        /// </summary>
        [JsonProperty]
        public bool MouseInputEnabled { get { return mouseInputEnabled; } set { mouseInputEnabled = value; } }

        /// <summary>
        /// Determines whether the control receives keyboard input events.
        /// </summary>
        [JsonProperty]
        public bool KeyboardInputEnabled { get { return keyboardInputEnabled; } set { keyboardInputEnabled = value; } }

        /// <summary>
        /// Gets or sets the mouse cursor when the cursor is hovering the control.
        /// </summary>
        public Cursor Cursor { get { return cursor; } set { cursor = value; } }

        /// <summary>
        /// Indicates whether the control is tabable (can be focused by pressing Tab).
        /// </summary>
        [JsonProperty]
        public bool IsTabable { get { return tabable; } set { tabable = value; } }

        /// <summary>
        /// Indicates whether control's background should be drawn during rendering.
        /// </summary>
        [JsonProperty]
        public bool ShouldDrawBackground { get { return drawBackground; } set { drawBackground = value; } }

        /// <summary>
        /// Indicates whether the renderer should cache drawing to a texture to improve performance (at the cost of memory).
        /// </summary>
        public bool ShouldCacheToTexture { get { return cacheToTexture; } set { cacheToTexture = value; /*Children.ForEach(x => x.ShouldCacheToTexture=value);*/ } }

        /// <summary>
        /// Gets or sets the control's internal name.
        /// </summary>
        [JsonProperty]
        public string Name { get { return name; } set { name = value; } }

        /// <summary>
        /// Control's size and position relative to the parent.
        /// </summary>
        public Rectangle Bounds { get { return bounds; } }

        /// <summary>
        /// Bounds for the renderer.
        /// </summary>
        public Rectangle RenderBounds { get { return renderBounds; } }

        /// <summary>
        /// Bounds adjusted by padding.
        /// </summary>
        public Rectangle InnerBounds { get { return innerBounds; } }

        /// <summary>
        /// Size restriction.
        /// </summary>
        [JsonProperty]
        public Point MinimumSize { get { return minimumSize; } set { minimumSize = value; } }

        /// <summary>
        /// Size restriction.
        /// </summary>
        [JsonProperty]
        public Point MaximumSize { get { return maximumSize; } set { maximumSize = value; } }

        private Point minimumSize = new Point(1, 1);
        private Point maximumSize = new Point(MaxCoord, MaxCoord);

        /// <summary>
        /// Determines whether hover should be drawn during rendering.
        /// </summary>
        protected bool shouldDrawHover { get { return InputHandler.MouseFocus == this || InputHandler.MouseFocus == null; } }

        protected virtual bool accelOnlyFocus { get { return false; } }

        protected virtual bool needsInputChars { get { return false; } }

        /// <summary>
        /// Indicates whether the control and its parents are visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (IsHidden)
                    return false;

                if (Parent != null)
                    return Parent.IsVisible;

                return true;
            }
        }

        /// <summary>
        /// Leftmost coordinate of the control.
        /// </summary>
        [JsonProperty]
        public int X { get { return bounds.X; } set { SetPosition(value, Y); } }

        /// <summary>
        /// Topmost coordinate of the control.
        /// </summary>
        [JsonProperty]
        public int Y { get { return bounds.Y; } set { SetPosition(X, value); } }

        // todo: Bottom/Right includes margin but X/Y not?

        [JsonProperty]
        public int Width { get { return bounds.Width; } set { SetSize(value, Height); } }

        [JsonProperty]
        public int Height { get { return bounds.Height; } set { SetSize(Width, value); } }

        public int Bottom { get { return bounds.Bottom + margin.Bottom; } }

        public int Right { get { return bounds.Right + margin.Right; } }

        /// <summary>
        /// Determines whether margin, padding and bounds outlines for the control will be drawn. Applied recursively to all children.
        /// </summary>
        public bool DrawDebugOutlines
        {
            get { return drawDebugOutlines; }
            set
            {
                if (drawDebugOutlines == value)
                    return;
                drawDebugOutlines = value;
                foreach (ControlBase child in Children)
                {
                    child.DrawDebugOutlines = value;
                }
            }
        }

        public Color PaddingOutlineColor { get; set; }

        public Color MarginOutlineColor { get; set; }

        public Color BoundsOutlineColor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Base"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ControlBase(ControlBase parent = null)
        {
            children = new List<ControlBase>();
            accelerators = new Dictionary<string, GwenEventHandler<EventArgs>>();

            Parent = parent;

            hidden = false;
            bounds = new Rectangle(0, 0, 10, 10);
            padding = Padding.Zero;
            margin = Margin.Zero;

            RestrictToParent = false;

            MouseInputEnabled = true;
            KeyboardInputEnabled = false;

            Invalidate();
            Cursor = Cursors.Default;
            //ToolTip = null;
            IsTabable = false;
            ShouldDrawBackground = true;
            disabled = false;
            cacheTextureDirty = true;
            cacheToTexture = false;

            BoundsOutlineColor = Color.Red;
            MarginOutlineColor = Color.Green;
            PaddingOutlineColor = Color.Blue;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            //Debug.Print("Control.Base: Disposing {0} {1:X}", this, GetHashCode());
            if (disposed)
            {
#if DEBUG
                throw new ObjectDisposedException(String.Format("Control.Base [{1:X}] disposed twice: {0}", this, GetHashCode()));
#else
                return;
#endif
            }

            if (InputHandler.HoveredControl == this)
                InputHandler.HoveredControl = null;
            if (InputHandler.KeyboardFocus == this)
                InputHandler.KeyboardFocus = null;
            if (InputHandler.MouseFocus == this)
                InputHandler.MouseFocus = null;

            DragAndDrop.ControlDeleted(this);
            Gwen.ToolTip.ControlDeleted(this);
            Animation.Cancel(this);

            foreach (ControlBase child in children)
                child.Dispose();

            children.Clear();

            disposed = true;
            GC.SuppressFinalize(this);
        }

        #if DEBUG
        ~ControlBase()
        {
            throw new InvalidOperationException(String.Format("IDisposable object finalized [{1:X}]: {0}", this, GetHashCode()));
            //Debug.Print(String.Format("IDisposable object finalized: {0}", GetType()));
        }
        #endif

        /// <summary>
        /// Detaches the control from canvas and adds to the deletion queue (processed in Canvas.doThink).
        /// </summary>
        public void DelayedDelete()
        {
            GetCanvas().AddDelayedDelete(this);
        }

        public override string ToString()
        {
            if (this is MenuItem)
                return "[MenuItem: " + (this as MenuItem).Text + "]";
            if (this is Label)
                return "[Label: " + (this as Label).Text + "]";
            if (this is ControlInternal.TextControl)
                return "[Text: " + (this as ControlInternal.TextControl).Text + "]";
            return GetType().ToString();
        }

        /// <summary>
        /// Gets the canvas (root parent) of the control.
        /// </summary>
        /// <returns></returns>
        public virtual Canvas GetCanvas()
        {
            ControlBase canvas = parent;
            if (canvas == null)
                throw new InvalidOperationException("Could not retrieve canvas because m_Parent was null");

            return canvas.GetCanvas();
        }

        /// <summary>
        /// Enables the control.
        /// </summary>
        public void Enable()
        {
            IsDisabled = false;
        }

        /// <summary>
        /// Disables the control.
        /// </summary>
        public virtual void Disable()
        {
            IsDisabled = true;
        }

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        /// <param name="control">Event source.</param>
        private void defaultAcceleratorHandler(ControlBase control, EventArgs args)
        {
            onAccelerator();
        }

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        protected virtual void onAccelerator()
        {

        }

        /// <summary>
        /// Hides the control.
        /// </summary>
        public virtual void Hide()
        {
            IsHidden = true;
        }

        /// <summary>
        /// Shows the control.
        /// </summary>
        public virtual void Show()
        {
            IsHidden = false;
        }

        /// <summary>
        /// Creates a tooltip for the control.
        /// </summary>
        /// <param name="text">Tooltip text.</param>
        public virtual void SetToolTipText(string text)
        {
            Label tooltip = new Label(this);
            tooltip.Text = text;
            tooltip.TextColorOverride = Skin.Colors.TooltipText;
            tooltip.Padding = new Padding(5, 3, 5, 3);
            tooltip.SizeToContents();

            ToolTip = tooltip;
        }

        /// <summary>
        /// Invalidates the control's children (relayout/repaint).
        /// </summary>
        /// <param name="recursive">Determines whether the operation should be carried recursively.</param>
        protected virtual void invalidateChildren(bool recursive = false)
        {
            foreach (ControlBase child in children)
            {
                child.Invalidate();
                if (recursive)
                    child.invalidateChildren(true);
            }

            if (innerPanel != null)
            {
                foreach (ControlBase child in innerPanel.children)
                {
                    child.Invalidate();
                    if (recursive)
                        child.invalidateChildren(true);
                }
            }
        }

        /// <summary>
        /// Invalidates the control.
        /// </summary>
        /// <remarks>
        /// Causes layout, repaint, invalidates cached texture.
        /// </remarks>
        public virtual void Invalidate()
        {
            needsLayout = true;
            cacheTextureDirty = true;
        }

        /// <summary>
        /// Sends the control to the bottom of paren't visibility stack.
        /// </summary>
        public virtual void SendToBack()
        {
            if (actualParent == null)
                return;
            if (actualParent.children.Count == 0)
                return;
            if (actualParent.children.First() == this)
                return;

            actualParent.children.Remove(this);
            actualParent.children.Insert(0, this);

            InvalidateParent();
        }

        /// <summary>
        /// Brings the control to the top of paren't visibility stack.
        /// </summary>
        public virtual void BringToFront()
        {
            if (actualParent == null)
                return;
            if (actualParent.children.Last() == this)
                return;

            actualParent.children.Remove(this);
            actualParent.children.Add(this);
            InvalidateParent();
            Redraw();
        }

        public virtual void BringNextToControl(ControlBase child, bool behind)
        {
            if (null == actualParent)
                return;

            actualParent.children.Remove(this);

            // todo: validate
            int idx = actualParent.children.IndexOf(child);
            if (idx == actualParent.children.Count - 1)
            {
                BringToFront();
                return;
            }

            if (behind)
            {
                ++idx;

                if (idx == actualParent.children.Count - 1)
                {
                    BringToFront();
                    return;
                }
            }

            actualParent.children.Insert(idx, this);
            InvalidateParent();
        }

        /// <summary>
        /// Finds a child by name.
        /// </summary>
        /// <param name="name">Child name.</param>
        /// <param name="recursive">Determines whether the search should be recursive.</param>
        /// <returns>Found control or null.</returns>
        public virtual ControlBase FindChildByName(string name, bool recursive = false)
        {
            ControlBase b = children.Find(x => x.name == name);
            if (b != null)
                return b;

            if (recursive)
            {
                foreach (ControlBase child in children)
                {
                    b = child.FindChildByName(name, true);
                    if (b != null)
                        return b;
                }
            }
            return null;
        }

        /// <summary>
        /// Attaches specified control as a child of this one.
        /// </summary>
        /// <remarks>
        /// If InnerPanel is not null, it will become the parent.
        /// </remarks>
        /// <param name="child">Control to be added as a child.</param>
        public virtual void AddChild(ControlBase child)
        {
            if (innerPanel != null)
            {
                innerPanel.AddChild(child);
            }
            else
            {
                children.Add(child);
                child.actualParent = this;
            }
            onChildAdded(child);
        }

        /// <summary>
        /// Detaches specified control from this one.
        /// </summary>
        /// <param name="child">Child to be removed.</param>
        /// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
        public virtual void RemoveChild(ControlBase child, bool dispose)
        {
            // If we removed our innerpanel
            // remove our pointer to it
            if (innerPanel == child)
            {
                children.Remove(innerPanel);
                innerPanel.DelayedDelete();
                innerPanel = null;
                return;
            }

            if (innerPanel != null && innerPanel.Children.Contains(child))
            {
                innerPanel.RemoveChild(child, dispose);
                return;
            }

            children.Remove(child);
            onChildRemoved(child);

            if (dispose)
                child.DelayedDelete();
        }

        /// <summary>
        /// Removes all children (and disposes them).
        /// </summary>
        public virtual void DeleteAllChildren()
        {
            // todo: probably shouldn't invalidate after each removal
            while (children.Count > 0)
                RemoveChild(children[0], true);
        }

        /// <summary>
        /// Handler invoked when a child is added.
        /// </summary>
        /// <param name="child">Child added.</param>
        protected virtual void onChildAdded(ControlBase child)
        {
            Invalidate();
        }

        /// <summary>
        /// Handler invoked when a child is removed.
        /// </summary>
        /// <param name="child">Child removed.</param>
        protected virtual void onChildRemoved(ControlBase child)
        {
            Invalidate();
        }

        /// <summary>
        /// Moves the control by a specific amount.
        /// </summary>
        /// <param name="x">X-axis movement.</param>
        /// <param name="y">Y-axis movement.</param>
        public virtual void MoveBy(int x, int y)
        {
            SetBounds(X + x, Y + y, Width, Height);
        }

        /// <summary>
        /// Moves the control to a specific point.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void MoveTo(float x, float y)
        {
            MoveTo((int)x, (int)y);
        }

        /// <summary>
        /// Moves the control to a specific point, clamping on paren't bounds if RestrictToParent is set.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void MoveTo(int x, int y)
        {
            if (RestrictToParent && (Parent != null))
            {
                ControlBase parent = Parent;
                if (x - Padding.Left < parent.Margin.Left)
                    x = parent.Margin.Left + Padding.Left;
                if (y - Padding.Top < parent.Margin.Top)
                    y = parent.Margin.Top + Padding.Top;
                if (x + Width + Padding.Right > parent.Width - parent.Margin.Right)
                    x = parent.Width - parent.Margin.Right - Width - Padding.Right;
                if (y + Height + Padding.Bottom > parent.Height - parent.Margin.Bottom)
                    y = parent.Height - parent.Margin.Bottom - Height - Padding.Bottom;
            }

            SetBounds(x, y, Width, Height);
        }

        /// <summary>
        /// Sets the control position.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void SetPosition(float x, float y)
        {
            SetPosition((int)x, (int)y);
        }

        /// <summary>
        /// Sets the control position.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void SetPosition(int x, int y)
        {
            SetBounds(x, y, Width, Height);
        }

        /// <summary>
        /// Sets the control size.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SetSize(int width, int height)
        {
            return SetBounds(X, Y, width, height);
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="bounds">New bounds.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SetBounds(Rectangle bounds)
        {
            return SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public virtual bool SetBounds(float x, float y, float width, float height)
        {
            return SetBounds((int)x, (int)y, (int)width, (int)height);
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public virtual bool SetBounds(int x, int y, int width, int height)
        {
            if (bounds.X == x &&
                bounds.Y == y &&
                bounds.Width == width &&
                bounds.Height == height)
                return false;

            Rectangle oldBounds = Bounds;

            bounds.X = x;
            bounds.Y = y;

            bounds.Width = width;
            bounds.Height = height;

            onBoundsChanged(oldBounds);

            if (BoundsChanged != null)
                BoundsChanged.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Positions the control inside its parent.
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public virtual void Position(Pos pos, int xpadding = 0, int ypadding = 0) // todo: a bit ambiguous name
        {
            int w = Parent.Width;
            int h = Parent.Height;
            Padding padding = Parent.Padding;

            int x = X;
            int y = Y;
            if (0 != (pos & Pos.Left))
                x = padding.Left + xpadding;
            if (0 != (pos & Pos.Right))
                x = w - Width - padding.Right - xpadding;
            if (0 != (pos & Pos.CenterH))
                x = (int)(padding.Left + xpadding + (w - Width - padding.Left - padding.Right) * 0.5f);

            if (0 != (pos & Pos.Top))
                y = padding.Top + ypadding;
            if (0 != (pos & Pos.Bottom))
                y = h - Height - padding.Bottom - ypadding;
            if (0 != (pos & Pos.CenterV))
                y = (int)(padding.Top + ypadding + (h - Height - padding.Bottom - padding.Top) * 0.5f);

            SetPosition(x, y);
        }

        /// <summary>
        /// Handler invoked when control's bounds change.
        /// </summary>
        /// <param name="oldBounds">Old bounds.</param>
        protected virtual void onBoundsChanged(Rectangle oldBounds)
        {
            //Anything that needs to update on size changes
            //Iterate my children and tell them I've changed
            //
            if (Parent != null)
                Parent.onChildBoundsChanged(oldBounds, this);


            if (bounds.Width != oldBounds.Width || bounds.Height != oldBounds.Height)
            {
                Invalidate();
            }

            Redraw();
            updateRenderBounds();
        }

        /// <summary>
        /// Handler invoked when control's scale changes.
        /// </summary>
        protected virtual void onScaleChanged()
        {
            foreach (ControlBase child in children)
            {
                child.onScaleChanged();
            }
        }

        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        protected virtual void onChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
        {

        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void render(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Renders the control to a cache using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        /// <param name="master">Root parent.</param>
        protected virtual void doCacheRender(Skin.SkinBase skin, ControlBase master)
        {
            Renderer.RendererBase renderer = skin.Renderer;
            Renderer.ICacheToTexture cache = renderer.CTT;

            if (cache == null)
                return;

            Point oldRenderOffset = renderer.RenderOffset;
            Rectangle oldRegion = renderer.ClipRegion;

            if (this != master)
            {
                renderer.AddRenderOffset(Bounds);
                renderer.AddClipRegion(Bounds);
            }
            else
            {
                renderer.RenderOffset = Point.Empty;
                renderer.ClipRegion = new Rectangle(0, 0, Width, Height);
            }

            if (cacheTextureDirty && renderer.ClipRegionVisible)
            {
                renderer.StartClip();

                if (ShouldCacheToTexture)
                    cache.SetupCacheTexture(this);

                //render myself first
                //var old = renderer.ClipRegion;
                //renderer.ClipRegion = Bounds;
                //var old = renderer.RenderOffset;
                //renderer.RenderOffset = new Point(Bounds.X, Bounds.Y);
                render(skin);
                //renderer.RenderOffset = old;
                //renderer.ClipRegion = old;

                if (children.Count > 0)
                {
                    //Now render my kids
                    foreach (ControlBase child in children)
                    {
                        if (child.IsHidden)
                            continue;
                        child.doCacheRender(skin, master);
                    }
                }

                if (ShouldCacheToTexture)
                {
                    cache.FinishCacheTexture(this);
                    cacheTextureDirty = false;
                }
            }

            renderer.ClipRegion = oldRegion;
            renderer.StartClip();
            renderer.RenderOffset = oldRenderOffset;

            if (ShouldCacheToTexture)
                cache.DrawCachedControlTexture(this);
        }

        /// <summary>
        /// Rendering logic implementation.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        internal virtual void DoRender(Skin.SkinBase skin)
        {
            // If this control has a different skin,
            // then so does its children.
            if (skin != null)
                this.skin = skin;

            // Do think
            Think();

            Renderer.RendererBase render = skin.Renderer;

            if (render.CTT != null && ShouldCacheToTexture)
            {
                doCacheRender(skin, this);
                return;
            }

            renderRecursive(skin, Bounds);

            if (DrawDebugOutlines)
                skin.DrawDebugOutlines(this);
        }

        /// <summary>
        /// Recursive rendering logic.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        /// <param name="clipRect">Clipping rectangle.</param>
        protected virtual void renderRecursive(Skin.SkinBase skin, Rectangle clipRect)
        {
            Renderer.RendererBase renderer = skin.Renderer;
            Point oldRenderOffset = renderer.RenderOffset;

            renderer.AddRenderOffset(clipRect);

            renderUnder(skin);

            Rectangle oldRegion = renderer.ClipRegion;

            if (shouldClip)
            {
                renderer.AddClipRegion(clipRect);

                if (!renderer.ClipRegionVisible)
                {
                    renderer.RenderOffset = oldRenderOffset;
                    renderer.ClipRegion = oldRegion;
                    return;
                }

                renderer.StartClip();
            }

            //render myself first
            render(skin);

            if (children.Count > 0)
            {
                //Now render my kids
                foreach (ControlBase child in children)
                {
                    if (child.IsHidden)
                        continue;
                    child.DoRender(skin);
                }
            }

            renderer.ClipRegion = oldRegion;
            renderer.StartClip();
            renderOver(skin);

            renderFocus(skin);

            renderer.RenderOffset = oldRenderOffset;
        }

        /// <summary>
        /// Sets the control's skin.
        /// </summary>
        /// <param name="skin">New skin.</param>
        /// <param name="doChildren">Deterines whether to change children skin.</param>
        public virtual void SetSkin(Skin.SkinBase skin, bool doChildren = false)
        {
            if (this.skin == skin)
                return;
            this.skin = skin;
            Invalidate();
            Redraw();
            onSkinChanged(skin);

            if (doChildren)
            {
                foreach (ControlBase child in children)
                {
                    child.SetSkin(skin, true);
                }
            }
        }

        /// <summary>
        /// Handler invoked when control's skin changes.
        /// </summary>
        /// <param name="newSkin">New skin.</param>
        protected virtual void onSkinChanged(Skin.SkinBase newSkin)
        {

        }

        /// <summary>
        /// Handler invoked on mouse wheel event.
        /// </summary>
        /// <param name="delta">Scroll delta.</param>
        protected virtual bool onMouseWheeled(int delta)
        {
            if (actualParent != null)
                return actualParent.onMouseWheeled(delta);

            return false;
        }

        /// <summary>
        /// Invokes mouse wheeled event (used by input system).
        /// </summary>
        internal bool InputMouseWheeled(int delta)
        {
            return onMouseWheeled(delta);
        }

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected virtual void onMouseMoved(int x, int y, int dx, int dy)
        {

        }

        /// <summary>
        /// Invokes mouse moved event (used by input system).
        /// </summary>
        internal void InputMouseMoved(int x, int y, int dx, int dy)
        {
            onMouseMoved(x, y, dx, dy);
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected virtual void onMouseClickedLeft(int x, int y, bool down)
        {
            if (down && Clicked != null)
                Clicked(this, new ClickedEventArgs(x, y, down));
        }

        /// <summary>
        /// Invokes left mouse click event (used by input system).
        /// </summary>
        internal void InputMouseClickedLeft(int x, int y, bool down)
        {
            onMouseClickedLeft(x, y, down);
        }

        /// <summary>
        /// Handler invoked on mouse click (right) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected virtual void onMouseClickedRight(int x, int y, bool down)
        {
            if (down && RightClicked != null)
                RightClicked(this, new ClickedEventArgs(x, y, down));
        }

        /// <summary>
        /// Invokes right mouse click event (used by input system).
        /// </summary>
        internal void InputMouseClickedRight(int x, int y, bool down)
        {
            onMouseClickedRight(x, y, down);
        }

        /// <summary>
        /// Handler invoked on mouse double click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected virtual void onMouseDoubleClickedLeft(int x, int y)
        {
            // [omeg] should this be called?
            // [halfofastaple] Maybe. Technically, a double click is still technically a single click. However, this shouldn't be called here, and
            //					Should be called by the event handler.
            onMouseClickedLeft(x, y, true);

            if (DoubleClicked != null)
                DoubleClicked(this, new ClickedEventArgs(x, y, true));
        }

        /// <summary>
        /// Invokes left double mouse click event (used by input system).
        /// </summary>
        internal void InputMouseDoubleClickedLeft(int x, int y)
        {
            onMouseDoubleClickedLeft(x, y);
        }

        /// <summary>
        /// Handler invoked on mouse double click (right) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected virtual void onMouseDoubleClickedRight(int x, int y)
        {
            // [halfofastaple] See: OnMouseDoubleClicked for discussion on triggering single clicks in a double click event
            onMouseClickedRight(x, y, true);

            if (DoubleRightClicked != null)
                DoubleRightClicked(this, new ClickedEventArgs(x, y, true));
        }

        /// <summary>
        /// Invokes right double mouse click event (used by input system).
        /// </summary>
        internal void InputMouseDoubleClickedRight(int x, int y)
        {
            onMouseDoubleClickedRight(x, y);
        }

        /// <summary>
        /// Handler invoked on mouse cursor entering control's bounds.
        /// </summary>
        protected virtual void onMouseEntered()
        {
            if (HoverEnter != null)
                HoverEnter.Invoke(this, EventArgs.Empty);

            if (ToolTip != null)
                Gwen.ToolTip.Enable(this);
            else if (Parent != null && Parent.ToolTip != null)
                Gwen.ToolTip.Enable(Parent);

            Redraw();
        }

        /// <summary>
        /// Invokes mouse enter event (used by input system).
        /// </summary>
        internal void InputMouseEntered()
        {
            onMouseEntered();
        }

        /// <summary>
        /// Handler invoked on mouse cursor leaving control's bounds.
        /// </summary>
        protected virtual void onMouseLeft()
        {
            if (HoverLeave != null)
                HoverLeave.Invoke(this, EventArgs.Empty);

            if (ToolTip != null)
                Gwen.ToolTip.Disable(this);

            Redraw();
        }

        /// <summary>
        /// Invokes mouse leave event (used by input system).
        /// </summary>
        internal void InputMouseLeft()
        {
            onMouseLeft();
        }

        /// <summary>
        /// Focuses the control.
        /// </summary>
        public virtual void Focus()
        {
            if (InputHandler.KeyboardFocus == this)
                return;

            if (InputHandler.KeyboardFocus != null)
                InputHandler.KeyboardFocus.onLostKeyboardFocus();

            InputHandler.KeyboardFocus = this;
            onKeyboardFocus();
            Redraw();
        }

        /// <summary>
        /// Unfocuses the control.
        /// </summary>
        public virtual void Blur()
        {
            if (InputHandler.KeyboardFocus != this)
                return;

            InputHandler.KeyboardFocus = null;
            onLostKeyboardFocus();
            Redraw();
        }

        /// <summary>
        /// Control has been clicked - invoked by input system. Windows use it to propagate activation.
        /// </summary>
        public virtual void Touch()
        {
            if (Parent != null)
                Parent.onChildTouched(this);
        }

        protected virtual void onChildTouched(ControlBase control)
        {
            Touch();
        }

        /// <summary>
        /// Gets a child by its coordinates.
        /// </summary>
        /// <param name="x">Child X.</param>
        /// <param name="y">Child Y.</param>
        /// <returns>Control or null if not found.</returns>
        public virtual ControlBase GetControlAt(int x, int y)
        {
            if (IsHidden)
                return null;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;

            // todo: convert to linq FindLast
            var rev = ((IList<ControlBase>)children).Reverse(); // IList.Reverse creates new list, List.Reverse works in place.. go figure
            foreach (ControlBase child in rev)
            {
                ControlBase found = child.GetControlAt(x - child.X, y - child.Y);
                if (found != null)
                    return found;
            }

            if (!MouseInputEnabled)
                return null;

            return this;
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void layout(Skin.SkinBase skin)
        {
            if (skin.Renderer.CTT != null && ShouldCacheToTexture)
                skin.Renderer.CTT.CreateControlCacheTexture(this);
        }

        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void recurseLayout(Skin.SkinBase skin)
        {
            if (skin != null)
                this.skin = skin;
            if (IsHidden)
                return;

            if (needsLayout)
            {
                needsLayout = false;
                layout(skin);
            }

            Rectangle bounds = RenderBounds;

            // Adjust bounds for padding
            bounds.X += padding.Left;
            bounds.Width -= padding.Left + padding.Right;
            bounds.Y += padding.Top;
            bounds.Height -= padding.Top + padding.Bottom;

            foreach (ControlBase child in children)
            {
                if (child.IsHidden)
                    continue;

                Pos dock = child.Dock;

                if (0 != (dock & Pos.Fill))
                    continue;

                if (0 != (dock & Pos.Top))
                {
                    Margin margin = child.Margin;

                    child.SetBounds(bounds.X + margin.Left, bounds.Y + margin.Top,
                        bounds.Width - margin.Left - margin.Right, child.Height);

                    int height = margin.Top + margin.Bottom + child.Height;
                    bounds.Y += height;
                    bounds.Height -= height;
                }

                if (0 != (dock & Pos.Left))
                {
                    Margin margin = child.Margin;

                    child.SetBounds(bounds.X + margin.Left, bounds.Y + margin.Top, child.Width,
                        bounds.Height - margin.Top - margin.Bottom);

                    int width = margin.Left + margin.Right + child.Width;
                    bounds.X += width;
                    bounds.Width -= width;
                }

                if (0 != (dock & Pos.Right))
                {
                    // TODO: THIS MARGIN CODE MIGHT NOT BE FULLY FUNCTIONAL
                    Margin margin = child.Margin;

                    child.SetBounds((bounds.X + bounds.Width) - child.Width - margin.Right, bounds.Y + margin.Top,
                        child.Width, bounds.Height - margin.Top - margin.Bottom);

                    int width = margin.Left + margin.Right + child.Width;
                    bounds.Width -= width;
                }

                if (0 != (dock & Pos.Bottom))
                {
                    // TODO: THIS MARGIN CODE MIGHT NOT BE FULLY FUNCTIONAL
                    Margin margin = child.Margin;

                    child.SetBounds(bounds.X + margin.Left,
                        (bounds.Y + bounds.Height) - child.Height - margin.Bottom,
                        bounds.Width - margin.Left - margin.Right, child.Height);
                    bounds.Height -= child.Height + margin.Bottom + margin.Top;
                }

                child.recurseLayout(skin);
            }

            innerBounds = bounds;

            //
            // Fill uses the left over space, so do that now.
            //
            foreach (ControlBase child in children)
            {
                Pos dock = child.Dock;

                if (!(0 != (dock & Pos.Fill)))
                    continue;

                Margin margin = child.Margin;

                child.SetBounds(bounds.X + margin.Left, bounds.Y + margin.Top,
                    bounds.Width - margin.Left - margin.Right, bounds.Height - margin.Top - margin.Bottom);
                child.recurseLayout(skin);
            }

            postLayout(skin);

            if (IsTabable)
            {
                if (GetCanvas().FirstTab == null)
                    GetCanvas().FirstTab = this;
                if (GetCanvas().NextTab == null)
                    GetCanvas().NextTab = this;
            }

            if (InputHandler.KeyboardFocus == this)
            {
                GetCanvas().NextTab = null;
            }
        }

        /// <summary>
        /// Checks if the given control is a child of this instance.
        /// </summary>
        /// <param name="child">Control to examine.</param>
        /// <returns>True if the control is out child.</returns>
        public bool IsChild(ControlBase child)
        {
            return children.Contains(child);
        }

        /// <summary>
        /// Converts local coordinates to canvas coordinates.
        /// </summary>
        /// <param name="pnt">Local coordinates.</param>
        /// <returns>Canvas coordinates.</returns>
        public virtual Point LocalPosToCanvas(Point pnt)
        {
            if (parent != null)
            {
                int x = pnt.X + X;
                int y = pnt.Y + Y;

                // If our parent has an innerpanel and we're a child of it
                // add its offset onto us.
                //
                if (parent.innerPanel != null && parent.innerPanel.IsChild(this))
                {
                    x += parent.innerPanel.X;
                    y += parent.innerPanel.Y;
                }

                return parent.LocalPosToCanvas(new Point(x, y));
            }

            return pnt;
        }

        /// <summary>
        /// Converts canvas coordinates to local coordinates.
        /// </summary>
        /// <param name="pnt">Canvas coordinates.</param>
        /// <returns>Local coordinates.</returns>
        public virtual Point CanvasPosToLocal(Point pnt)
        {
            if (parent != null)
            {
                int x = pnt.X - X;
                int y = pnt.Y - Y;

                // If our parent has an innerpanel and we're a child of it
                // add its offset onto us.
                //
                if (parent.innerPanel != null && parent.innerPanel.IsChild(this))
                {
                    x -= parent.innerPanel.X;
                    y -= parent.innerPanel.Y;
                }


                return parent.CanvasPosToLocal(new Point(x, y));
            }

            return pnt;
        }

        /// <summary>
        /// Closes all menus recursively.
        /// </summary>
        public virtual void CloseMenus()
        {
            //Debug.Print("Base.CloseMenus: {0}", this);

            // todo: not very efficient with the copying and recursive closing, maybe store currently open menus somewhere (canvas)?
            var childrenCopy = children.FindAll(x => true);
            foreach (ControlBase child in childrenCopy)
            {
                child.CloseMenus();
            }
        }

        /// <summary>
        /// Copies Bounds to RenderBounds.
        /// </summary>
        protected virtual void updateRenderBounds()
        {
            renderBounds.X = 0;
            renderBounds.Y = 0;

            renderBounds.Width = bounds.Width;
            renderBounds.Height = bounds.Height;
        }

        /// <summary>
        /// Sets mouse cursor to current cursor.
        /// </summary>
        public virtual void UpdateCursor()
        {
            Platform.Neutral.SetCursor(cursor);
        }

        // giver
        public virtual Package DragAndDrop_GetPackage(int x, int y)
        {
            return dragAndDrop_Package;
        }

        // giver
        public virtual bool DragAndDrop_Draggable()
        {
            if (dragAndDrop_Package == null)
                return false;

            return dragAndDrop_Package.IsDraggable;
        }

        // giver
        public virtual void DragAndDrop_SetPackage(bool draggable, string name = "", object userData = null)
        {
            if (dragAndDrop_Package == null)
            {
                dragAndDrop_Package = new Package();
                dragAndDrop_Package.IsDraggable = draggable;
                dragAndDrop_Package.Name = name;
                dragAndDrop_Package.UserData = userData;
            }
        }

        // giver
        public virtual bool DragAndDrop_ShouldStartDrag()
        {
            return true;
        }

        // giver
        public virtual void DragAndDrop_StartDragging(Package package, int x, int y)
        {
            package.HoldOffset = CanvasPosToLocal(new Point(x, y));
            package.DrawControl = this;
        }

        // giver
        public virtual void DragAndDrop_EndDragging(bool success, int x, int y)
        {
        }

        // receiver
        public virtual bool DragAndDrop_HandleDrop(Package p, int x, int y)
        {
            DragAndDrop.SourceControl.Parent = this;
            return true;
        }

        // receiver
        public virtual void DragAndDrop_HoverEnter(Package p, int x, int y)
        {

        }

        // receiver
        public virtual void DragAndDrop_HoverLeave(Package p)
        {

        }

        // receiver
        public virtual void DragAndDrop_Hover(Package p, int x, int y)
        {

        }

        // receiver
        public virtual bool DragAndDrop_CanAcceptPackage(Package p)
        {
            return false;
        }

        /// <summary>
        /// Resizes the control to fit its children.
        /// </summary>
        /// <param name="width">Determines whether to change control's width.</param>
        /// <param name="height">Determines whether to change control's height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SizeToChildren(bool width = true, bool height = true)
        {
            Point size = GetChildrenSize();
            size.X += Padding.Right;
            size.Y += Padding.Bottom;
            return SetSize(width ? size.X : Width, height ? size.Y : Height);
        }

        /// <summary>
        /// Returns the total width and height of all children.
        /// </summary>
        /// <remarks>Default implementation returns maximum size of children since the layout is unknown.
        /// Implement this in derived compound controls to properly return their size.</remarks>
        /// <returns></returns>
        public virtual Point GetChildrenSize()
        {
            Point size = Point.Empty;

            foreach (ControlBase child in children)
            {
                if (child.IsHidden)
                    continue;

                size.X = Math.Max(size.X, child.Right);
                size.Y = Math.Max(size.Y, child.Bottom);
            }

            return size;
        }

        /// <summary>
        /// Handles keyboard accelerator.
        /// </summary>
        /// <param name="accelerator">Accelerator text.</param>
        /// <returns>True if handled.</returns>
        internal virtual bool HandleAccelerator(string accelerator)
        {
            if (InputHandler.KeyboardFocus == this || !accelOnlyFocus)
            {
                if (accelerators.ContainsKey(accelerator))
                {
                    accelerators[accelerator].Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            return children.Any(child => child.HandleAccelerator(accelerator));
        }

        /// <summary>
        /// Adds keyboard accelerator.
        /// </summary>
        /// <param name="accelerator">Accelerator text.</param>
        /// <param name="handler">Handler.</param>
        public void AddAccelerator(string accelerator, GwenEventHandler<EventArgs> handler)
        {
            accelerator = accelerator.Trim().ToUpperInvariant();
            accelerators[accelerator] = handler;
        }

        /// <summary>
        /// Adds keyboard accelerator with a default handler.
        /// </summary>
        /// <param name="accelerator">Accelerator text.</param>
        public void AddAccelerator(string accelerator)
        {
            accelerators[accelerator] = defaultAcceleratorHandler;
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void postLayout(Skin.SkinBase skin)
        {

        }

        /// <summary>
        /// Re-renders the control, invalidates cached texture.
        /// </summary>
        public virtual void Redraw()
        {
            UpdateColors();
            cacheTextureDirty = true;
            if (parent != null)
                parent.Redraw();
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        /// <remarks>
        /// Used in composite controls like lists to differentiate row colors etc.
        /// </remarks>
        public virtual void UpdateColors()
        {

        }

        /// <summary>
        /// Invalidates control's parent.
        /// </summary>
        public void InvalidateParent()
        {
            if (parent != null)
            {
                parent.Invalidate();
            }
        }

        /// <summary>
        /// Handler for keyboard events.
        /// </summary>
        /// <param name="key">Key pressed.</param>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyPressed(Key key, bool down = true)
        {
            bool handled = false;
            switch (key)
            {
                case Key.Tab:
                    handled = onKeyTab(down);
                    break;
                case Key.Space:
                    handled = onKeySpace(down);
                    break;
                case Key.Home:
                    handled = onKeyHome(down);
                    break;
                case Key.End:
                    handled = onKeyEnd(down);
                    break;
                case Key.Return:
                    handled = onKeyReturn(down);
                    break;
                case Key.Backspace:
                    handled = onKeyBackspace(down);
                    break;
                case Key.Delete:
                    handled = onKeyDelete(down);
                    break;
                case Key.Right:
                    handled = onKeyRight(down);
                    break;
                case Key.Left:
                    handled = onKeyLeft(down);
                    break;
                case Key.Up:
                    handled = onKeyUp(down);
                    break;
                case Key.Down:
                    handled = onKeyDown(down);
                    break;
                case Key.Escape:
                    handled = onKeyEscape(down);
                    break;
                default:
                    break;
            }

            if (!handled && Parent != null)
                Parent.onKeyPressed(key, down);

            return handled;
        }

        /// <summary>
        /// Invokes key press event (used by input system).
        /// </summary>
        internal bool InputKeyPressed(Key key, bool down = true)
        {
            return onKeyPressed(key, down);
        }

        /// <summary>
        /// Handler for keyboard events.
        /// </summary>
        /// <param name="key">Key pressed.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyReleaseed(Key key)
        {
            return onKeyPressed(key, false);
        }

        /// <summary>
        /// Handler for Tab keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyTab(bool down)
        {
            if (!down)
                return true;

            if (GetCanvas().NextTab != null)
            {
                GetCanvas().NextTab.Focus();
                Redraw();
            }

            return true;
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeySpace(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Return keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyReturn(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Backspace keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyBackspace(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Delete keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyDelete(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyRight(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyLeft(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Home keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyHome(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for End keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyEnd(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyUp(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Down Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyDown(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Escape keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onKeyEscape(bool down)
        {
            return false;
        }

        /// <summary>
        /// Handler for Paste event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void onPaste(ControlBase from, EventArgs args)
        {
        }

        /// <summary>
        /// Handler for Copy event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void onCopy(ControlBase from, EventArgs args)
        {
        }

        /// <summary>
        /// Handler for Cut event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void onCut(ControlBase from, EventArgs args)
        {
        }

        /// <summary>
        /// Handler for Select All event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void onSelectAll(ControlBase from, EventArgs args)
        {
        }

        internal void InputCopy(ControlBase from)
        {
            onCopy(from, EventArgs.Empty);
        }

        internal void InputPaste(ControlBase from)
        {
            onPaste(from, EventArgs.Empty);
        }

        internal void InputCut(ControlBase from)
        {
            onCut(from, EventArgs.Empty);
        }

        internal void InputSelectAll(ControlBase from)
        {
            onSelectAll(from, EventArgs.Empty);
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void renderFocus(Skin.SkinBase skin)
        {
            if (InputHandler.KeyboardFocus != this)
                return;
            if (!IsTabable)
                return;

            skin.DrawKeyboardHighlight(this, RenderBounds, 3);
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void renderUnder(Skin.SkinBase skin)
        {

        }

        /// <summary>
        /// Renders over the actual control (overlays).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void renderOver(Skin.SkinBase skin)
        {

        }

        /// <summary>
        /// Called during rendering.
        /// </summary>
        public virtual void Think()
        {

        }

        /// <summary>
        /// Handler for gaining keyboard focus.
        /// </summary>
        protected virtual void onKeyboardFocus()
        {

        }

        /// <summary>
        /// Handler for losing keyboard focus.
        /// </summary>
        protected virtual void onLostKeyboardFocus()
        {

        }

        /// <summary>
        /// Handler for character input event.
        /// </summary>
        /// <param name="chr">Character typed.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool onChar(Char chr)
        {
            return false;
        }

        internal bool InputChar(Char chr)
        {
            return onChar(chr);
        }

        public virtual void Anim_WidthIn(float length, float delay = 0.0f, float ease = 1.0f)
        {
            Animation.Add(this, new Anim.Size.Width(0, Width, length, false, delay, ease));
            Width = 0;
        }

        public virtual void Anim_HeightIn(float length, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Height(0, Height, length, false, delay, ease));
            Height = 0;
        }

        public virtual void Anim_WidthOut(float length, bool hide, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Width(Width, 0, length, hide, delay, ease));
        }

        public virtual void Anim_HeightOut(float length, bool hide, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Height(Height, 0, length, hide, delay, ease));
        }

        public void FitChildrenToSize()
        {
            foreach (ControlBase child in Children)
            {
                //push them back into view if they are outside it
                child.X = Math.Min(Bounds.Width, child.X + child.Width) - child.Width;
                child.Y = Math.Min(Bounds.Height, child.Y + child.Height) - child.Height;

                //Non-negative has priority, so do it second.
                child.X = Math.Max(0, child.X);
                child.Y = Math.Max(0, child.Y);
            }
        }
    }
}
