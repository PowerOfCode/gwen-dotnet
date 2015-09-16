using System.Drawing;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Image container.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ImagePanel : ControlBase
    {
        private readonly Texture texture;
        private readonly float[] uv;
        private Color drawColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePanel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ImagePanel(ControlBase parent)
            : base(parent)
        {
            uv = new float[4];
            texture = new Texture(Skin.Renderer);
            SetUV(0, 0, 1, 1);
            MouseInputEnabled = true;
            drawColor = Color.White;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            texture.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Sets the texture coordinates of the image.
        /// </summary>
        public virtual void SetUV(float u1, float v1, float u2, float v2)
        {
            uv[0] = u1;
            uv[1] = v1;
            uv[2] = u2;
            uv[3] = v2;
        }

        /// <summary>
        /// Texture name.
        /// </summary>
        public string ImageName
        {
            get { return texture.Name; }
            set { texture.Load(value); }
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            base.render(skin);
            skin.Renderer.DrawColor = drawColor;
            skin.Renderer.DrawTexturedRect(texture, RenderBounds, uv[0], uv[1], uv[2], uv[3]);
        }

        /// <summary>
        /// Sizes the control to its contents.
        /// </summary>
        public virtual void SizeToContents()
        {
            SetSize(texture.Width, texture.Height);
        }

        /// <summary>
        /// Control has been clicked - invoked by input system. Windows use it to propagate activation.
        /// </summary>
        public override void Touch()
        {
            base.Touch();
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeySpace(bool down)
        {
            if (down)
                base.onMouseClickedLeft(0, 0, true);
            return true;
        }
    }
}
