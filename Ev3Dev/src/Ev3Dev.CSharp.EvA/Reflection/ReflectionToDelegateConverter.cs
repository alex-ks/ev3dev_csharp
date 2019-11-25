using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.Reflection
{
    /// <summary>
    /// Extracts delegate from reflection of method with unknown signature.
    /// See https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/
    /// for implementation details
    /// </summary>
    public static class ReflectionToDelegateConverter
    {
        // Functions
        private static Func<TRet> CreateFuncZeroArgGeneric<TRet>(object target, MethodInfo method)
        {
            var func = Delegate.CreateDelegate(typeof(Func<TRet>), target, method) as Func<TRet>;
            return () => func();
        }

        public static Delegate CreateFuncZeroArgRaw(object target, MethodInfo method)
        {
            var getterType = typeof(Func<>).MakeGenericType(method.ReturnType);
            return Delegate.CreateDelegate(getterType, target, method);
        }

        // We can create delegate from reflection only if we know exact signature, so we use genereric method
        private static Func<TRet> CreateFunc1ArgGeneric<TArg, TRet>(object target,
                                                                    MethodInfo method,
                                                                    Delegate argGetter)
        {
            var func = Delegate.CreateDelegate(typeof(Func<TArg, TRet>), target, method) as Func<TArg, TRet>;
            var getter = argGetter as Func<TArg>;
            if (getter == null)
                throw new ArgumentException("argGetter does not have expected signature.");
            return () => func(getter());
        }

        public static Delegate CreateFunc1Arg(object target, MethodInfo method, Delegate argGetter)
        {
            if (method.GetParameters().Length != 1)
            { throw new ArgumentException(string.Format(Resources.WrongParamsCount, 1)); }

            var genericCreator = typeof(ReflectionToDelegateConverter)
                .GetMethod(nameof(CreateFunc1ArgGeneric),
                           BindingFlags.Static | BindingFlags.NonPublic);
            // Getting parameters types and manually setting them to generic method
            // now, at runtime types of parameters will be known and CreateFunc1ArgGeneric
            // will successfully create delegate.
            var correctCreator = genericCreator.MakeGenericMethod(method.GetParameters()[0].ParameterType,
                                                                  method.ReturnType);
            return correctCreator.Invoke(null, new[] { target, method, argGetter }) as Delegate;
        }

        // Same trick for all other methods.
        private static Func<TRet> CreateFunc2ArgsGeneric<TArg1, TArg2, TRet>(object target,
                                                                             MethodInfo method,
                                                                             Delegate argGetter1,
                                                                             Delegate argGetter2)
        {
            var func = Delegate.CreateDelegate(typeof(Func<TArg1, TArg2, TRet>),
                                               target,
                                               method) as Func<TArg1, TArg2, TRet>;

            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");

            return () => func(getter1(), getter2());
        }

        private static Func<TRet> CreateFunc3ArgsGeneric<TArg1, TArg2, TArg3, TRet>(object target,
                                                                                    MethodInfo method,
                                                                                    Delegate argGetter1,
                                                                                    Delegate argGetter2,
                                                                                    Delegate argGetter3)
        {
            var func = Delegate.CreateDelegate(typeof(Func<TArg1, TArg2, TArg3, TRet>),
                                                target,
                                                method) as Func<TArg1, TArg2, TArg3, TRet>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            var getter3 = argGetter3 as Func<TArg3>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            if (getter3 == null)
                throw new ArgumentException("argGetter3 does not have expected signature.");
            return () => func(getter1(), getter2(), getter3());
        }

        private static Func<TRet> CreateFunc4ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TRet>(
            object target,
            MethodInfo method,
            Delegate argGetter1,
            Delegate argGetter2,
            Delegate argGetter3,
            Delegate argGetter4)
        {
            var func = Delegate.CreateDelegate(typeof(Func<TArg1, TArg2, TArg3, TArg4, TRet>),
                                                target,
                                                method) as Func<TArg1, TArg2, TArg3, TArg4, TRet>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            var getter3 = argGetter3 as Func<TArg3>;
            var getter4 = argGetter4 as Func<TArg4>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            if (getter3 == null)
                throw new ArgumentException("argGetter3 does not have expected signature.");
            if (getter4 == null)
                throw new ArgumentException("argGetter4 does not have expected signature.");
            return () => func(getter1(), getter2(), getter3(), getter4());
        }

        private static Func<TRet> CreateFunc5ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TArg5, TRet>(
            object target,
            MethodInfo method,
            Delegate argGetter1,
            Delegate argGetter2,
            Delegate argGetter3,
            Delegate argGetter4,
            Delegate argGetter5)
        {
            var func = Delegate.CreateDelegate(typeof(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TRet>),
                                                target,
                                                method) as Func<TArg1, TArg2, TArg3, TArg4, TArg5, TRet>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            var getter3 = argGetter3 as Func<TArg3>;
            var getter4 = argGetter4 as Func<TArg4>;
            var getter5 = argGetter5 as Func<TArg5>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            if (getter3 == null)
                throw new ArgumentException("argGetter3 does not have expected signature.");
            if (getter4 == null)
                throw new ArgumentException("argGetter4 does not have expected signature.");
            if (getter5 == null)
                throw new ArgumentException("argGetter5 does not have expected signature.");
            return () => func(getter1(), getter2(), getter3(), getter4(), getter5());
        }

        public static Delegate CreateStrictFunc(object target,
                                                MethodInfo method,
                                                params Delegate[] argGetters)
        {
            var parameters = method.GetParameters();
            string creatorName = null;
            switch (parameters.Length)
            {
                case 0:
                    creatorName = nameof(CreateFuncZeroArgGeneric);
                    break;
                case 1:
                    creatorName = nameof(CreateFunc1ArgGeneric);
                    break;
                case 2:
                    creatorName = nameof(CreateFunc2ArgsGeneric);
                    break;
                case 3:
                    creatorName = nameof(CreateFunc3ArgsGeneric);
                    break;
                case 4:
                    creatorName = nameof(CreateFunc4ArgsGeneric);
                    break;
                case 5:
                    creatorName = nameof(CreateFunc5ArgsGeneric);
                    break;
                default:
                    throw new ArgumentException("Method must have less than 5 parameters.");
            }
            var genericCreator = typeof(ReflectionToDelegateConverter)
                .GetMethod(creatorName, BindingFlags.Static | BindingFlags.NonPublic);
            var typeParameters = parameters.Select(p => p.ParameterType).Append(method.ReturnType).ToArray();
            var exactCreator = genericCreator.MakeGenericMethod(typeParameters);
            var exactCreatorArgs = Enumerable.Empty<object>().Append(target).Append(method).Concat(argGetters);
            return exactCreator.Invoke(null, exactCreatorArgs.ToArray()) as Delegate;
        }

        // Actions
        public static Action CreateStrictActionZeroArg(object target, MethodInfo method)
        {
            var act = Delegate.CreateDelegate(typeof(Action), target, method) as Action;
            return act;
        }

        private static Action CreateStrictAction1ArgGeneric<TArg>(object target, MethodInfo method, Delegate argGetter)
        {
            var act = Delegate.CreateDelegate(typeof(Action<TArg>), target, method) as Action<TArg>;
            var getter = argGetter as Func<TArg>;
            if (getter == null)
                throw new ArgumentException("argGetter does not have expected signature.");
            return () => act(getter());
        }

        private static Action CreateStrictAction2ArgsGeneric<TArg1, TArg2>(object target,
                                                                        MethodInfo method,
                                                                        Delegate argGetter1,
                                                                        Delegate argGetter2)
        {
            var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2>), target, method) as Action<TArg1, TArg2>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            return () => act(getter1(), getter2());
        }

        private static Action CreateStrictAction3ArgsGeneric<TArg1, TArg2, TArg3>(object target,
                                                                               MethodInfo method,
                                                                               Delegate argGetter1,
                                                                               Delegate argGetter2,
                                                                               Delegate argGetter3)
        {
            var act = Delegate.CreateDelegate(typeof(Action<TArg1,
                                                            TArg2,
                                                            TArg3>), target, method) as Action<TArg1, TArg2, TArg3>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            var getter3 = argGetter3 as Func<TArg3>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            if (getter3 == null)
                throw new ArgumentException("argGetter3 does not have expected signature.");
            return () => act(getter1(), getter2(), getter3());
        }

        private static Action CreateStrictAction4ArgsGeneric<TArg1, TArg2, TArg3, TArg4>(object target,
                                                                                      MethodInfo method,
                                                                                      Delegate argGetter1,
                                                                                      Delegate argGetter2,
                                                                                      Delegate argGetter3,
                                                                                      Delegate argGetter4)
        {
            var act = Delegate.CreateDelegate(typeof(Action<TArg1,
                                                            TArg2,
                                                            TArg3,
                                                            TArg4>), target, method) as Action<TArg1,
                                                                                               TArg2,
                                                                                               TArg3,
                                                                                               TArg4>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            var getter3 = argGetter3 as Func<TArg3>;
            var getter4 = argGetter4 as Func<TArg4>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            if (getter3 == null)
                throw new ArgumentException("argGetter3 does not have expected signature.");
            if (getter4 == null)
                throw new ArgumentException("argGetter4 does not have expected signature.");
            return () => act(getter1(), getter2(), getter3(), getter4());
        }

        private static Action CreateStrictAction5ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TArg5>(
            object target,
            MethodInfo method,
            Delegate argGetter1,
            Delegate argGetter2,
            Delegate argGetter3,
            Delegate argGetter4,
            Delegate argGetter5)
        {
            var act = Delegate.CreateDelegate(typeof(Action<TArg1, TArg2, TArg3, TArg4, TArg5>),
                                              target, method) as Action<TArg1, TArg2, TArg3, TArg4, TArg5>;
            var getter1 = argGetter1 as Func<TArg1>;
            var getter2 = argGetter2 as Func<TArg2>;
            var getter3 = argGetter3 as Func<TArg3>;
            var getter4 = argGetter4 as Func<TArg4>;
            var getter5 = argGetter5 as Func<TArg5>;
            if (getter1 == null)
                throw new ArgumentException("argGetter1 does not have expected signature.");
            if (getter2 == null)
                throw new ArgumentException("argGetter2 does not have expected signature.");
            if (getter3 == null)
                throw new ArgumentException("argGetter3 does not have expected signature.");
            if (getter4 == null)
                throw new ArgumentException("argGetter4 does not have expected signature.");
            if (getter5 == null)
                throw new ArgumentException("argGetter5 does not have expected signature.");
            return () => act(getter1(), getter2(), getter3(), getter4(), getter5());
        }

        public static Action CreateStrictAction(object target,
                                                MethodInfo method,
                                                params Delegate[] argGetters)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                return CreateStrictActionZeroArg(target, method);
            string creatorName = null;
            switch (parameters.Length)
            {
                case 1:
                    creatorName = nameof(CreateStrictAction1ArgGeneric);
                    break;
                case 2:
                    creatorName = nameof(CreateStrictAction2ArgsGeneric);
                    break;
                case 3:
                    creatorName = nameof(CreateStrictAction3ArgsGeneric);
                    break;
                case 4:
                    creatorName = nameof(CreateStrictAction4ArgsGeneric);
                    break;
                case 5:
                    creatorName = nameof(CreateStrictAction5ArgsGeneric);
                    break;
                default:
                    throw new ArgumentException("Method must have less than 5 parameters.");
            }
            var genericCreator = typeof(ReflectionToDelegateConverter)
                .GetMethod(creatorName, BindingFlags.Static | BindingFlags.NonPublic);
            var typeParameters = parameters.Select(p => p.ParameterType).ToArray();
            var exactCreator = genericCreator.MakeGenericMethod(typeParameters);
            var exactCreatorArgs = Enumerable.Empty<object>().Append(target).Append(method).Concat(argGetters);
            return exactCreator.Invoke(null, exactCreatorArgs.ToArray()) as Action;
        }

        private static Func<object> GeneralizeGetterGeneric<T>(Delegate getter)
        {
            var func = getter as Func<T>;
            return () => func();
        }

        public static Func<object> GeneralizeGetter(Delegate getter)
        {
            if (getter.Method.GetParameters().Length != 0)
                throw new ArgumentException("Getter should have no arguments");
            var genericCreator = typeof(ReflectionToDelegateConverter)
                            .GetMethod(nameof(GeneralizeGetterGeneric),
                                       BindingFlags.Static | BindingFlags.NonPublic);
            var exactCreator = genericCreator.MakeGenericMethod(getter.Method.ReturnType);
            return exactCreator.Invoke(null, new[] { getter }) as Func<object>;
        }
    }
}
