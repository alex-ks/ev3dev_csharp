using Ev3Dev.CSharp.EvA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ev3Dev.EvATest
{
    public class PriorityTest
    {
        public class PriorityModelBase
        {
            protected int counter_ = 0;

            public int Do1Order { get; set; }
            public int Do2Order { get; set; }
            public int Do3Order { get; set; }

            [ShutdownEvent]
            public bool ShouldStop => counter_ > 0;
        }

        [PriorityOrdering]
        class SimplePriorityModel : PriorityModelBase
        {
            [Action, Priority(3)]
            public void Do1() { Do1Order = counter_++; }

            [Action, Priority(1)]
            public void Do2() { Do2Order = counter_++; }

            [Action, Priority(2)]
            public void Do3() { Do3Order = counter_++; }
        }

        [PriorityOrdering]
        class DefaultPriorityModel : PriorityModelBase
        {
            [Action, Priority(11)]
            public void Do1() { Do1Order = counter_++; }

            [Action, Priority(1)]
            public void Do2() { Do2Order = counter_++; }

            [Action]
            public void Do3() { Do3Order = counter_++; }
        }

        [PriorityOrdering(DefaultPriority = 2)]
        class CustomDefaultPriorityModel : PriorityModelBase
        {
            [Action, Priority(3)]
            public void Do1() { Do1Order = counter_++; }

            [Action, Priority(1)]
            public void Do2() { Do2Order = counter_++; }

            [Action]
            public void Do3() { Do3Order = counter_++; }
        }

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new[] { new SimplePriorityModel() };
            yield return new[] { new DefaultPriorityModel() };
            yield return new[] { new CustomDefaultPriorityModel() };
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void GenericPriorityTest(PriorityModelBase model)
        {
            var loop = model.BuildLoop();
            loop.Start();
            Assert.Equal(0, model.Do2Order);
            Assert.Equal(1, model.Do3Order);
            Assert.Equal(2, model.Do1Order);
        }
    }
}
