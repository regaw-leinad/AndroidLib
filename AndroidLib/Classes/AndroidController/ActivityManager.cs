/*
 * ActivityManager.cs       Developed by Simon C. for AndroidLib
 * © Simon Cahill 2015
 * Date created: 30.03.2015 23:53
 * Date draft finished: n/a
 */

using System;

namespace RegawMOD.Android {

    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The <c>ActivityManager</c> class is a wrapper class for Android's am executable.
    /// This class provides methods and properties for interaction with the am executable.
    /// This class is inheritable.
    /// </summary>
    /// <remarks>
    /// Add remarks here. Use following format:
    /// {Date}, {Name}: {Changes/Remarks}
    /// </remarks>
    public class ActivityManager {

#region Class variables
        private readonly Device m_device;
#endregion

#region Enumerations
        
        /// <summary>
        /// This enum contains options for use with the <c>Start()</c> method.
        /// </summary>
        public enum StartOption {

        }
#endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityManager"/> class.
        /// </summary>
        /// <param name="m_device">The <see cref="RegawMOD.Android.Device"/> to associate with this object.</param>
        internal ActivityManager(Device m_device) { this.m_device = m_device; }

        public void Start(Intent m_intent) {
            // Todo: Finish method.
        }

    }

    public sealed class Intent {

        internal static Intent GetIntent(IntentArgument m_intentArg) {
            return new Intent(m_intentArg);
        }

        private Intent(IntentArgument m_intentArg) {

        }

    }

    /// <summary>
    /// This class represents an <c>Intent</c> argument.
    /// <example>
    /// For example; this class is the same as if you were to manually pass the following command:
    /// 
    /// <code>
    /// adb shell am -W -n com.package.name/com.package.name.ActivityName
    /// </code>
    /// 
    /// where the following would be declared in this class:
    /// <code>
    /// -n com.package.name/com.package.name.ActivityName
    /// </code>
    /// 
    /// Those arguments would look like the following:
    /// <code>
    /// var intentArg = IntentArgument.GetIntentArgument(IntentArgument.ArgumentPrefix.Component, )
    /// </code>
    /// 
    /// It's a wrapper for the AM executable. To make your life easy.
    /// </example>
    /// </summary>
    public sealed class IntentArgument {

