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

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [System.CLSCompliant(true)]
    public sealed class IndexableAttribute : Attribute
    {
        #region Fields

        private FieldStorage defaultStorageRule;
        private bool indexPropsWithNoAttribute;
        private bool isIndexable;

        #endregion Fields

        #region Constructors

        public IndexableAttribute(bool isIndexable)
            : this(isIndexable, false)
        {
        }

        public IndexableAttribute(bool isIndexable, bool indexPropertiesWithNoAttribute)
            : this(isIndexable, indexPropertiesWithNoAttribute, new FieldStorage(true, FieldSearchableRule.Analyzed, FieldVectorRule.No))
        {
        }

        public IndexableAttribute(bool isIndexable, bool indexPropertiesWithNoAttribute, FieldStorage defaultStorageRule)
        {
            if (defaultStorageRule == null)
                throw new ArgumentNullException("defaultStorageRule", "defaultStorageRule cannot be null");
            this.isIndexable = isIndexable;
            this.indexPropsWithNoAttribute = indexPropertiesWithNoAttribute;
            this.defaultStorageRule = defaultStorageRule;
        }

        #endregion Constructors

        #region Properties

        public FieldStorage DefaultStorageRule
        {
            get { return this.defaultStorageRule; }
        }

        public bool IndexPropertiesWithNoAttribute
        {
            get { return this.indexPropsWithNoAttribute; }
        }

        public bool IsIndexable
        {
            get { return this.isIndexable; }
        }

        #endregion Properties
    }
}