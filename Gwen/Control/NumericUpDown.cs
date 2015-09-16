using System;
using Gwen.Control.Layout;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Numeric up/down.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class NumericUpDown : TextBoxNumeric
    {
        private int max;
        private int min;

        private readonly Splitter splitter;
        private readonly UpDownButton_Up up;
        private readonly UpDownButton_Down down;

        /// <summary>
        /// Minimum value.
        /// </summary>
        public int Min { get { return min; } set { min = value; } }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public int Max { get { return max; } set { max = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericUpDown"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public NumericUpDown(ControlBase parent)
            : base(parent)
        {
            SetSize(100, 20);

            splitter = new Splitter(this);
            splitter.Dock = Pos.Right;
            splitter.SetSize(13, 13);

            up = new UpDownButton_Up(splitter);
            up.Clicked += onButtonUp;
            up.IsTabable = false;
            splitter.SetPanel(0, up, false);

            down = new UpDownButton_Down(splitter);
            down.Clicked += onButtonDown;
            down.IsTabable = false;
            down.Padding = new Padding(0, 1, 1, 0);
            splitter.SetPanel(1, down, false);

            max = 100;
            min = 0;
            value = 0f;
            Text = "0";
        }

        /// <summary>
        /// Invoked when the value has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyUp(bool down)
        {
            if (down) onButtonUp(null, EventArgs.Empty);
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
            if (down) onButtonDown(null, new ClickedEventArgs(0, 0, true));
            return true;
        }

        /// <summary>
        /// Handler for the button up event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onButtonUp(ControlBase control, EventArgs args)
        {
            Value = value + 1;
        }

        /// <summary>
        /// Handler for the button down event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void onButtonDown(ControlBase control, ClickedEventArgs args)
        {
            Value = value - 1;
        }

        /// <summary>
        /// Determines whether the text can be assighed to the control.
        /// </summary>
        /// <param name="str">Text to evaluate.</param>
        /// <returns>True if the text is allowed.</returns>
        protected override bool isTextAllowed(string str)
        {
            float d;
            if (!float.TryParse(str, out d))
                return false;
            if (d < min) return false;
            if (d > max) return false;
            return true;
        }

        /// <summary>
        /// Numeric value of the control.
        /// </summary>
        public override float Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (value < min) this.value = min;
                if (value > max) this.value = max;
                if (this.value == value) return;

                base.Value = value;
            }
        }

        /// <summary>
        /// Handler for the text changed event.
        /// </summary>
        protected override void onTextChanged()
        {
            base.onTextChanged();
            if (ValueChanged != null)
                ValueChanged.Invoke(this, EventArgs.Empty);
        }
    }
}
