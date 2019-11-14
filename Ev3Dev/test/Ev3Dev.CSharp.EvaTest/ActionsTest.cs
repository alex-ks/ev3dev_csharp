using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.CSharp.EvATest
{
    public class ActionsTest
    {
        class SimpleActionModel
        {
            public int Counter { get; set; } = 0;

            [ShutdownEvent]
            public bool Called { get => Counter > 0; }

            [Action]
            public void Do()
            {
                ++Counter;
            }
        }

        [Fact]
        public void ActionCalled()
        {
            var model = new SimpleActionModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(1, model.Counter);
        }

        class SimpleAsyncActionModel
        {
            public int Counter { get; set; } = 0;

            [ShutdownEvent]
            public bool Called { get => Counter > 0; }

            [Action]
            public async Task Do()
            {
                await Task.Run(() => ++Counter);
            }
        }

        [Fact]
        public void AsyncActionCalled()
        {
            var model = new SimpleAsyncActionModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.True(model.Counter > 0);
        }

        class EndlessModel
        {
            [Action]
            public void Do()
            {
                Console.WriteLine("Done!");
            }
        }

        [Fact]
        public void EndlessNotAllowedByDefault()
        {
            var model = new EndlessModel();
            Assert.Throws<InvalidOperationException>(() => model.BuildLoop());
        }

        class PropertyForwardModel
        {
            public int Counter { get; set; } = 0;

            [ShutdownEvent]
            public bool Finished => Counter == 3;

            [Action]
            public void Do([FromSource(nameof(Counter))] int counter)
            {
                Assert.Equal(Counter, counter);
                ++Counter;
            }
        }

        [Fact]
        public void ArgumentsForwarded()
        {
            var model = new PropertyForwardModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(3, model.Counter);
        }

        class CapitalizedAsDefaultSourceModel
        {
            public int Counter { get; set; } = 0;

            [ShutdownEvent]
            public bool Finished => Counter == 3;

            [Action]
            public void Do(int counter)
            {
                Assert.Equal(Counter, counter);
                ++Counter;
            }
        }

        [Fact]
        public void CapitalizedUsedAsDefaultSource()
        {
            var model = new CapitalizedAsDefaultSourceModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(3, model.Counter);
        }

        class SourceUnknownModel
        {
            public int Counter { get; set; } = 0;

            [ShutdownEvent]
            public bool Finished => Counter == 3;

            [Action]
            public void Do(int someCounter)
            {
                Assert.Equal(Counter, someCounter);
                ++Counter;
            }
        }

        [Fact]
        public void NoSourceNotSupported()
        {
            var model = new SourceUnknownModel();
            Assert.Throws<InvalidOperationException>(() => model.BuildLoop());
        }

        class ActionOverloadedModel
        {
            public int Counter { get; set; } = 0;

            [ShutdownEvent]
            public bool Called { get => Counter > 0; }

            [Action]
            public void Do()
            {
                ++Counter;
            }

            [Action]
            public async Task Do([FromSource("Counter")]int counter)
            {
                await Task.Run(() => ++Counter);
            }
        }

        [Fact]
        public void ActionOverloadNotSupported()
        {
            var model = new ActionOverloadedModel();
            Assert.Throws<InvalidOperationException>(() => model.BuildLoop());
        }
    }
}
