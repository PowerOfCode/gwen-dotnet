using System;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Single property row.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class PropertyRow : ControlBase
    {
        private readonly Label label;
        private readonly Property.PropertyBase property;
        private bool lastEditing;
        private bool lastHover;

        /// <summary>
        /// Invoked when the property value has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public bool IsEditing { get { return property != null && property.IsEditing; } }

        /// <summary>
        /// Property value.
        /// </summary>
        public string Value { get { return property.Value; } set { property.Value = value; } }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get
            {
                return base.IsHovered || (property != null && property.IsHovered);
            }
        }

        /// <summary>
        /// Property name.
        /// </summary>
        public string Label { get { return label.Text; } set { label.Text = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="prop">Property control associated with this row.</param>
        public PropertyRow(ControlBase parent, Property.PropertyBase prop)
            : base(parent)
        {
            PropertyRowLabel label = new PropertyRowLabel(this);
            label.Dock = Pos.Left;
            label.Alignment = Pos.Left | Pos.Top;
            label.Margin = new Margin(2, 2, 0, 0);
            this.label = label;

            property = prop;
            property.Parent = this;
            property.Dock = Pos.Fill;
            property.ValueChanged += onValueChanged;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            /* SORRY */
            if (IsEditing != lastEditing)
            {
                onEditingChanged();
                lastEditing = IsEditing;
            }

            if (IsHovered != lastHover)
            {
                onHoverChanged();
                lastHover = IsHovered;
            }
            /* SORRY */

            skin.DrawPropertyRow(this, label.Right, IsEditing, IsHovered | property.IsHovered);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            Properties parent = Parent as Properties;
            if (null == parent) return;

            label.Width = parent.SplitWidth;

            if (property != null)
            {
                Height = property.Height;
            }
        }

        protected virtual void onValueChanged(ControlBase control, EventArgs args)
        {
            if (ValueChanged != null)
				ValueChanged.Invoke(this, EventArgs.Empty);
        }

        private void onEditingChanged()
        {
            label.Redraw();
        }

        private void onHoverChanged()
        {
            label.Redraw();
        }
    }
}
