using System;
using System.Collections.Generic;
using System.Reflection;
using Ev3Dev.CSharp.EvA;
using Ev3Dev.CSharp.EvA.AttributeContracts;
using BenchmarkDotNet.Attributes;

namespace Ev3Dev.CSharp.EvaBenchmark
{
    public class ActionGenerationBenchmark
    {
        private int Arg1 => 1;
        private char Arg2 => '2';
        private double Arg3 => 3.0;
        private bool Arg4 => true;
        private string Arg5 => "5";
        private decimal Arg6 => 6.0m;

        public void ZeroArgMethod() { }

        public void OneArgMethod(int arg1) { }

        public void TwoArgMethod(int arg1, char arg2) { }

        public void ThreeArgMethod(int arg1, char arg2, double arg3) { }

        public void FourArgMethod(int arg1, char arg2, double arg3, bool arg4) { }

        public void FiveArgMethod(int arg1, char arg2, double arg3, bool arg4, string arg5) { }

        public void SixArgMethod(int arg1, char arg2, double arg3, bool arg4, string arg5, decimal arg6) { }

        private Dictionary<string, PropertyWrapper> _properties;

        private Action _zeroArgAction;
        private Action _oneArgAction;
        private Action _twoArgAction;
        private Action _threeArgAction;
        private Action _fourArgAction;
        private Action _fiveArgAction;
        private Action _sixArgAction;

        public ActionGenerationBenchmark()
        {
            Func<object> arg1Getter = () => Arg1;
            Func<object> arg2Getter = () => Arg2;
            Func<object> arg3Getter = () => Arg3;
            Func<bool> arg4Getter = () => Arg4;
            Func<object> arg5Getter = () => Arg5;
            Func<object> arg6Getter = () => Arg6;

            _properties = new Dictionary<string, PropertyWrapper>()
            {
                { nameof(Arg1), new PropertyWrapper(Arg1.GetType(), arg1Getter) },
                { nameof(Arg2), new PropertyWrapper(Arg2.GetType(), arg2Getter) },
                { nameof(Arg3), new PropertyWrapper(Arg3.GetType(), arg3Getter) },
                { nameof(Arg4), new PropertyWrapper(Arg4.GetType(), arg4Getter) },
                { nameof(Arg5), new PropertyWrapper(Arg5.GetType(), arg5Getter) },
                { nameof(Arg6), new PropertyWrapper(Arg6.GetType(), arg6Getter) }
            };

            _zeroArgAction = GenerateAction(nameof(ZeroArgMethod));
            _oneArgAction = GenerateAction(nameof(OneArgMethod));
            _twoArgAction = GenerateAction(nameof(TwoArgMethod));
            _threeArgAction = GenerateAction(nameof(ThreeArgMethod));
            _fourArgAction = GenerateAction(nameof(FourArgMethod));
            _fiveArgAction = GenerateAction(nameof(FiveArgMethod));
            _sixArgAction = GenerateAction(nameof(SixArgMethod));
        }

        private Action GenerateAction(string methodName)
        {
            var method = this.GetType().GetMethod(methodName,
                                                  BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentNullException(methodName);
            var actionAttribute = new ActionAttribute();
            return actionAttribute.ExtractAction(this, method, _properties);
        }

        [Benchmark]
        public void RegularZeroArgCall() => ZeroArgMethod();

        [Benchmark]
        public void RegularOneArgCall() => OneArgMethod(Arg1);

        [Benchmark]
        public void RegularTwoArgCall() => TwoArgMethod(Arg1, Arg2);

        [Benchmark]
        public void RegularThreeArgCall() => ThreeArgMethod(Arg1, Arg2, Arg3);

        [Benchmark]
        public void RegularFourArgCall() => FourArgMethod(Arg1, Arg2, Arg3, Arg4);

        [Benchmark]
        public void RegularFiveArgCall() => FiveArgMethod(Arg1, Arg2, Arg3, Arg4, Arg5);

        [Benchmark]
        public void RegularSixArgCall() => SixArgMethod(Arg1, Arg2, Arg3, Arg4, Arg5, Arg6);

        [Benchmark]
        public void ZeroArgAction() => _zeroArgAction();

        [Benchmark]
        public void OneArgAction() => _oneArgAction();

        [Benchmark]
        public void TwoArgAction() => _twoArgAction();

        [Benchmark]
        public void ThreeArgAction() => _threeArgAction();

        [Benchmark]
        public void FourArgAction() => _fourArgAction();

        [Benchmark]
        public void FiveArgAction() => _fiveArgAction();

        [Benchmark]
        public void SixArgAction() => _sixArgAction();
    }
}