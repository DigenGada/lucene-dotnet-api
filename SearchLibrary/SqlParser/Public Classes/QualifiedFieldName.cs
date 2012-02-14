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
namespace IndexLibrary.SqlParser
{
    using System;
    using System.Linq;

    [System.CLSCompliant(true)]
    internal sealed class QualifiedFieldName
    {
        #region Fields

        private string fieldName;
        private string tableQualifier;

        #endregion Fields

        #region Constructors

        public QualifiedFieldName(string inputName)
        {
            if (string.IsNullOrEmpty(inputName))
                return;
            int index = inputName.IndexOf('.');
            if (index > -1) {
                this.tableQualifier = inputName.Substring(0, index);
                this.fieldName = inputName.Substring(index + 1);
                if (this.fieldName.Contains('.'))
                    throw new ArgumentException("The IndexLibrary does not allow for multi-qualified field names");
                return;
            }
            this.tableQualifier = "*";
            this.fieldName = inputName;
        }

        #endregion Constructors

        #region Properties

        public string FieldName
        {
            get { return this.fieldName; }
        }

        public string TableQualifier
        {
            get { return this.tableQualifier; }
        }

        #endregion Properties

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(QualifiedFieldName))
                return false;
            QualifiedFieldName temp = (QualifiedFieldName)obj;
            return this.fieldName.Equals(temp.fieldName, StringComparison.OrdinalIgnoreCase) && this.tableQualifier.Equals(temp.tableQualifier, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion Methods
    }
}