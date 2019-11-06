using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class SwitchTest
    {
        class SimpleSwitchModel
        {
            private int _value = 0;
            private int _iterationCounter = 0;

            [ShutdownEvent]
            public bool Finished => _iterationCounter > 4;

            [Switch]
            public int Value => _value;

            [Action]
            public void Do([FromSource("Value")] bool changed)
            {
                switch (_iterationCounter++)
                {
                    case 0:
                        Assert.False(changed);
                        break;

                    case 1:
                        Assert.False(changed);
                        ++_value;
                        break;

                    case 2:
                        Assert.True(changed);
                        break;

                    case 3:
                        Assert.False(changed);
                        break;
                }
            }
        }

        [Fact]
        public void SimpleSwitchTest()
        {
            var model = new SimpleSwitchModel();
            var loop = model.BuildLoop();
            loop.Start();
        }

        class MultipleSwitchUsagesModel
        {
            private int _value = 0;
            private int _fstIterationCounter = 0;
            private int _sndIterationCounter = 0;

            [ShutdownEvent]
            public bool Finished => _fstIterationCounter > 2;

            [Switch]
            public int Value => _value;

            [Action]
            public void OneDo([FromSource("Value")] bool changed)
            {
                switch (_fstIterationCounter++)
                {
                    case 0:
                        Assert.False(changed);
                        ++_value;
                        break;

                    case 1:
                        Assert.True(changed);
                        break;
                }
            }

            [Action]
            public void AnotherDo([FromSource("Value")] bool changed)
            {
                switch (_sndIterationCounter++)
                {
                    case 0:
                        Assert.False(changed);
                        break;

                    case 1:
                        Assert.True(changed);
                        break;
                }
            }
        }

        [Fact]
        public void MultipleSwitchUsagesTest()
        {
            var model = new MultipleSwitchUsagesModel();
            var loop = model.BuildLoop();
            loop.Start();
        }
    }
}
