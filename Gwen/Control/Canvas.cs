using System;
using System.Collections.Generic;
using System.Drawing;
using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Canvas control. It should be the root parent for all other controls.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Canvas : ControlBase
    {
        private bool needsRedraw;
        private float scale;

        private Color backgroundColor;

        // [omeg] these are not created by us, so no disposing
        internal ControlBase FirstTab;
        internal ControlBase NextTab;

        private readonly List<IDisposable> disposeQueue; // dictionary for faster access?

        /// <summary>
        /// Scale for rendering.
        /// </summary>
        [JsonProperty]
        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale == value)
                    return;

                scale = value;

                if (Skin != null && Skin.Renderer != null)
                    Skin.Renderer.Scale = scale;

                onScaleChanged();
                Redraw();
            }
        }

        /// <summary>
        /// Background color.
        /// </summary>
        public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; } }

        /// <summary>
        /// In most situations you will be rendering the canvas every frame.
        /// But in some situations you will only want to render when there have been changes.
        /// You can do this by checking NeedsRedraw.
        /// </summary>
        public bool NeedsRedraw { get { return needsRedraw; } set { needsRedraw = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        public Canvas(Skin.SkinBase skin)
        {
            SetBounds(0, 0, 10000, 10000);
            SetSkin(skin ?? Defaults.Skin);
            Scale = 1.0f;
            BackgroundColor = Color.White;
            ShouldDrawBackground = false;

            disposeQueue = new List<IDisposable>();
        }

        public override void Dispose()
        {
            processDelayedDeletes();
            base.Dispose();
        }

        /// <summary>
        /// Re-renders the control, invalidates cached texture.
        /// </summary>
        public override void Redraw()
        {
            NeedsRedraw = true;
            base.Redraw();
        }

        // Children call parent.GetCanvas() until they get to
        // this top level function.
        public override Canvas GetCanvas()
        {
            return this;
        }

        /// <summary>
        /// Additional initialization (which is sometimes not appropriate in the constructor)
        /// </summary>
        protected void initialize()
        {

        }

        /// <summary>
        /// Renders the canvas. Call in your rendering loop.
        /// </summary>
        public void RenderCanvas()
        {
            doThink();

            Renderer.RendererBase render = Skin.Renderer;

            render.Begin();

            recurseLayout(Skin);

            render.ClipRegion = Bounds;
            render.RenderOffset = Point.Empty;
            render.Scale = Scale;

            if (ShouldDrawBackground)
            {
                render.DrawColor = backgroundColor;
                render.DrawFilledRect(Bounds);
            }

            DoRender(Skin);

            DragAndDrop.RenderOverlay(this, Skin);

            Gwen.ToolTip.RenderToolTip(Skin);

            render.EndClip();

            render.End();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            //skin.Renderer.rnd = new Random(1);
            base.render(skin);
            needsRedraw = false;
        }

        /// <summary>
        /// Handler invoked when control's bounds change.
        /// </summary>
        /// <param name="oldBounds">Old bounds.</param>
        protected override void onBoundsChanged(Rectangle oldBounds)
        {
            base.onBoundsChanged(oldBounds);
            invalidateChildren(true);
        }

        /// <summary>
        /// Processes input and layout. Also purges delayed delete queue.
        /// </summary>
        private void doThink()
        {
            if (IsHidden)
                return;

            Animation.GlobalThink();

            // Reset tabbing
            NextTab = null;
            FirstTab = null;

            processDelayedDeletes();

            // Check has focus etc..
            recurseLayout(Skin);

            // If we didn't have a next tab, cycle to the start.
            if (NextTab == null)
                NextTab = FirstTab;

            InputHandler.OnCanvasThink(this);
        }

        /// <summary>
        /// Adds given control to the delete queue and detaches it from canvas. Don't call from Dispose, it modifies child list.
        /// </summary>
        /// <param name="control">Control to delete.</param>
        public void AddDelayedDelete(ControlBase control)
        {
            if (!disposeQueue.Contains(control))
            {
                disposeQueue.Add(control);
                RemoveChild(control, false);
            }
#if DEBUG
            else
                throw new InvalidOperationException("Control deleted twice");
#endif
        }

        private void processDelayedDeletes()
        {
            //if (disposeQueue.Count > 0)
            //    System.Diagnostics.Debug.Print("Canvas.processDelayedDeletes: {0} items", disposeQueue.Count);
            foreach (IDisposable control in disposeQueue)
            {
                control.Dispose();
            }
            disposeQueue.Clear();
        }

        /// <summary>
        /// Handles mouse movement events. Called from Input subsystems.
        /// </summary>
        /// <returns>True if handled.</returns>
        public bool Input_MouseMoved(int x, int y, int dx, int dy)
        {
            if (IsHidden)
                return false;

            // Todo: Handle scaling here..
            //float fScale = 1.0f / Scale();

            InputHandler.OnMouseMoved(this, x, y, dx, dy);

            if (InputHandler.HoveredControl == null) return false;
            if (InputHandler.HoveredControl == this) return false;
            if (InputHandler.HoveredControl.GetCanvas() != this) return false;

            InputHandler.HoveredControl.InputMouseMoved(x, y, dx, dy);
            InputHandler.HoveredControl.UpdateCursor();

            DragAndDrop.OnMouseMoved(InputHandler.HoveredControl, x, y);
            return true;
        }

        /// <summary>
        /// Handles mouse button events. Called from Input subsystems.
        /// </summary>
        /// <returns>True if handled.</returns>
        public bool Input_MouseButton(int button, bool down)
        {
            if (IsHidden) return false;

            return InputHandler.OnMouseClicked(this, button, down);
        }

        /// <summary>
        /// Handles keyboard events. Called from Input subsystems.
        /// </summary>
        /// <returns>True if handled.</returns>
        public bool Input_Key(Key key, bool down)
        {
            if (IsHidden) return false;
            if (key <= Key.Invalid) return false;
            if (key >= Key.Count) return false;

            return InputHandler.OnKeyEvent(this, key, down);
        }

        /// <summary>
        /// Handles keyboard events. Called from Input subsystems.
        /// </summary>
        /// <returns>True if handled.</returns>
        public bool Input_Character(char chr)
        {
            if (IsHidden) return false;
            if (char.IsControl(chr)) return false;

            //Handle Accelerators
            if (InputHandler.HandleAccelerator(this, chr))
                return true;

            //Handle characters
            if (InputHandler.KeyboardFocus == null) return false;
            if (InputHandler.KeyboardFocus.GetCanvas() != this) return false;
            if (!InputHandler.KeyboardFocus.IsVisible) return false;
            if (InputHandler.IsControlDown) return false;

            return InputHandler.KeyboardFocus.InputChar(chr);
        }

        /// <summary>
        /// Handles the mouse wheel events. Called from Input subsystems.
        /// </summary>
        /// <returns>True if handled.</returns>
        public bool Input_MouseWheel(int val)
        {
            if (IsHidden) return false;
            if (InputHandler.HoveredControl == null) return false;
            if (InputHandler.HoveredControl == this) return false;
            if (InputHandler.HoveredControl.GetCanvas() != this) return false;

            return InputHandler.HoveredControl.InputMouseWheeled(val);
        }
    }
}
