﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ev3Dev.CSharp.EvA {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Ev3Dev.CSharp.ControlFlow.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} cant be action and event handler at one time.
        /// </summary>
        internal static string ActionCantBeHandler {
            get {
                return ResourceManager.GetString("ActionCantBeHandler", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Action {0} must be void.
        /// </summary>
        internal static string InvalidAction {
            get {
                return ResourceManager.GetString("InvalidAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Async action {0} must return object of class Task.
        /// </summary>
        internal static string InvalidAsyncAction {
            get {
                return ResourceManager.GetString("InvalidAsyncAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Trigger {0} of event handler {1} must be bool.
        /// </summary>
        internal static string InvalidEventTrigger {
            get {
                return ResourceManager.GetString("InvalidEventTrigger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Event handler {0} must have at least one trigger.
        /// </summary>
        internal static string InvalidEventTriggerCount {
            get {
                return ResourceManager.GetString("InvalidEventTriggerCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shutdown event property must have bool type.
        /// </summary>
        internal static string InvalidShutdownEvent {
            get {
                return ResourceManager.GetString("InvalidShutdownEvent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Action parameter {0} of action {1} does not have source.
        /// </summary>
        internal static string NoParameterSource {
            get {
                return ResourceManager.GetString("NoParameterSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Source {0} not found for parameter {1} of action {2}.
        /// </summary>
        internal static string SourceNotFound {
            get {
                return ResourceManager.GetString("SourceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type of source {0} does not match the type of parameter {1} of action {2}.
        /// </summary>
        internal static string SourceTypeMismatch {
            get {
                return ResourceManager.GetString("SourceTypeMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method must have {0} parameters.
        /// </summary>
        internal static string WrongParamsCount {
            get {
                return ResourceManager.GetString("WrongParamsCount", resourceCulture);
            }
        }
    }
}
