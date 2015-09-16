using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Vertical slider.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class VerticalSlider : Slider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public VerticalSlider(ControlBase parent)
            : base(parent)
        {
            sliderBar.IsHorizontal = false;
        }

        protected override float calculateValue()
        {
            return 1 - sliderBar.Y / (float)(Height - sliderBar.Height);
        }

        protected override void updateBarFromValue()
        {
            sliderBar.MoveTo(sliderBar.X, (int)((Height - sliderBar.Height) * (1 - value)));
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
            sliderBar.MoveTo(sliderBar.X, (int) (CanvasPosToLocal(new Point(x, y)).Y - sliderBar.Height*0.5));
            sliderBar.InputMouseClickedLeft(x, y, down);
            onMoved(sliderBar, EventArgs.Empty);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            sliderBar.SetSize(Width, 15);
            updateBarFromValue();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawSlider(this, false, snapToNotches ? notchCount : 0, sliderBar.Height);
        }
    }
}