        /// <summary>
        /// This enum contains all the possible prefixes for the <code>Intent</code>.
        /// </summary>
        public enum ArgumentPrefix {            
            /// <summary>
            /// Specifies the intent action, such as "android.intent.action.VIEW"
            /// You can declare this only once.
            /// </summary>
            [StringValue("-a")] Action,                                                     // -a          
            /// <summary>
            /// Specify the intent data URI, such as "content://contacts/people/1". 
            /// You can declare this only once.
            /// </summary>
            [StringValue("-d")] DataUri,                                                    // -d
            /// <summary>
            /// Specify the intent MIME type, such as "image/png". 
            /// You can declare this only once.
            /// </summary>
            [StringValue("-t")] MimeType,                                                   // -t
            /// <summary>
            /// Specify an intent category, 
            /// such as "android.intent.category.APP_CONTACTS".
            /// </summary>
            [StringValue("-c")] Category,                                                   // -c            
            /// <summary>
            /// Specifiy the component name with package name 
            /// prefix to create an explicit intent, 
            /// such as "com.example.app/.ExampleActiviy".
            /// </summary>
            [StringValue("-n")] Component,                                                  // -n
            /// <summary>
            /// Add flags to the intent.
            /// </summary>
            [StringValue("-f")] Flags,                                                      // -f
            /// <summary>
            /// Add a null extra. 
            /// This option is not supported for URI intents.
            /// </summary>
            [StringValue("-esn")] ExtraKey,                                                 // --esn            
            /// <summary>
            /// Add string data as a key-value pair.
            /// </summary>
            [StringValue("--es")] ExtraKey_ExtraStringValue,                                // -e|--es            
            /// <summary>
            /// Add boolean data as a key-value pair.
            /// </summary>
            [StringValue("--ez")] ExtraKey_ExtraBooleanValue,                               // --ez            
            /// <summary>
            /// Add integer data as a key-value pair.
            /// </summary>
            [StringValue("--ei")] ExtraKey_ExtraIntValue,                                   // --ei            
            /// <summary>
            /// Add long data as a key-value pair.
            /// </summary>
            [StringValue("--ei")] ExtraKey_ExtraLongValue,                                  // --el            
            /// <summary>
            /// Add float data as a key-value pair.
            /// </summary>
            [StringValue("--ef")] ExtraKey_ExtraFloatValue,                                 // --ef            
            /// <summary>
            /// Add URI data as a key-value pair.
            /// </summary>
            [StringValue("--eu")] ExtraKey_ExtraUriValue,                                   // --eu            
            /// <summary>
            /// Add a component name, 
            /// which is converted and passed as an Android ComponentName object
            /// </summary>
            [StringValue("--ecn")] ExtraKey_ExtraComponentNameValue,                        // --ecn            
            /// <summary>
            /// Add an array of integers.
            /// </summary>
            [StringValue("--eia")] ExtraKey_ExtraIntArrayValue,                             // --eia            
            /// <summary>
            /// Add an array of longs.
            /// </summary>
            [StringValue("--ela")] ExtraKey_ExtraLongArrayValue,                            // --ela            
            /// <summary>
            /// Add an array of floats.
            /// </summary>
            [StringValue("--efa")] ExtraKey_ExtraFloatArrayValue,                           // --efa            
            /// <summary>
            /// Include the flag <c>FLAG_GRANT_READ_URI_PERMISSION</c>
            /// </summary>
            [StringValue("--grant-read-uri-permission")] GrantReadUriPermission,            // --grant-read-uri-permission
            /// <summary>
            /// Include the flag <c>FLAG_GRANT_WRITE_URI_PERMISSION</c>
            /// </summary>
            [StringValue("--grant-write-uri-permission")] GrantWriteUriPermission,          // --grant-write-uri-permission
            /// <summary>
            /// Include the flag <c>FLAG_DEBUG_LOG_RESOLUTION</c>
            /// </summary>
            [StringValue("--debug.log-resolution")] DebugLogResolution,                     // --debug-log-resolution            
            /// <summary>
            /// Includes the flag <c>FLAG_EXCLUDE_STOPPED_PACKAGES</c>
            /// </summary>
            [StringValue("--exclude-stopped-packages")] ExcludeStoppedPackages,             // --exclude-stopped-packages
            /// <summary>
            /// Includes the flag <c>FLAG_INCLUDE_STOPPED_PACKAGES</c>
            /// </summary>
            [StringValue("--include-stopped-packages")] IncludeStoppedPackages,             // --include-stopped-packages
            /// <summary>
            /// Includes the flag <c>FLAG_ACTIVITY_BROUGHT_TO_FRONT</c>
            /// </summary>
            [StringValue("--activity-brought-to-front")] ActivityBroughtToFront,            // --activity-brought-to-front
            /// <summary>
            /// Includes the flag <c>FLAG_ACTIVITY_CLEAR_TOP</c>
            /// </summary>
            [StringValue("--activiy-clear-top")] ActivityClearTop,                          // --activity-clear-top
            /// <summary>
            /// Includes the flag <c>FLAG_ACTIVITY_CLEAR_WHEN_TASK_RESET</c>
            /// </summary>
            [StringValue("--activity-clear-when-task-reset")] ActivityClearWhenTaskReset,   // --activity-clear-when-task-reset
            /// <summary>
            /// Includes the flag <c>FLAG_ACTIVITY_EXCLUDE_FROM_RECENTS</c>
            /// </summary>
            [StringValue("--activity-exclude-from-recents")] ActivityExcludeFromRecents,    // --activity-exclude-from-recents
            /// <summary>
            /// Includes the flag <c>FLAG_ACTIVITY_LAUNCHED_FROM_RECENTS</c>
            /// </summary>
            [StringValue("--activity-launched-from-recents")] ActivityLaunchedFromRecents,  // --activity-launched-from-recents
            /// <summary>
            /// Includes the flag <c>FLAG_ACTIVITY_MULTIPLE_TASK</c>
            /// </summary>
            [StringValue("--activity-multiple-task")] ActivityMultipleTask,                 // --activity-multiple-task
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_NO_ANIMATION</c>
            /// </summary>
            [StringValue("--activity-no-animation")] ActivityNoAnimation,                   // --activity-no-animation
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_NO_HISTORY</c>
            /// </summary>
            [StringValue("--activity-no-history")] ActivityNoHistory,                       // --activity-no-history
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_NO_USER_ACTION</c>
            /// </summary>
            [StringValue("--activity-no-user-action")] ActivityNoUserAction,                // --activity-no-user-action
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_PREVIOUS_IS_TOP</c>
            /// </summary>
            [StringValue("--activity-previous-is-top")] ActivityPreviousIsTop,              // --activity-previous-is-top
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_REORDER_TO_FRONT</c>
            /// </summary>
            [StringValue("--activity-reorder-to-front")] ActivityReorderToFront,            // --activity-reorder-to-front
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_RESET_TASK_IF_NEEDED</c>
            /// </summary>
            [StringValue("--activity-reset-task-if-needed")] ActivityResetTaskIfNeeded,     // --activity-reset-task-if-needed
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_SINGLE_TOP</c>
            /// </summary>
            [StringValue("--activity-single-top")] ActivitySingleTop,                       // --activity-single-top
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_CLEAR_TASK</c>
            /// </summary>
            [StringValue("--activity-clear-task")] ActivityClearTask,                       // --activity-clear-task
            /// <summary>
            /// Includes flag <c>FLAG_ACTIVITY_TASK_ON_HOME</c>
            /// </summary>
            [StringValue("--activity-task-on-home")] ActivityTaskOnHome,                    // --activity-task-on-home
            /// <summary>
            /// Includes flag <c>FLAG_RECEIVER_REGISTERED_ONLY</c>
            /// </summary>
            [StringValue("--receiver-registered-only")] ReceiverRegisteredOnly,             // --receiver-registered-only
            /// <summary>
            /// Includes flag <c>FLAG_RECEIVER_REPLACE_PENDING</c>
            /// </summary>
            [StringValue("--receiver-replace-pending")] ReceiverReplacePending,             // --receiver-replace-pending
            /// <summary>
            /// Requires the use of <c>DataUri</c> (-d) and <c>MimeType</c> (-t) options to set the intent data and type.
            /// </summary>
            [StringValue("--selector")] Selector                                            // --selector
        }

