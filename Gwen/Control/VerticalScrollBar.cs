using System;
using System.Drawing;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Vertical scrollbar.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class VerticalScrollBar : ScrollBar
    {
        /// <summary>
        /// Bar size (in pixels).
        /// </summary>
        public override int BarSize
        {
            get { return bar.Height; }
            set { bar.Height = value; }
        }

        /// <summary>
        /// Bar position (in pixels).
        /// </summary>
        public override int BarPos
        {
            get { return bar.Y - Width; }
        }

        /// <summary>
        /// Button size (in pixels).
        /// </summary>
        public override int ButtonSize
        {
            get { return Width; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalScrollBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public VerticalScrollBar(ControlBase parent)
            : base(parent)
        {
            bar.IsVertical = true;

            scrollButton[0].SetDirectionUp();
            scrollButton[0].Clicked += NudgeUp;

            scrollButton[1].SetDirectionDown();
            scrollButton[1].Clicked += NudgeDown;

            bar.Dragged += onBarMoved;
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);

            scrollButton[0].Height = Width;
            scrollButton[0].Dock = Pos.Top;

            scrollButton[1].Height = Width;
            scrollButton[1].Dock = Pos.Bottom;

            bar.Width = ButtonSize;
            bar.Padding = new Padding(0, ButtonSize, 0, ButtonSize);

            float barHeight = 0.0f;
            if (contentSize > 0.0f) barHeight = (viewableContentSize/contentSize)*(Height - (ButtonSize*2));

            if (barHeight < ButtonSize*0.5f)
                barHeight = (int) (ButtonSize*0.5f);

            bar.Height = (int) (barHeight);
            bar.IsHidden = Height - (ButtonSize*2) <= barHeight;

            //Based on our last scroll amount, produce a position for the bar
            if (!bar.IsHeld)
            {
                SetScrollAmount(ScrollAmount, true);
            }
        }

		public virtual void NudgeUp(ControlBase control, EventArgs args)
        {
            if (!IsDisabled)
                SetScrollAmount(ScrollAmount - NudgeAmount, true);
        }

		public virtual void NudgeDown(ControlBase control, EventArgs args)
        {
            if (!IsDisabled)
                SetScrollAmount(ScrollAmount + NudgeAmount, true);
        }

        public override void ScrollToTop()
        {
            SetScrollAmount(0, true);
        }

        public override void ScrollToBottom()
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
                if (clickPos.Y < bar.Y)
                    NudgeUp(this, EventArgs.Empty);
                else if (clickPos.Y > bar.Y + bar.Height)
                    NudgeDown(this, EventArgs.Empty);

                depressed = false;
                InputHandler.MouseFocus = null;
            }
        }

        protected override float calculateScrolledAmount()
        {
            return (float)(bar.Y - ButtonSize) / (Height - bar.Height - (ButtonSize * 2));
        }

        /// <summary>
        /// Sets the scroll amount (0-1).
        /// </summary>
        /// <param name="value">Scroll amount.</param>
        /// <param name="forceUpdate">Determines whether the control should be updated.</param>
        /// <returns>True if control state changed.</returns>
        public override bool SetScrollAmount(float value, bool forceUpdate = false)
        {
            value = Util.Clamp(value, 0, 1);

            if (!base.SetScrollAmount(value, forceUpdate))
                return false;

            if (forceUpdate)
            {
                int newY = (int)(ButtonSize + (value * ((Height - bar.Height) - (ButtonSize * 2))));
                bar.MoveTo(bar.X, newY);
            }

            return true;
        }

        /// <summary>
        /// Handler for the BarMoved event.
        /// </summary>
        /// <param name="control">The control.</param>
		protected override void onBarMoved(ControlBase control, EventArgs args)
        {
            if (bar.IsHeld)
            {
                SetScrollAmount(calculateScrolledAmount(), false);
				base.onBarMoved(control, EventArgs.Empty);
            }
            else
                InvalidateParent();
        }
    }
}
