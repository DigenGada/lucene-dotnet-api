/// Copyright 2011 Timothy James
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
namespace IndexLibrary
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Marks a class, method, struct, enum, field, or property as Experimental
    /// </summary>
    /// <remarks>
    /// <para>
    /// The experimental attribute was created as a safe way to mark classes, structs, enums, constructors,
    /// methods, properties, fields, events, interfaces, and delegates as 'in-progress'. In many cases objects
    /// marked with the experimental attribute are working classes, containing valid code to perform a function,
    /// but the code has either not been tested or is not stable in its current configuration and thus should
    /// be avoided unless a specified need exists.
    /// </para>
    /// <para>
    /// Note that code body marked with this attribute should begin it's XML summary with [experimental], similar
    /// to how code with the [Obsolete] attribute begins with [depreciated].
    /// </para>
    /// </remarks>
    [Serializable]
    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
    [System.CLSCompliant(true)]
    public sealed class ExperimentalAttribute : Attribute
    {
        #region Fields

        /// <summary>
        /// The reason a code body is marked as [Experimental]
        /// </summary>
        private string reason;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExperimentalAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// Sets the reason to "No reason";
        /// </remarks>
        public ExperimentalAttribute()
        {
            this.reason = "No reason";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExperimentalAttribute"/> class.
        /// </summary>
        /// <param name="reasonBodyIsExperimental">The reason this code body is marked as experimental.</param>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="reasonBodyIsExperimental"/> parameter is null or empty.
        /// </exception>
        public ExperimentalAttribute(string reasonBodyIsExperimental)
        {
            if (string.IsNullOrEmpty(reasonBodyIsExperimental))
                throw new ArgumentNullException("reasonBodyIsExperimental", "reasonBodyIsExperimental cannot be null or empty");
            this.reason = reasonBodyIsExperimental;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the reason for applying this attribute to a code body.
        /// </summary>
        public string ReasonBodyIsExperimental
        {
            get { return this.reason; }
        }

        #endregion Properties
    }
}