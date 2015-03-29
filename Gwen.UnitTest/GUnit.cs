using System;
using Gwen.Control;

namespace Gwen.UnitTest
{
    public class GUnit : ControlBase
    {
        public UnitTest UnitTest;

        public GUnit(ControlBase parent) : base(parent)
        {
            
        }

        public void UnitPrint(string str)
        {
            if (UnitTest != null)
                UnitTest.PrintText(str);
        }
    }
}