        /// <summary>
        /// Gets or sets the argument prefix.
        /// </summary>
        /// <value>
        /// The argument prefix.
        /// </value>
        private ArgumentPrefix Prefix { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        private object[] Args { get; set; }

        /// <summary>
        /// Gets the argument.
        /// </summary>
        /// <value>
        /// The argument.
        /// </value>
        public string Argument {
            get {
                var formatter = "";
                for (int i = 0; i < Args.Length; i++)
                    formatter = string.Concat(formatter, " ", "{", i, "}");
                return string.Format(formatter, Prefix, Args);
            }
        }

        /// <summary>
        /// Gets the intent argument.
        /// </summary>
        /// <param name="m_prefix">The argument prefix.</param>
        /// <param name="m_args">The argument arg(s).</param>
        /// <returns>A new instance of this class.</returns>
        internal static IntentArgument GetIntentArgument(ArgumentPrefix m_prefix, params object[] m_args) {
            return new IntentArgument(m_prefix, m_args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntentArgument"/> class.
        /// </summary>
        /// <param name="m_prefix">The m_prefix.</param>
        /// <param name="m_args">The m_args.</param>
        private IntentArgument(ArgumentPrefix m_prefix, params object[] m_args) {
            this.Prefix = m_prefix;
            this.Args = m_args;
        }

    }

}
