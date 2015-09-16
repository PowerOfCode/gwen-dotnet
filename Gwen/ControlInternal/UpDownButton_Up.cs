using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Numeric up arrow.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class UpDownButton_Up : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpDownButton_Up"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public UpDownButton_Up(ControlBase parent)
            : base(parent)
        {
            SetSize(7, 7);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawNumericUpDownButton(this, IsDepressed, true);
        }
    }
}
