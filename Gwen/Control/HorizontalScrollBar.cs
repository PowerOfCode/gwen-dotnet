using System;
using System.Drawing;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Horizontal scrollbar.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class HorizontalScrollBar : ScrollBar
    {
        /// <summary>
        /// Bar size (in pixels).
        /// </summary>
        public override int BarSize
        {
            get { return bar.Width; }
            set { bar.Width = value; }
        }

        /// <summary>
        /// Bar position (in pixels).
        /// </summary>
        public override int BarPos
        {
            get { return bar.X - Height; }
        }

        /// <summary>
        /// Indicates whether the bar is horizontal.
        /// </summary>
        public override bool IsHorizontal
        {
            get { return true; }
        }

        /// <summary>
        /// Button size (in pixels).
        /// </summary>
        public override int ButtonSize
        {
            get { return Height; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalScrollBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public HorizontalScrollBar(ControlBase parent)
            : base(parent)
        {
            bar.IsHorizontal = true;

            scrollButton[0].SetDirectionLeft();
            scrollButton[0].Clicked += NudgeLeft;

            scrollButton[1].SetDirectionRight();
            scrollButton[1].Clicked += NudgeRight;

            bar.Dragged += onBarMoved;
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);

            scrollButton[0].Width = Height;
            scrollButton[0].Dock = Pos.Left;

            scrollButton[1].Width = Height;
            scrollButton[1].Dock = Pos.Right;

            bar.Height = ButtonSize;
            bar.Padding = new Padding(ButtonSize, 0, ButtonSize, 0);

            float barWidth = (viewableContentSize / contentSize) * (Width - (ButtonSize * 2));

            if (barWidth < ButtonSize * 0.5f)
                barWidth = (int)(ButtonSize * 0.5f);

            bar.Width = (int)(barWidth);
            bar.IsHidden = Width - (ButtonSize * 2) <= barWidth;

            //Based on our last scroll amount, produce a position for the bar
            if (!bar.IsHeld)
            {
                SetScrollAmount(ScrollAmount, true);
            }
        }

		public void NudgeLeft(ControlBase control, EventArgs args)
        {
            if (!IsDisabled)
                SetScrollAmount(ScrollAmount - NudgeAmount, true);
        }

		public void NudgeRight(ControlBase control, EventArgs args)
        {
            if (!IsDisabled)
                SetScrollAmount(ScrollAmount + NudgeAmount, true);
        }

        public override void ScrollToLeft()
        {
            SetScrollAmount(0, true);
        }

        public override void ScrollToRight()
        {
            SetScrollAmount(1, true);
        }

        public override float NudgeAmount
        {
            get
            {
                if (depressed)
                    return viewableContentSize / contentSize;
                else
                    return base.NudgeAmount;
            }
            set
            {
                base.NudgeAmount = value;
            }
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
            if (down)
            {
                depressed = true;
                InputHandler.MouseFocus = this;
            }
            else
            {
                Point clickPos = CanvasPosToLocal(new Point(x, y));
                if (clickPos.X < bar.X)
					NudgeLeft(this, EventArgs.Empty);
                else
                    if (clickPos.X > bar.X + bar.Width)
						NudgeRight(this, EventArgs.Empty);

                depressed = false;
                InputHandler.MouseFocus = null;
            }
        }

        protected override float calculateScrolledAmount()
        {
            return (float)(bar.X - ButtonSize) / (Width - bar.Width - (ButtonSize * 2));
        }

        /// <summary>
        /// Sets the scroll amount (0-1).
        /// </summary>
        /// <param name="value">Scroll amount.</param>
        /// <param name="forceUpdate">Determines whether the control should be updated.</param>
        /// <returns>
        /// True if control state changed.
        /// </returns>
        public override bool SetScrollAmount(float value, bool forceUpdate = false)
        {
            value = Util.Clamp(value, 0, 1);

            if (!base.SetScrollAmount(value, forceUpdate))
                return false;

            if (forceUpdate)
            {
                int newX = (int)(ButtonSize + (value * ((Width - bar.Width) - (ButtonSize * 2))));
                bar.MoveTo(newX, bar.Y);
            }

            return true;
        }

        /// <summary>
        /// Handler for the BarMoved event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected override void onBarMoved(ControlBase control, EventArgs args)
        {
            if (bar.IsHeld)
            {
                SetScrollAmount(calculateScrolledAmount(), false);
                base.onBarMoved(control, args);
            }
            else
                InvalidateParent();
        }
    }
}
