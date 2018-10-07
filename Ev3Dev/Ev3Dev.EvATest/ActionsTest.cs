using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
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

    public class ActionsTest
    {
        [Fact]
        public void ActionCalled()
        {
            var model = new SimpleActionModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(1, model.Counter);
        }
    }
}
