using System;
using System.Drawing;

namespace Gwen.Skin.Texturing
{
    /// <summary>
    /// Single textured element.
    /// </summary>
    public struct Single
    {
        private readonly Texture texture;
        private readonly float[] uv;
        private readonly int width;
        private readonly int height;

        public Single(Texture texture, float x, float y, float w, float h )
        {
            this.texture = texture;

            float texw = texture.Width;
            float texh = texture.Height;

            uv = new float[4];
            uv[0] = x / texw;
            uv[1] = y / texh;
            uv[2] = (x + w) / texw;
            uv[3] = (y + h) / texh;

            width = (int) w;
            height = (int) h;
        }

        // can't have this as default param
        public void Draw(Renderer.RendererBase render, Rectangle r)
        {
            Draw(render, r, Color.White);
        }

        public void Draw(Renderer.RendererBase render, Rectangle r, Color col)
        {
            if (texture == null)
                return;

            render.DrawColor = col;
            render.DrawTexturedRect(texture, r, uv[0], uv[1], uv[2], uv[3]);
        }

        public void DrawCenter(Renderer.RendererBase render, Rectangle r)
        {
            if (texture == null)
                return;

            DrawCenter(render, r, Color.White);
        }

        public void DrawCenter(Renderer.RendererBase render, Rectangle r, Color col)
        {
            r.X += (int)((r.Width - width) * 0.5);
            r.Y += (int)((r.Height - height) * 0.5);
            r.Width = width;
            r.Height = height;

            Draw(render, r, col);
        }
    }
}
