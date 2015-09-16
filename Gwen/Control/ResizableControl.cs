using System;
using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Base resizable control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ResizableControl : ControlBase
    {
        private bool clampMovement;
        private readonly Resizer[] resizer;

        /// <summary>
        /// Determines whether control's position should be restricted to its parent bounds.
        /// </summary>
        public bool ClampMovement { get { return clampMovement; } set { clampMovement = value; } }

        /// <summary>
        /// Invoked when the control has been resized.
        /// </summary>
		public event GwenEventHandler<EventArgs> Resized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizableControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ResizableControl(ControlBase parent)
            : base(parent)
        {
            resizer = new Resizer[10];
            MinimumSize = new Point(5, 5);
            clampMovement = false;

            resizer[2] = new Resizer(this);
            resizer[2].Dock = Pos.Bottom;
            resizer[2].ResizeDir = Pos.Bottom;
            resizer[2].Resized += onResized;
            resizer[2].Target = this;

            resizer[1] = new Resizer(resizer[2]);
            resizer[1].Dock = Pos.Left;
            resizer[1].ResizeDir = Pos.Bottom | Pos.Left;
            resizer[1].Resized += onResized;
            resizer[1].Target = this;

            resizer[3] = new Resizer(resizer[2]);
            resizer[3].Dock = Pos.Right;
            resizer[3].ResizeDir = Pos.Bottom | Pos.Right;
            resizer[3].Resized += onResized;
            resizer[3].Target = this;

            resizer[8] = new Resizer(this);
            resizer[8].Dock = Pos.Top;
            resizer[8].ResizeDir = Pos.Top;
            resizer[8].Resized += onResized;
            resizer[8].Target = this;

            resizer[7] = new Resizer(resizer[8]);
            resizer[7].Dock = Pos.Left;
            resizer[7].ResizeDir = Pos.Left | Pos.Top;
            resizer[7].Resized += onResized;
            resizer[7].Target = this;

            resizer[9] = new Resizer(resizer[8]);
            resizer[9].Dock = Pos.Right;
            resizer[9].ResizeDir = Pos.Right | Pos.Top;
            resizer[9].Resized += onResized;
            resizer[9].Target = this;

            resizer[4] = new Resizer(this);
            resizer[4].Dock = Pos.Left;
            resizer[4].ResizeDir = Pos.Left;
            resizer[4].Resized += onResized;
            resizer[4].Target = this;

            resizer[6] = new Resizer(this);
            resizer[6].Dock = Pos.Right;
            resizer[6].ResizeDir = Pos.Right;
            resizer[6].Resized += onResized;
            resizer[6].Target = this;
        }

        /// <summary>
        /// Handler for the resized event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onResized(ControlBase control, EventArgs args)
        {
            if (Resized != null)
				Resized.Invoke(this, EventArgs.Empty);
        }

        protected Resizer getResizer(int i)
        {
            return resizer[i];
        }

        /// <summary>
        /// Disables resizing.
        /// </summary>
        public virtual void DisableResizing()
        {
            for (int i = 0; i < 10; i++)
            {
                if (resizer[i] == null)
                    continue;
                resizer[i].MouseInputEnabled = false;
                resizer[i].IsHidden = true;
                Padding = new Padding(resizer[i].Width, resizer[i].Width, resizer[i].Width, resizer[i].Width);
            }
        }

        /// <summary>
        /// Enables resizing.
        /// </summary>
        public void EnableResizing()
        {
            for (int i = 0; i < 10; i++)
            {
                if (resizer[i] == null)
                    continue;
                resizer[i].MouseInputEnabled = true;
                resizer[i].IsHidden = false;
                Padding = new Padding(0, 0, 0, 0); // todo: check if ok
            }
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
        public override bool SetBounds(int x, int y, int width, int height)
        {
            Point minSize = MinimumSize;
            // Clamp Minimum Size
            if (width < minSize.X) width = minSize.X;
            if (height < minSize.Y) height = minSize.Y;

            // Clamp to parent's window
            ControlBase parent = Parent;
            if (parent != null && clampMovement)
            {
                if (x + width > parent.Width) x = parent.Width - width;
                if (x < 0) x = 0;
                if (y + height > parent.Height) y = parent.Height - height;
                if (y < 0) y = 0;
            }

            return base.SetBounds(x, y, width, height);
        }

        /// <summary>
        /// Sets the control size.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <returns>True if bounds changed.</returns>
        public override bool SetSize(int width, int height) {
            bool Changed = base.SetSize(width, height);
            if (Changed) {
				onResized(this, EventArgs.Empty);
            }
            return Changed;
        }
    }
}
