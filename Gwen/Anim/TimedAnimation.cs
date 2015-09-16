using System;

namespace Gwen.Anim
{
    // Timed animation. Provides a useful base for animations.
    public class TimedAnimation : Animation
    {
        private bool started;
        private bool finished;
        private float start;
        private float end;
        private float ease;

        public override bool Finished { get { return finished; } }

        public TimedAnimation(float length, float delay = 0.0f, float ease = 1.0f)
        {
            start = Platform.Neutral.GetTimeInSeconds() + delay;
            end = start + length;
            this.ease = ease;
            started = false;
            finished = false;
        }

        protected override void think()
        {
            //base.Think();

            if (finished)
                return;

            float current = Platform.Neutral.GetTimeInSeconds();
            float secondsIn = current - start;
            if (secondsIn < 0.0)
                return;

            if (!started)
            {
                started = true;
                onStart();
            }

            float delta = secondsIn / (end - start);
            if (delta < 0.0f)
                delta = 0.0f;
            if (delta > 1.0f)
                delta = 1.0f;

            run((float)Math.Pow(delta, ease));

            if (delta == 1.0f)
            {
                finished = true;
                onFinish();
            }
        }

        // These are the magic functions you should be overriding

        protected virtual void onStart()
        { }

        protected virtual void run(float delta)
        { }

        protected virtual void onFinish()
        { }
    }
}
