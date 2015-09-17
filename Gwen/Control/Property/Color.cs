using System;
using Gwen.ControlInternal;
using Gwen.Input;

namespace Gwen.Control.Property
{
    /// <summary>
    /// Color property.
    /// </summary>
    public class Color : Text
    {
        protected readonly ColorButton button;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Color(Control.ControlBase parent) : base(parent)
        {
            button = new ColorButton(textBox);
            button.Dock = Pos.Right;
            button.Width = 20;
            button.Margin = new Margin(1, 1, 1, 2);
            button.Clicked += onButtonPressed;
        }

        /// <summary>
        /// Color-select button press handler.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onButtonPressed(Control.ControlBase control, EventArgs args)
        {
            Menu menu = new Menu(GetCanvas());
            menu.SetSize(256, 180);
            menu.DeleteOnClose = true;
            menu.IconMarginDisabled = true;

            HSVColorPicker picker = new HSVColorPicker(menu);
            picker.Dock = Pos.Fill;
            picker.SetSize(256, 128);

            picker.SetColor(getColorFromText(), false, true);
            picker.ColorChanged += onColorChanged;

            menu.Open(Pos.Right | Pos.Top);
        }

        /// <summary>
        /// Color changed handler.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onColorChanged(Control.ControlBase control, EventArgs args)
        {
            HSVColorPicker picker = control as HSVColorPicker;
            setTextFromColor(picker.SelectedColor);
            doChanged();
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get { return textBox.Text; }
            set { base.Value = value; }
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void setValue(string value, bool fireEvents = false)
        {
            textBox.SetText(value, fireEvents);
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return textBox == InputHandler.KeyboardFocus; }
        }

        private void setTextFromColor(System.Drawing.Color color)
        {
            textBox.Text = String.Format("{0} {1} {2}", color.R, color.G, color.B);
        }

        private System.Drawing.Color getColorFromText()
        {
            string[] split = textBox.Text.Split(' ');

            byte red = 0;
            byte green = 0;
            byte blue = 0;
            byte alpha = 255;

            if (split.Length > 0 && split[0].Length > 0)
            {
                Byte.TryParse(split[0], out red);
            }

            if (split.Length > 1 && split[1].Length > 0)
            {
                Byte.TryParse(split[1], out green);
            }

            if (split.Length > 2 && split[2].Length > 0)
            {
                Byte.TryParse(split[2], out blue);
            }

            return System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }

        protected override void doChanged()
        {
            base.doChanged();
            button.Color = getColorFromText();
        }
    }
}
