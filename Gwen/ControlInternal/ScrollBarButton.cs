using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Scrollbar button.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ScrollBarButton : Button
    {
        private Pos direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollBarButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ScrollBarButton(ControlBase parent)
            : base(parent)
        {
            SetDirectionUp();
        }

        public virtual void SetDirectionUp()
        {
            direction = Pos.Top;
        }

        public virtual void SetDirectionDown()
        {
            direction = Pos.Bottom;
        }

        public virtual void SetDirectionLeft()
        {
            direction = Pos.Left;
        }

        public virtual void SetDirectionRight()
        {
            direction = Pos.Right;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawScrollButton(this, direction, IsDepressed, IsHovered, IsDisabled);
        }
    }
}
