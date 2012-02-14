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

    using IndexLibrary;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [System.CLSCompliant(true)]
    [Experimental("Unused attribute")]
    public sealed class IndexOptionsAttribute : Attribute
    {
        #region Fields

        private bool isIndexable;

        // null storage rule means use class default
        private FieldStorage storageRule;

        #endregion Fields

        #region Constructors

        public IndexOptionsAttribute(bool isIndexable)
            : this(isIndexable, null)
        {
        }

        public IndexOptionsAttribute(bool isIndexable, FieldStorage storageRule)
        {
            this.isIndexable = isIndexable;
            this.storageRule = storageRule;
        }

        #endregion Constructors

        #region Properties

        public bool IsIndexable
        {
            get { return this.isIndexable; }
        }

        public FieldStorage StorageRule
        {
            get { return this.storageRule; }
        }

        #endregion Properties
    }
}