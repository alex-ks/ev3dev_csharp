using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA.Reflection
{
    /// <summary>
    /// Provides methods for delegates creation from reflection.
    /// </summary>
    public class DelegateGenerator
    {
        /// <summary>
        /// Creates <see cref="Func{TResult}"/> to access property value.
        /// Performs optimizations for actions with parameters count less or equal 5 -
        /// in this case methods are called via delegates, otherwise via reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Object-owner of a property</param>
        /// <param name="property">Property for wich value the getter will be created</param>
        /// <returns><see cref="Func{TResult}"/> representing property getter</returns>
        public static Func<T> CreateGetter<T>(object target, PropertyInfo property)
        {
            return Delegate.CreateDelegate(typeof(Func<T>),
                                           target,
                                           property.GetMethod) as Func<T>;
        }

        public static Delegate CreateRawGetter(object target, PropertyInfo property)
        {
            return ReflectionToDelegateConverter.CreateFuncZeroArgRaw(target, property.GetMethod);
        }

        /// <summary>
        /// Creates <see cref="System.Action"/> to call method with no return value.
        /// Method parameters are to be passed as array of objects.
        /// Performs optimizations for actions with parameters count less or equal 5 -
        /// in this case methods are called directly via delegates, otherwise via reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <param name="argumentGetters">Zero-argument getters for the method argumets</param>
        /// <returns><see cref="Action"/> object to perform the action</returns>
        public static Action GenerateAction(object target, MethodInfo method, Delegate[] argumentGetters)
        {
            if (argumentGetters == null)
                throw new ArgumentNullException(nameof(argumentGetters));

            if (method.ReturnType != typeof(void))
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidAction,
                                                                  method.Name));
            }

            var parametersCount = method.GetParameters().Length;

            if (parametersCount != argumentGetters.Length)
                throw new InvalidOperationException("Incorrect argument count provided.");

            // create direct delegate invocation if argument count is small
            if (parametersCount <= 5)
                return ReflectionToDelegateConverter.CreateStrictAction(target, method, argumentGetters);
            var generalizedGetters =
                argumentGetters.Select(g => ReflectionToDelegateConverter.GeneralizeGetter(g)).ToArray();
            return () =>
            {
                var args = generalizedGetters.Select(g => g()).ToArray();
                method.Invoke(target, args);
            };
        }

        /// <summary>
        /// Creates function to call an async method.
        /// Method parameters are to be passed as array of objects.
        /// Performs optimizations for actions with parameters count less or equal 5 -
        /// in this case methods are called via delegates, otherwise via reflection.
        /// </summary>
        /// <param name="target">Object-owner of the action</param>
        /// <param name="method"><see cref="MethodInfo"/> object representing action</param>
        /// <returns><see cref="Func{Task}"/> object to perform the action</returns>
        public static Func<Task> GenerateAsyncAction(object target, MethodInfo method, Delegate[] argumentGetters)
        {
            var parametersCount = method.GetParameters().Length;

            // Create direct delegate invocation if argument count is small.
            if (parametersCount <= 5)
                return ReflectionToDelegateConverter.CreateStrictFunc(target, method, argumentGetters) as Func<Task>;

            // Otherwise perform slow invocation through reflection.
            var args = argumentGetters.Select(g => g.DynamicInvoke()).ToArray();
            return () => method.Invoke(target, args) as Task;
        }

        /// <summary>
        /// Checks if method represented by <see cref="MethodInfo"/> is async.
        /// </summary>
        /// <param name="method">Method to check for async-ness</param>
        /// <returns>True if method is marked as async, false otherwise</returns>
        public static bool IsAsync(MethodInfo method)
        {
            return method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        }
    }
}
