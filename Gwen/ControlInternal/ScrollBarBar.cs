using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Scrollbar bar.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ScrollBarBar : Dragger
    {
        private bool horizontal;

        /// <summary>
        /// Indicates whether the bar is horizontal.
        /// </summary>
        public bool IsHorizontal { get { return horizontal; } set { horizontal = value; } }

        /// <summary>
        /// Indicates whether the bar is vertical.
        /// </summary>
        public bool IsVertical { get { return !horizontal; } set { horizontal = !value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollBarBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ScrollBarBar(ControlBase parent)
            : base(parent)
        {
            RestrictToParent = true;
            Target = this;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawScrollBarBar(this, held, IsHovered, horizontal);
            base.render(skin);
        }

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected override void onMouseMoved(int x, int y, int dx, int dy)
        {
            base.onMouseMoved(x, y, dx, dy);
            if (!held)
                return;

            InvalidateParent();
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void onMouseClickedLeft(int x, int y, bool down)
        {
            base.onMouseClickedLeft(x, y, down);
            InvalidateParent();
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            if (null == Parent)
                return;

            //Move to our current position to force clamping - is this a hack?
            MoveTo(X, Y);
        }
    }
}
