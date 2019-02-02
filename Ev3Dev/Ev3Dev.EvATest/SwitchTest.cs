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
            private int value_ = 0;
            private int iterationCounter_ = 0;

            [ShutdownEvent]
            public bool Finished => iterationCounter_ > 4;

            [Switch]
            public int Value => value_;

            [Action]
            public void Do([FromSource("Value")] bool changed)
            {
                switch (iterationCounter_++)
                {
                    case 0:
                        Assert.False(changed);
                        break;

                    case 1:
                        Assert.False(changed);
                        ++value_;
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
            private int value_ = 0;
            private int fstIterationCounter_ = 0;
            private int sndIterationCounter_ = 0;

            [ShutdownEvent]
            public bool Finished => fstIterationCounter_ > 2;

            [Switch]
            public int Value => value_;

            [Action]
            public void OneDo([FromSource("Value")] bool changed)
            {
                switch (fstIterationCounter_++)
                {
                    case 0:
                        Assert.False(changed);
                        ++value_;
                        break;

                    case 1:
                        Assert.True(changed);
                        break;
                }
            }

            [Action]
            public void AnotherDo([FromSource("Value")] bool changed)
            {
                switch (sndIterationCounter_++)
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
