﻿using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Modal control for windows.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Modal : ControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Modal"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Modal(ControlBase parent)
            : base(parent)
        {
            KeyboardInputEnabled = true;
            MouseInputEnabled = true;
            ShouldDrawBackground = true;
            SetBounds(0, 0, GetCanvas().Width, GetCanvas().Height);
        }
        
        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Layout(Skin.SkinBase skin)
        {
            SetBounds(0, 0, GetCanvas().Width, GetCanvas().Height);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawModalControl(this);
        }
    }
}
