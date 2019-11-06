using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class NonReenterableTest
    {
        class DiscardingNonReenterableModel
        {
            public int Counter { get; set; } = 0;
            public int AsyncCounter { get; set; } = 0;

            [ShutdownEvent]
            public bool Called { get => Counter >= 3; }

            [Action]
            public void Do() => ++Counter;

            [Action, Discardable]
            public async Task DoAsync()
            {
                ++AsyncCounter;
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
        }

        [Fact]
        public async Task NonReenterableActionDiscarded()
        {
            var model = new DiscardingNonReenterableModel();
            var loop = model.BuildLoop();
            loop.Start();
            // If DoAsync isn't discarded, all three task executions will have enough time to finish.
            await Task.Delay(TimeSpan.FromSeconds(1.6));
            Assert.Equal(1, model.AsyncCounter);
        }

        class CumulativeNonReenterableModel
        {
            public int Counter { get; set; } = 0;
            public int AsyncCounter { get; set; } = 0;

            [ShutdownEvent]
            public bool Called { get => Counter >= 3; }

            [Action]
            public void Do() => ++Counter;

            [Action, Cumulative]
            public async Task DoAsync()
            {
                ++AsyncCounter;
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
        }

        [Fact]
        public async Task NonReenterableActionAccumulated()
        {
            var model = new CumulativeNonReenterableModel();
            var loop = model.BuildLoop();
            loop.Start();
            await Task.Delay(TimeSpan.FromSeconds(0.25));
            Assert.Equal(1, model.AsyncCounter);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.Equal(2, model.AsyncCounter);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.Equal(3, model.AsyncCounter);
        }

        class AwaitableNonReenterableModel
        {
            public int Counter { get; set; } = 0;
            public int AsyncCounter { get; set; } = 0;

            [ShutdownEvent]
            public bool Called { get => Counter >= 3; }

            [Action]
            public void Do() => ++Counter;

            [Action, Awaitable]
            public async Task DoAsync([FromSource("Counter")] int counter)
            {
                Assert.Equal(AsyncCounter, counter);
                ++AsyncCounter;
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
        }

        [Fact]
        public async Task NonReenterableActionSequenced()
        {
            var model = new CumulativeNonReenterableModel();
            var loop = model.BuildLoop();
            loop.Start();
            // Wait for last task execution.
            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }
    }
}
