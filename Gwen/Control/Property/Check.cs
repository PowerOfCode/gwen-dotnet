using System;

namespace Gwen.Control.Property
{
    /// <summary>
    /// Checkable property.
    /// </summary>
    public class Check : PropertyBase
    {
        protected readonly Control.CheckBox checkBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="Check"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Check(Control.ControlBase parent)
            : base(parent)
        {
            checkBox = new Control.CheckBox(this);
            checkBox.ShouldDrawBackground = false;
            checkBox.CheckChanged += onValueChanged;
            checkBox.IsTabable = true;
            checkBox.KeyboardInputEnabled = true;
            checkBox.SetPosition(2, 1);

            Height = 18;
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get { return checkBox.IsChecked ? "1" : "0"; }
            set { base.Value = value; }
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void setValue(string value, bool fireEvents = false)
        {
            if (value == "1" || value.ToLower() == "true" || value.ToLower() == "yes")
            {
                checkBox.IsChecked = true;
            }
            else
            {
                checkBox.IsChecked = false;
            }
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return checkBox.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered || checkBox.IsHovered; }
        }
    }
}
