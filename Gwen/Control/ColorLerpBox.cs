using System;
using System.Drawing;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Linear-interpolated HSV color box.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ColorLerpBox : ControlBase
    {
        private Point cursorPos;
        private bool depressed;
        private float hue;
        private Texture texture;

        /// <summary>
        /// Invoked when the selected color has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorLerpBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ColorLerpBox(ControlBase parent) : base(parent)
        {
            SetColor(Color.FromArgb(255, 255, 128, 0));
            SetSize(128, 128);
            MouseInputEnabled = true;
            depressed = false;

            // texture is initialized in renderer() if null
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
        /// Linear color interpolation.
        /// </summary>
        public static Color Lerp(Color toColor, Color fromColor, float amount)
        {
            Color delta = toColor.Subtract(fromColor);
            delta = delta.Multiply(amount);
            return fromColor.Add(delta);
        }

        /// <summary>
        /// Selected color.
        /// </summary>
        public Color SelectedColor
        {
            get { return getColorAt(cursorPos.X, cursorPos.Y); }
        }

        /// <summary>
        /// Sets the selected color.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="onlyHue">Deetrmines whether to only set H value (not SV).</param>
        public void SetColor(Color value, bool onlyHue = true)
        {
            HSV hsv = value.ToHSV();
            hue = hsv.h;

            if (!onlyHue)
            {
                cursorPos.X = (int)(hsv.s * Width);
                cursorPos.Y = (int)((1 - hsv.v) * Height);
            }
            Invalidate();

            if (ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
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
                cursorPos = CanvasPosToLocal(new Point(x, y));
                //Do we have clamp?
                if (cursorPos.X < 0)
                    cursorPos.X = 0;
                if (cursorPos.X > Width)
                    cursorPos.X = Width;

                if (cursorPos.Y < 0)
                    cursorPos.Y = 0;
                if (cursorPos.Y > Height)
                    cursorPos.Y = Height;

                if (ColorChanged != null)
                    ColorChanged.Invoke(this, EventArgs.Empty);
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
            depressed = down;
            if (down)
                InputHandler.MouseFocus = this;
            else
                InputHandler.MouseFocus = null;

            onMouseMoved(x, y, 0, 0);
        }

        /// <summary>
        /// Gets the color from specified coordinates.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Color value.</returns>
        private Color getColorAt(int x, int y)
        {
            float xPercent = (x / (float)Width);
            float yPercent = 1 - (y / (float)Height);

            Color result = Util.HSVToColor(hue, xPercent, yPercent);

            return result;
        }

        /// <summary>
        /// Invalidates the control.
        /// </summary>
        public override void Invalidate()
        {
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }
            base.Invalidate();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            if (texture == null)
            {
                byte[] pixelData = new byte[Width*Height*4];

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Color c = getColorAt(x, y);
                        pixelData[4*(x + y*Width)] = c.R;
                        pixelData[4*(x + y*Width) + 1] = c.G;
                        pixelData[4*(x + y*Width) + 2] = c.B;
                        pixelData[4*(x + y*Width) + 3] = c.A;
                    }
                }

                texture = new Texture(skin.Renderer);
                texture.Width = Width;
                texture.Height = Height;
                texture.LoadRaw(Width, Height, pixelData);
            }

            skin.Renderer.DrawColor = Color.White;
            skin.Renderer.DrawTexturedRect(texture, RenderBounds);


            skin.Renderer.DrawColor = Color.Black;
            skin.Renderer.DrawLinedRect(RenderBounds);

            Color selected = SelectedColor;
            if ((selected.R + selected.G + selected.B)/3 < 170)
                skin.Renderer.DrawColor = Color.White;
            else
                skin.Renderer.DrawColor = Color.Black;

            Rectangle testRect = new Rectangle(cursorPos.X - 3, cursorPos.Y - 3, 6, 6);

            skin.Renderer.DrawShavedCornerRect(testRect);

            base.render(skin);
        }
    }
}
