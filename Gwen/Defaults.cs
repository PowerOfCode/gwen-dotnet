namespace Gwen
{
    public static class Defaults
    {
        public static Gwen.Skin.SkinBase Skin { get; set; }
        public static Renderer.RendererBase Renderer { get{ return Skin.Renderer; } }

//        static Defaults()
//        {
//            //Skin = new TexturedBase(new Gwen.Renderer.OpenTK(), "DefaultSkin.png");
//        }
    }
}

