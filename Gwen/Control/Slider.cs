using System;
using System.Drawing;
using Gwen.ControlInternal;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Base slider.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Slider : ControlBase
    {
        protected readonly SliderBar sliderBar;
        protected bool snapToNotches;
        protected int notchCount;
        protected float value;
        protected float min;
        protected float max;

        /// <summary>
        /// Number of notches on the slider axis.
        /// </summary>
        public int NotchCount { get { return notchCount; } set { notchCount = value; } }

        /// <summary>
        /// Determines whether the slider should snap to notches.
        /// </summary>
        public bool SnapToNotches { get { return snapToNotches; } set { snapToNotches = value; } }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public float Min { get { return min; } set { SetRange(value, max); } }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public float Max { get { return max; } set { SetRange(min, value); } }

        /// <summary>
        /// Current value.
        /// </summary>
        public float Value
        {
            get { return min + (this.value * (max - min)); }
            set
            {
                if (value < min) this.value = min;
                if (value > max) this.value = max;
                // Normalize Value
                this.value = (value - min) / (max - min);
                setValueInternal(this.value);
                Redraw();
            }
        }

        /// <summary>
        /// Invoked when the value has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        protected Slider(ControlBase parent)
            : base(parent)
        {
            SetBounds(new Rectangle(0, 0, 32, 128));

            sliderBar = new SliderBar(this);
            sliderBar.Dragged += onMoved;

            min = 0.0f;
            max = 1.0f;

            snapToNotches = false;
            notchCount = 5;
            value = 0.0f;

            KeyboardInputEnabled = true;
            IsTabable = true;
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
                Value = Value + 1;
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
                Value = Value + 1;
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
                Value = Value - 1;
            return true;
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
                Value = Value - 1;
            return true;
        }

        /// <summary>
        /// Handler for Home keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyHome(bool down)
        {
            if (down)
                Value = min;
            return true;
        }

        /// <summary>
        /// Handler for End keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyEnd(bool down)
        {
            if (down)
                Value = max;
            return true;
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void onMouseClickedLeft(int x, int y, bool down)
        {

        }

		protected virtual void onMoved(ControlBase control, EventArgs args)
        {
            setValueInternal(calculateValue());
        }

        protected virtual float calculateValue()
        {
            return 0;
        }

        protected virtual void updateBarFromValue()
        {

        }

        protected virtual void setValueInternal(float val)
        {
            if (snapToNotches)
            {
                val = (float)Math.Floor((val * notchCount) + 0.5f);
                val /= notchCount;
            }

            if (value != val)
            {
                value = val;
                if (ValueChanged != null)
					ValueChanged.Invoke(this, EventArgs.Empty);
            }

            updateBarFromValue();
        }

        /// <summary>
        /// Sets the value range.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        public void SetRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderFocus(Skin.SkinBase skin)
        {
            if (InputHandler.KeyboardFocus != this) return;
            if (!IsTabable) return;

            skin.DrawKeyboardHighlight(this, RenderBounds, 0);
        }
    }
}
