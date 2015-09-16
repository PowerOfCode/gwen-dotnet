using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Label for PropertyRow.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class PropertyRowLabel : Label
    {
        private readonly PropertyRow propertyRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRowLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyRowLabel(PropertyRow parent) : base(parent)
        {
			AutoSizeToContents = false;
            Alignment = Pos.Left | Pos.CenterV;
            propertyRow = parent;
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        public override void UpdateColors()
        {
            if (IsDisabled)
            {
                TextColor = Skin.Colors.Button.Disabled;
                return;
            }

            if (propertyRow != null && propertyRow.IsEditing)
            {
                TextColor = Skin.Colors.Properties.Label_Selected;
                return;
            }

            if (propertyRow != null && propertyRow.IsHovered)
            {
                TextColor = Skin.Colors.Properties.Label_Hover;
                return;
            }

            TextColor = Skin.Colors.Properties.Label_Normal;
        }
    }
}
