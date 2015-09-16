using System;

namespace Gwen.Control.Layout
{
    /// <summary>
    /// Base splitter class.
    /// </summary>
    public class Splitter : ControlBase
    {
        private readonly ControlBase[] panel;
        private readonly bool[] scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="Splitter"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Splitter(ControlBase parent) : base(parent)
        {
            panel = new ControlBase[2];
            scale = new bool[2];
            scale[0] = true;
            scale[1] = true;
        }

        /// <summary>
        /// Sets the contents of a splitter panel.
        /// </summary>
        /// <param name="panelIndex">Panel index (0-1).</param>
        /// <param name="panel">Panel contents.</param>
        /// <param name="noScale">Determines whether the content is to be scaled.</param>
        public void SetPanel(int panelIndex, ControlBase panel, bool noScale = false)
        {
            if (panelIndex < 0 || panelIndex > 1)
                throw new ArgumentException("Invalid panel index", "panelIndex");

            this.panel[panelIndex] = panel;
            scale[panelIndex] = !noScale;

            if (null != this.panel[panelIndex])
            {
                this.panel[panelIndex].Parent = this;
            }
        }

        /// <summary>
        /// Gets the contents of a secific panel.
        /// </summary>
        /// <param name="panelIndex">Panel index (0-1).</param>
        /// <returns></returns>
        ControlBase GetPanel(int panelIndex)
        {
            if (panelIndex < 0 || panelIndex > 1)
                throw new ArgumentException("Invalid panel index", "panelIndex");
            return panel[panelIndex];
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            layoutVertical(skin);
        }

        protected virtual void layoutVertical(Skin.SkinBase skin)
        {
            int w = Width;
            int h = Height;

            if (panel[0] != null)
            {
                Margin m = panel[0].Margin;
                if (scale[0])
                    panel[0].SetBounds(m.Left, m.Top, w - m.Left - m.Right, (h*0.5f) - m.Top - m.Bottom);
                else
                    panel[0].Position(Pos.Center, 0, (int) (h*-0.25f));
            }

            if (panel[1] != null)
            {
                Margin m = panel[1].Margin;
                if (scale[1])
                    panel[1].SetBounds(m.Left, m.Top + (h*0.5f), w - m.Left - m.Right, (h*0.5f) - m.Top - m.Bottom);
                else
                    panel[1].Position(Pos.Center, 0, (int) (h*0.25f));
            }
        }

        protected virtual void layoutHorizontal(Skin.SkinBase skin)
        {
            throw new NotImplementedException();
        }
    }
}
