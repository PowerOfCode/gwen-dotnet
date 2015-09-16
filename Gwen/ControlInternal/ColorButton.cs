using System;
using System.Drawing;
using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Property button.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ColorButton : Button
    {
        private Color color;

        /// <summary>
        /// Current color value.
        /// </summary>
        public Color Color { get { return color; } set { color = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ColorButton(ControlBase parent) : base(parent)
        {
            color = Color.Black;
            Text = String.Empty;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = color;
            skin.Renderer.DrawFilledRect(RenderBounds);
        }
    }
}
