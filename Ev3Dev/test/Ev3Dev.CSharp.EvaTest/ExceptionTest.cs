using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class ExceptionTest
    {
        public class MyCustomException : Exception { }

        class SimpleExceptionModel
        {
            [ShutdownEvent]
            public bool ShouldStop => false;

            // This exception shouldn't be catched at all.
            [Action]
            public void Do() => throw new MyCustomException();
        }

        [Fact]
        public void NoModifiersTest()
        {
            var model = new SimpleExceptionModel();
            var loop = model.BuildLoop();
            Assert.Throws<MyCustomException>(() => loop.Start());
        }

        class CriticalActionModel
        {
            [ShutdownEvent]
            public bool ShouldStop => false;

            [Action, Critical]
            public void Do() => throw new MyCustomException();
        }

        [Fact]
        public void CriticalActionTest()
        {
            var model = new CriticalActionModel();
            var loop = model.BuildLoop();
            loop.Start();
        }

        [EverythingIsCritical]
        class EverythingIsCriticalActionModel
        {
            [ShutdownEvent]
            public bool ShouldStop => false;

            [Action]
            public void Do() => throw new MyCustomException();
        }

        [Fact]
        public void EverythingIsCriticalActionTest()
        {
            var model = new EverythingIsCriticalActionModel();
            var loop = model.BuildLoop();
            loop.Start();
        }

        class NonCriticalActionModel
        {
            private int counter_ = 0;

            [ShutdownEvent]
            public bool ShouldStop => counter_ > 1;

            public int Counter => counter_;

            [Action, NonCritical]
            public void Do()
            {
                ++counter_;
                throw new MyCustomException();
            }
        }

        [Fact]
        public void NonCriticalActionTest()
        {
            var model = new NonCriticalActionModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(2, model.Counter);
        }

        [EverythingIsNonCritical]
        class EverythingIsNonCriticalActionModel
        {
            private int counter_ = 0;

            [ShutdownEvent]
            public bool ShouldStop => counter_ > 1;

            public int Counter => counter_;

            [Action]
            public void Do()
            {
                ++counter_;
                throw new MyCustomException();
            }
        }

        [Fact]
        public void EverythingIsNonCriticalActionTest()
        {
            var model = new EverythingIsNonCriticalActionModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(2, model.Counter);
        }

        [EverythingIsCritical]
        class EverythingIsCriticalExceptActionModel
        {
            private int counter_ = 0;

            [ShutdownEvent]
            public bool ShouldStop => counter_ > 1;

            public int Counter => counter_;

            [Action, NonCritical]
            public void Do()
            {
                ++counter_;
                throw new MyCustomException();
            }
        }

        [Fact]
        public void EverythingIsCriticalExceptActionTest()
        {
            var model = new EverythingIsNonCriticalActionModel();
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(2, model.Counter);
        }

        [EverythingIsNonCritical]
        class EverythingIsNonCriticalExceptActionModel
        {
            [ShutdownEvent]
            public bool ShouldStop => false;

            [Action, Critical]
            public void Do() => throw new MyCustomException();
        }

        [Fact]
        public void EverythingIsNonCriticalExceptActionTest()
        {
            var model = new EverythingIsNonCriticalExceptActionModel();
            var loop = model.BuildLoop();
            loop.Start();
        }
    }
}
