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
            public void Do([FromSource("ValueChanged")] bool changed)
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
            public void OneDo([FromSource("ValueChanged")] bool changed)
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
            public void AnotherDo([FromSource("ValueChanged")] bool changed)
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

        // As we change the switching value inside an action, we need to specify strict ordering rules
        // to make switch behaviour deterministic.
        [PriorityOrdering]
        class SwitchAsEventModel
        {
            private int _value = 0;
            private int _iterationCounter = 0;

            [ShutdownEvent]
            public bool Finished => _iterationCounter >= 4;

            [Switch]
            public int Value => _value;

            public int HandlerInvokeCount { get; set; } = 0;

            [Action, Priority(1)]
            public void Do()
            {
                if (_iterationCounter++ % 2 == 1)
                    _value++;
            }

            [EventHandler("ValueChanged"), Priority(2)]
            public void HandleEvent(bool valueChanged)
            {
                Assert.True(valueChanged);
                ++HandlerInvokeCount;
            }
        }

        [Fact]
        public void SwitchAsEventTest()
        {
            var model = new SwitchAsEventModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(2, model.HandlerInvokeCount);
        }

        [PriorityOrdering]
        public class BooleanSwitchModel
        {
            private int _iterationCounter = 0;

            [ShutdownEvent]
            public bool ShouldStop => _iterationCounter > 2;

            [Switch]
            public bool Flag { get; set; } = true;

            [Action, Priority(1)]
            public void Do()
            {
                if (_iterationCounter++ % 2 == 1)
                    Flag = false;
            }

            [EventHandler("FlagChanged"), Priority(2)]
            public void HandleEvent(bool flag, bool flagChanged)
            {
                Assert.False(flag);
                Assert.True(flagChanged);
            }
        }

        [Fact]
        public void BooleanSwitchTest()
        {
            var model = new BooleanSwitchModel();
            var loop = model.BuildLoop();
            loop.Start();
        }
    }
}
