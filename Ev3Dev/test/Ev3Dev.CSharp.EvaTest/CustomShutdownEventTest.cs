using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class CustomShutdownEventTest
    {
        [CustomShutdownEvent("ValueChanged")]
        class SwitchAsShutdownEventModel
        {
            private int _value = 0;
            private int _iterationCounter = 0;

            [Switch]
            public int Value => _value;

            public int IterationCounter => _iterationCounter;

            [Action]
            public void Do()
            {
                if (_iterationCounter++ >= 3)
                    _value = 1;
            }
        }

        [Fact]
        public void SimpleSwitchTest()
        {
            var model = new SwitchAsShutdownEventModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(1, model.Value);
            Assert.Equal(4, model.IterationCounter);
        }
    }
}
