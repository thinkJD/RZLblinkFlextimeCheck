using System;
using System.Drawing;

namespace RzlBlinkFlextime
{
    class BlinkOneHandler : IDisposable
    {
        Blink1Lib.Blink1 blink1;
        public BlinkOneHandler()
        {
            blink1 = new Blink1Lib.Blink1();
            blink1.open();
        }

        public void SetColorTo(System.Drawing.Color Color)
        {
            blink1.fadeToRGB(1000, Color.R, Color.G, Color.B);
        }

        public string ID
        {
            get { return blink1.getCachedSerial(0); }
        }

        public void Dispose()
        {
            if (blink1 != null)
            {
                blink1.fadeToRGB(500, 0, 0, 0);
                blink1.close();
            }
        }
    }
}
