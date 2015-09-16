using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// ComboBox arrow.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class DownArrow : ControlBase
    {
        private readonly ComboBox comboBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownArrow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public DownArrow(ComboBox parent)
            : base(parent) // or Base?
        {
            MouseInputEnabled = false;
            SetSize(15, 15);

            comboBox = parent;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawComboBoxArrow(this, comboBox.IsHovered, comboBox.IsDepressed, comboBox.IsOpen, comboBox.IsDisabled);
        }
    }
}
