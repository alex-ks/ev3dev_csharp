using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class MutualExclusionTest
    {
        [MutualExclusion(nameof(Do))]
        class UnsynchronizedCausesExceptionModel
        {
            [Action]
            public void Do() { }
        }

        [Fact]
        public void UnsynchronizedCausesExceptionTest()
        {
            var model = new UnsynchronizedCausesExceptionModel();
            Assert.Throws<InvalidOperationException>(() => model.BuildLoop(allowEndless: true));
        }

        [MutualExclusion(nameof(DoA), nameof(DoB))]
        [MutualExclusion(nameof(DoB), nameof(DoC))]
        class MultiexclusionMethodCausesExceptionModel
        {
            [Action, Discardable]
            public void DoA() { }

            [Action, Discardable]
            public void DoB() { }

            [Action, Discardable]
            public void DoC() { }
        }

        [Fact]
        public void MultiexclusionMethodCausesExceptionTest()
        {
            var model = new MultiexclusionMethodCausesExceptionModel();
            Assert.Throws<ArgumentException>(() => model.BuildLoop(allowEndless: true));
        }

        [MutualExclusion(nameof(DoA), nameof(DoB))]
        class BothDiscardableExclusionModel
        {
            private int aCounter_ = 0;
            private int bCounter_ = 0;

            [ShutdownEvent]
            public bool ShouldStop => aCounter_ > 0 || bCounter_ > 0;

            public bool OnlyOneInvokation => aCounter_ == 1 ^ bCounter_ == 1;

            [Action, Discardable]
            public async Task DoA()
            {
                await Task.Delay(millisecondsDelay: 50);
                ++aCounter_;
            }

            [Action, Discardable]
            public async Task DoB()
            {
                await Task.Delay(millisecondsDelay: 50);
                ++bCounter_;
            }
        }

        [Fact]
        public void BothDiscardableExclusionTest()
        {
            var model = new BothDiscardableExclusionModel();
            var loop = model.BuildLoop();
            // Set cooldown large enough to allow one of the actions to execute and shutdown the loop.
            loop.Start(millisecondsCooldown: 100);
            Assert.True(model.OnlyOneInvokation);
        }
    }
}
