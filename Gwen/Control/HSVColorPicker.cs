using System;
using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// HSV color picker with "before" and "after" color boxes.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class HSVColorPicker : ControlBase, IColorPicker
    {
        private readonly ColorLerpBox lerpBox;
        private readonly ColorSlider colorSlider;
        private readonly ColorDisplay before;
        private readonly ColorDisplay after;

        /// <summary>
        /// Invoked when the selected color has changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

        /// <summary>
        /// The "before" color.
        /// </summary>
        public Color DefaultColor { get { return before.Color; } set { before.Color = value; } }

        /// <summary>
        /// Selected color.
        /// </summary>
        public Color SelectedColor { get { return lerpBox.SelectedColor; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="HSVColorPicker"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public HSVColorPicker(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = true;
            SetSize(256, 128);
            //ShouldCacheToTexture = true;

            lerpBox = new ColorLerpBox(this);
            lerpBox.ColorChanged += colorBoxChanged;
            lerpBox.Dock = Pos.Left;

            colorSlider = new ColorSlider(this);
            colorSlider.SetPosition(lerpBox.Width + 15, 5);
            colorSlider.ColorChanged += colorSliderChanged;
            colorSlider.Dock = Pos.Left;

            after = new ColorDisplay(this);
            after.SetSize(48, 24);
            after.SetPosition(colorSlider.X + colorSlider.Width + 15, 5);

            before = new ColorDisplay(this);
            before.SetSize(48, 24);
            before.SetPosition(after.X, 28);

            int x = before.X;
            int y = before.Y + 30;

            {
                Label label = new Label(this);
                label.SetText("R:");
                label.SizeToContents();
                label.SetPosition(x, y);

                TextBoxNumeric numeric = new TextBoxNumeric(this);
                numeric.Name = "RedBox";
                numeric.SetPosition(x + 15, y - 1);
                numeric.SetSize(26, 16);
                numeric.SelectAllOnFocus = true;
                numeric.TextChanged += numericTyped;
            }

            y += 20;

            {
                Label label = new Label(this);
                label.SetText("G:");
                label.SizeToContents();
                label.SetPosition(x, y);

                TextBoxNumeric numeric = new TextBoxNumeric(this);
                numeric.Name = "GreenBox";
                numeric.SetPosition(x + 15, y - 1);
                numeric.SetSize(26, 16);
                numeric.SelectAllOnFocus = true;
                numeric.TextChanged += numericTyped;
            }

            y += 20;

            {
                Label label = new Label(this);
                label.SetText("B:");
                label.SizeToContents();
                label.SetPosition(x, y);

                TextBoxNumeric numeric = new TextBoxNumeric(this);
                numeric.Name = "BlueBox";
                numeric.SetPosition(x + 15, y - 1);
                numeric.SetSize(26, 16);
                numeric.SelectAllOnFocus = true;
                numeric.TextChanged += numericTyped;
            }

            SetColor(DefaultColor);
        }

		private void numericTyped(ControlBase control, EventArgs args)
        {
            TextBoxNumeric box = control as TextBoxNumeric;
            if (null == box) return;

            if (box.Text == String.Empty) return;

            int textValue = (int)box.Value;
            if (textValue < 0) textValue = 0;
            if (textValue > 255) textValue = 255;

            Color newColor = SelectedColor;

            if (box.Name.Contains("Red"))
            {
                newColor = Color.FromArgb(SelectedColor.A, textValue, SelectedColor.G, SelectedColor.B);
            }
            else if (box.Name.Contains("Green"))
            {
                newColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, textValue, SelectedColor.B);
            }
            else if (box.Name.Contains("Blue"))
            {
                newColor = Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, textValue);
            }
            else if (box.Name.Contains("Alpha"))
            {
                newColor = Color.FromArgb(textValue, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            }

            SetColor(newColor);
        }

        private void updateControls(Color color)
        {
            // [???] TODO: Make this code safer.
			// [halfofastaple] This code SHOULD (in theory) never crash/not work as intended, but referencing children by their name is unsafe.
            //		Instead, a direct reference to their objects should be maintained. Worst case scenario, we grab the wrong "RedBox".

            TextBoxNumeric redBox = FindChildByName("RedBox", false) as TextBoxNumeric;
            if (redBox != null)
                redBox.SetText(color.R.ToString(), false);

            TextBoxNumeric greenBox = FindChildByName("GreenBox", false) as TextBoxNumeric;
            if (greenBox != null)
                greenBox.SetText(color.G.ToString(), false);

            TextBoxNumeric blueBox = FindChildByName("BlueBox", false) as TextBoxNumeric;
            if (blueBox != null)
                blueBox.SetText(color.B.ToString(), false);

            after.Color = color;

            if (ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the selected color.
        /// </summary>
        /// <param name="color">Color to set.</param>
        /// <param name="onlyHue">Determines whether only the hue should be set.</param>
        /// <param name="reset">Determines whether the "before" color should be set as well.</param>
        public void SetColor(Color color, bool onlyHue = false, bool reset = false)
        {
            updateControls(color);

            if (reset)
                before.Color = color;

            colorSlider.SelectedColor = color;
            lerpBox.SetColor(color, onlyHue);
            after.Color = color;
        }

		private void colorBoxChanged(ControlBase control, EventArgs args)
        {
            updateControls(SelectedColor);
            Invalidate();
        }

		private void colorSliderChanged(ControlBase control, EventArgs args)
        {
            if (lerpBox != null)
                lerpBox.SetColor(colorSlider.SelectedColor, true);
            Invalidate();
        }
    }
}
