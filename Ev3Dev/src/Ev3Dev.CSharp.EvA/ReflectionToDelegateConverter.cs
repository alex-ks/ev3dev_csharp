using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Extracts delegate from reflection of method with unknown signature.
    /// See http://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
    /// for implementation details
    /// </summary>
    public class ReflectionToDelegateConverter
    {
        // Functions

        private static Func<object> CreateFuncZeroArgGeneric<TRet>( object target, MethodInfo method )
        {
            var func = Delegate.CreateDelegate( typeof( Func<TRet> ), target, method ) as Func<TRet>;
            return ( ) => func( );
        }

        public static Func<object> CreateFuncZeroArg( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 0 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, "no" ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateFuncZeroArgGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.ReturnType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Func<object>;
        }

        // We can create delegate from reflection only if we know exact signature, so we use genereric method
        private static Func<object, object> CreateFunc1ArgGeneric<TArg, TRet>( object target, MethodInfo method )
        {
            var func = Delegate.CreateDelegate( typeof( Func<TArg, TRet> ), target, method ) as Func<TArg, TRet>;
            return ( arg ) => func( ( TArg )arg );
        }

        public static Func<object, object> CreateFunc1Arg( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 1 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 1 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateFunc1ArgGeneric ), 
                            BindingFlags.Static | BindingFlags.NonPublic );
            // getting parameters types and manually setting them to generic method
            // now, at runtime types of parameters will be known and CreateFunc1ArgGeneric will successfully create delegate
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.ReturnType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Func<object, object>;
        }

        // same trick for all other methods
        private static Func<object, object, object> 
            CreateFunc2ArgsGeneric<TArg1, TArg2, TRet>( object target, 
                                                        MethodInfo method )
        {
            var func = Delegate.CreateDelegate( typeof( Func<TArg1, TArg2, TRet> ), 
                                                target, 
                                                method ) as Func<TArg1, TArg2, TRet>;
            return ( arg1, arg2 ) => func( ( TArg1 )arg1, ( TArg2 )arg2 );
        }

        public static Func<object, object, object> CreateFunc2Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 2 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 2 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateFunc2ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.ReturnType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Func<object, object, object>;
        }

        private static Func<object, object, object, object>
            CreateFunc3ArgsGeneric<TArg1, TArg2, TArg3, TRet>( object target,
                                                               MethodInfo method )
        {
            var func = Delegate.CreateDelegate( typeof( Func<TArg1, TArg2, TArg3, TRet> ),
                                                target,
                                                method ) as Func<TArg1, TArg2, TArg3, TRet>;
            return ( arg1, arg2, arg3 ) => func( ( TArg1 )arg1, ( TArg2 )arg2, ( TArg3 )arg3 );
        }

        public static Func<object, object, object, object> CreateFunc3Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 3 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 3 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateFunc3ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.GetParameters( )[2].ParameterType,
                                                                   method.ReturnType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Func<object, object, object, object>;
        }

        private static Func<object, object, object, object, object>
            CreateFunc4ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TRet>( object target,
                                                                      MethodInfo method )
        {
            var func = Delegate.CreateDelegate( typeof( Func<TArg1, TArg2, TArg3, TArg4, TRet> ),
                                                target,
                                                method ) as Func<TArg1, TArg2, TArg3, TArg4, TRet>;
            return ( arg1, arg2, arg3, arg4 ) => func( ( TArg1 )arg1, ( TArg2 )arg2, ( TArg3 )arg3, ( TArg4 )arg4 );
        }

        public static Func<object, object, object, object, object> CreateFunc4Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 4 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 4 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateFunc4ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.GetParameters( )[2].ParameterType,
                                                                   method.GetParameters( )[3].ParameterType,
                                                                   method.ReturnType );
            return correctCreator.Invoke( null, new[] { target, method } ) 
                as Func<object, object, object, object, object>;
        }

        private static Func<object, object, object, object, object, object>
            CreateFunc5ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TArg5, TRet>( object target,
                                                                             MethodInfo method )
        {
            var func = Delegate.CreateDelegate( typeof( Func<TArg1, TArg2, TArg3, TArg4, TArg5, TRet> ),
                                                target,
                                                method ) as Func<TArg1, TArg2, TArg3, TArg4, TArg5, TRet>;
            return ( arg1, arg2, arg3, arg4, arg5 ) => func( ( TArg1 )arg1, 
                                                             ( TArg2 )arg2, 
                                                             ( TArg3 )arg3, 
                                                             ( TArg4 )arg4,
                                                             ( TArg5 )arg5 );
        }

        public static Func<object, object, object, object, object, object> 
            CreateFunc5Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 5 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 5 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateFunc5ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.GetParameters( )[2].ParameterType,
                                                                   method.GetParameters( )[3].ParameterType,
                                                                   method.GetParameters( )[4].ParameterType,
                                                                   method.ReturnType );
            return correctCreator.Invoke( null, new[] { target, method } ) 
                as Func<object, object, object, object, object, object>;
        }

        // Actions


        public static Action CreateActionZeroArg( object target, MethodInfo method )
        {
            var act = Delegate.CreateDelegate( typeof( Action ), target, method ) as Action;
            return act;
        }

        private static Action<object> CreateAction1ArgGeneric<TArg>( object target, MethodInfo method )
        {
            var act = Delegate.CreateDelegate( typeof( Action<TArg> ), target, method ) as Action<TArg>;
            return ( arg ) => act( ( TArg )arg );
        }

        public static Action<object> CreateAction1Arg( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 1 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 1 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateAction1ArgGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Action<object>;
        }

        private static Action<object, object>
            CreateAction2ArgsGeneric<TArg1, TArg2>( object target,
                                                    MethodInfo method )
        {
            var act = Delegate.CreateDelegate( typeof( Action<TArg1, TArg2> ),
                                                target,
                                                method ) as Action<TArg1, TArg2>;
            return ( arg1, arg2 ) => act( ( TArg1 )arg1, ( TArg2 )arg2 );
        }

        public static Action<object, object> CreateAction2Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 2 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 2 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateAction2ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Action<object, object>;
        }

        private static Action<object, object, object>
            CreateAction3ArgsGeneric<TArg1, TArg2, TArg3>( object target,
                                                           MethodInfo method )
        {
            var act = Delegate.CreateDelegate( typeof( Action<TArg1, TArg2, TArg3> ),
                                               target,
                                               method ) as Action<TArg1, TArg2, TArg3>;
            return ( arg1, arg2, arg3 ) => act( ( TArg1 )arg1, ( TArg2 )arg2, ( TArg3 )arg3 );
        }

        public static Action<object, object, object> CreateAction3Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 3 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 3 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateAction3ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.GetParameters( )[2].ParameterType );
            return correctCreator.Invoke( null, new[] { target, method } ) as Action<object, object, object>;
        }

        private static Action<object, object, object, object>
            CreateAction4ArgsGeneric<TArg1, TArg2, TArg3, TArg4>( object target,
                                                                  MethodInfo method )
        {
            var act = Delegate.CreateDelegate( typeof( Action<TArg1, TArg2, TArg3, TArg4> ),
                                               target,
                                               method ) as Action<TArg1, TArg2, TArg3, TArg4>;
            return ( arg1, arg2, arg3, arg4 ) => act( ( TArg1 )arg1, ( TArg2 )arg2, ( TArg3 )arg3, ( TArg4 )arg4 );
        }

        public static Action<object, object, object, object> CreateAction4Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 4 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 4 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateAction4ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.GetParameters( )[2].ParameterType,
                                                                   method.GetParameters( )[3].ParameterType );
            return correctCreator.Invoke( null, new[] { target, method } )
                as Action<object, object, object, object>;
        }

        private static Action<object, object, object, object, object>
            CreateAction5ArgsGeneric<TArg1, TArg2, TArg3, TArg4, TArg5>( object target,
                                                                             MethodInfo method )
        {
            var act = Delegate.CreateDelegate( typeof( Action<TArg1, TArg2, TArg3, TArg4, TArg5> ),
                                               target,
                                               method ) as Action<TArg1, TArg2, TArg3, TArg4, TArg5>;
            return ( arg1, arg2, arg3, arg4, arg5 ) => act( ( TArg1 )arg1,
                                                            ( TArg2 )arg2,
                                                            ( TArg3 )arg3,
                                                            ( TArg4 )arg4,
                                                            ( TArg5 )arg5 );
        }

        public static Action<object, object, object, object, object>
            CreateAction5Args( object target, MethodInfo method )
        {
            if ( method.GetParameters( ).Length != 5 )
            { throw new ArgumentException( string.Format( Resources.WrongParamsCount, 5 ) ); }

            var genericCreator = typeof( ReflectionToDelegateConverter )
                .GetMethod( nameof( CreateAction5ArgsGeneric ),
                            BindingFlags.Static | BindingFlags.NonPublic );
            var correctCreator = genericCreator.MakeGenericMethod( method.GetParameters( )[0].ParameterType,
                                                                   method.GetParameters( )[1].ParameterType,
                                                                   method.GetParameters( )[2].ParameterType,
                                                                   method.GetParameters( )[3].ParameterType,
                                                                   method.GetParameters( )[4].ParameterType );
            return correctCreator.Invoke( null, new[] { target, method } )
                as Action<object, object, object, object, object>;
        }
    }
}
