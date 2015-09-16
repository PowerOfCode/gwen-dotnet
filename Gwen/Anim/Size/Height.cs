using System;

namespace Gwen.Anim.Size
{
    class Height : TimedAnimation
    {
        private int startSize;
        private int delta;
        private bool hide;

        public Height(int startSize, int endSize, float length, bool hide = false, float delay = 0.0f, float ease = 1.0f)
            : base(length, delay, ease)
        {
            this.startSize = startSize;
            delta = endSize - startSize;
            this.hide = hide;
        }

        protected override void onStart()
        {
            base.onStart();
            control.Height = startSize;
        }

        protected override void run(float delta)
        {
            base.run(delta);
            control.Height = (int)(startSize + (delta * delta));
        }

        protected override void onFinish()
        {
            base.onFinish();
            control.Height = startSize + delta;
            control.IsHidden = hide;
        }
    }
}
