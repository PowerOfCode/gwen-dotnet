using System;

namespace Gwen.Control.Property
{
    /// <summary>
    /// Text property.
    /// </summary>
    public class Text : PropertyBase
    {
        protected readonly TextBox textBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Text(Control.ControlBase parent) : base(parent)
        {
            textBox = new TextBox(this);
            textBox.Dock = Pos.Fill;
            textBox.ShouldDrawBackground = false;
            textBox.TextChanged += onValueChanged;
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
            get { return textBox.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered | textBox.IsHovered; }
        }
    }
}
