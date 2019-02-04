using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class EventHandlerTest
    {
        class SimpleEventHandlerModel
        {
            private int _iterationCount;
            private List<int> _evenIterations = new List<int>();
            private List<int> _evenOrOddIterations = new List<int>();
            private List<int> _evenAndOddIterations = new List<int>();

            public int IterationNumber => _iterationCount;
            public bool IsEven => _iterationCount % 2 == 0;
            public bool IsOdd => !IsEven;

            public List<int> EvenIterations => _evenIterations;
            public List<int> EvenOrOdd => _evenOrOddIterations;
            public List<int> EvenAndOdd => _evenAndOddIterations;

            [ShutdownEvent]
            public bool Finished => _iterationCount >= 6;

            [Action]
            public void IncrementIteration() => ++_iterationCount;

            [EventHandler("IsEven")]
            public void RegisterEven([FromSource("IterationNumber")]int number) => _evenIterations.Add(number);

            [EventHandler(CompositionType.Or, "IsEven", "IsOdd")]
            public void RegisterOr([FromSource("IterationNumber")]int number) => _evenOrOddIterations.Add(number);

            [EventHandler(CompositionType.And, "IsEven", "IsOdd")]
            public void RegisterAnd([FromSource("IterationNumber")]int number) => _evenAndOddIterations.Add(number);
        }

        [Fact]
        public void SimpleEventHandlerTest()
        {
            var model = new SimpleEventHandlerModel();
            var loop = model.BuildLoop(loadPropertiesLazily: false);
            loop.Start();
            Assert.Equal(new[] { 0, 2, 4 }, model.EvenIterations);
            Assert.Equal(Enumerable.Range(0, 6), model.EvenOrOdd);
            Assert.Empty(model.EvenAndOdd);
        }
    }
}
