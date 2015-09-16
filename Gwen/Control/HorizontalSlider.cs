using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Horizontal slider.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class HorizontalSlider : Slider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public HorizontalSlider(ControlBase parent)
            : base(parent)
        {
            sliderBar.IsHorizontal = true;
        }

        protected override float calculateValue()
        {
            return (float)sliderBar.X / (Width - sliderBar.Width);
        }

        protected override void updateBarFromValue()
        {
            sliderBar.MoveTo((int)((Width - sliderBar.Width) * (value)), sliderBar.Y);
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
            sliderBar.MoveTo((int)(CanvasPosToLocal(new Point(x, y)).X - sliderBar.Width*0.5), sliderBar.Y);
            sliderBar.InputMouseClickedLeft(x, y, down);
            onMoved(sliderBar, EventArgs.Empty);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            sliderBar.SetSize(15, Height);
            updateBarFromValue();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawSlider(this, true, snapToNotches ? notchCount : 0, sliderBar.Width);
        }
    }
}
