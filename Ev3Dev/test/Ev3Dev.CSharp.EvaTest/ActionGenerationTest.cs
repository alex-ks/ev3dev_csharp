using System;
using System.Collections.Generic;
using System.Reflection;
using Ev3Dev.CSharp.EvA;
using Ev3Dev.CSharp.EvA.Reflection;
using Xunit;

namespace Ev3Dev.CSharp.EvaTest
{
    public class ActionGenerationTest
    {
        public static int Arg1 => 1;
        public static char Arg2 => '2';
        public static double Arg3 => 3.0;
        public static bool Arg4 => true;
        public static string Arg5 => "5";
        public static decimal Arg6 => 6.0m;

        void ZeroArgAction() { }

        void OneArgAction(int arg1)
        {
            Assert.Equal(Arg1, arg1);
        }

        void TwoArgAction(int arg1, char arg2)
        {
            Assert.Equal(Arg1, arg1);
            Assert.Equal(Arg2, arg2);
        }

        void ThreeArgAction(int arg1, char arg2, double arg3)
        {
            Assert.Equal(Arg1, arg1);
            Assert.Equal(Arg2, arg2);
            Assert.Equal(Arg3, arg3);
        }

        void FourArgAction(int arg1, char arg2, double arg3, bool arg4)
        {
            Assert.Equal(Arg1, arg1);
            Assert.Equal(Arg2, arg2);
            Assert.Equal(Arg3, arg3);
            Assert.Equal(Arg4, arg4);
        }

        void FiveArgAction(int arg1, char arg2, double arg3, bool arg4, string arg5)
        {
            Assert.Equal(Arg1, arg1);
            Assert.Equal(Arg2, arg2);
            Assert.Equal(Arg3, arg3);
            Assert.Equal(Arg4, arg4);
            Assert.Equal(Arg5, arg5);
        }

        void SixArgAction(int arg1, char arg2, double arg3, bool arg4, string arg5, decimal arg6)
        {
            Assert.Equal(Arg1, arg1);
            Assert.Equal(Arg2, arg2);
            Assert.Equal(Arg3, arg3);
            Assert.Equal(Arg4, arg4);
            Assert.Equal(Arg5, arg5);
            Assert.Equal(Arg6, arg6);
        }

        public static IEnumerable<object[]> Data
        {
            get
            {
                Func<int> arg1Getter = () => Arg1;
                Func<char> arg2Getter = () => Arg2;
                Func<double> arg3Getter = () => Arg3;
                Func<bool> arg4Getter = () => Arg4;
                Func<string> arg5Getter = () => Arg5;
                Func<decimal> arg6Getter = () => Arg6;

                var properties = new Dictionary<string, ICachingDelegate>();
                yield return new object[] { properties, nameof(ZeroArgAction) };

                properties.Add(nameof(Arg1), new CachingFunction<int>(arg1Getter));
                yield return new object[] { properties, nameof(OneArgAction) };

                properties.Add(nameof(Arg2), new CachingFunction<char>(arg2Getter));
                yield return new object[] { properties, nameof(TwoArgAction) };

                properties.Add(nameof(Arg3), new CachingFunction<double>(arg3Getter));
                yield return new object[] { properties, nameof(ThreeArgAction) };

                properties.Add(nameof(Arg4), new CachingFunction<bool>(arg4Getter));
                yield return new object[] { properties, nameof(FourArgAction) };

                properties.Add(nameof(Arg5), new CachingFunction<string>(arg5Getter));
                yield return new object[] { properties, nameof(FiveArgAction) };

                properties.Add(nameof(Arg6), new CachingFunction<decimal>(arg6Getter));
                yield return new object[] { properties, nameof(SixArgAction) };
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void TestAll(Dictionary<string, ICachingDelegate> properties, string methodName)
        {
            var method = this.GetType().GetMethod(methodName,
                                                  BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            var actionAttribute = new ActionAttribute();
            var action = actionAttribute.ExtractAction(this, method, properties);
            action();
        }
    }
}