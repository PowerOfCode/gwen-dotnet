using System;
using System.Drawing;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// HSV hue selector.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ColorSlider : ControlBase
    {
        private int selectedDist;
        private bool depressed;
        private Texture texture;

        /// <summary>
        /// Invoked when the selected color has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ColorSlider(ControlBase parent)
            : base(parent)
        {
            SetSize(32, 128);
            MouseInputEnabled = true;
            depressed = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (texture != null)
                texture.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            //Is there any way to move this into skin? Not for now, no idea how we'll "actually" render these

            if (texture == null)
            {
                byte[] pixelData = new byte[Width * Height * 4];

                for (int y = 0; y < Height; y++)
                {
                    Color c = getColorAtHeight(y);
                    for (int x = 0; x < Width; x++)
                    {
                        pixelData[4 * (x + y * Width)] = c.R;
                        pixelData[4 * (x + y * Width) + 1] = c.G;
                        pixelData[4 * (x + y * Width) + 2] = c.B;
                        pixelData[4 * (x + y * Width) + 3] = c.A;
                    }
                }

                texture = new Texture(skin.Renderer);
                texture.Width = Width;
                texture.Height = Height;
                texture.LoadRaw(Width, Height, pixelData);
            }

            skin.Renderer.DrawColor = Color.White;
            skin.Renderer.DrawTexturedRect(texture, new Rectangle(5, 0, Width-10, Height));

            int drawHeight = selectedDist - 3;

            //Draw our selectors
            skin.Renderer.DrawColor = Color.Black;
            skin.Renderer.DrawFilledRect(new Rectangle(0, drawHeight + 2, Width, 1));
            skin.Renderer.DrawFilledRect(new Rectangle(0, drawHeight, 5, 5));
            skin.Renderer.DrawFilledRect(new Rectangle(Width - 5, drawHeight, 5, 5));
            skin.Renderer.DrawColor = Color.White;
            skin.Renderer.DrawFilledRect(new Rectangle(1, drawHeight + 1, 3, 3));
            skin.Renderer.DrawFilledRect(new Rectangle(Width - 4, drawHeight + 1, 3, 3));

            base.render(skin);
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
            depressed = down;
            if (down)
                InputHandler.MouseFocus = this;
            else
                InputHandler.MouseFocus = null;

            onMouseMoved(x, y, 0, 0);
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
            if (depressed)
            {
                Point cursorPos = CanvasPosToLocal(new Point(x, y));

                if (cursorPos.Y < 0)
                    cursorPos.Y = 0;
                if (cursorPos.Y > Height)
                    cursorPos.Y = Height;

                selectedDist = cursorPos.Y;
                if (ColorChanged != null)
                    ColorChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private Color getColorAtHeight(int y)
        {
            float yPercent = y / (float)Height;
            return Util.HSVToColor(yPercent * 360, 1, 1);
        }

        private void setColor(Color color)
        {
            HSV hsv = color.ToHSV();

            selectedDist = (int)(hsv.h / 360 * Height);

            if (ColorChanged != null)
                ColorChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Selected color.
        /// </summary>
        public Color SelectedColor { get { return getColorAtHeight(selectedDist); } set { setColor(value); } }
    }
}
