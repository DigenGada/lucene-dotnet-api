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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using IndexLibrary.Interfaces;

    public static class IndexHelper
    {
        #region Methods

        public static IIndex CreateIndex(DirectoryInfo directory, IndexType type)
        {
            if (directory == null)
                throw new ArgumentNullException("directory", "directory cannot be null");
            directory.Refresh();
            if (!directory.Exists)
                directory.Create();
            switch (type) {
                case IndexType.SingleIndex:
                    return Index.Create(directory);
                case IndexType.DoubleIndex:
                    return DoubleIndex.Create(directory);
                case IndexType.CyclicalIndex:
                    return CyclicalIndex.Create(directory);
                default:
                    throw new NotSupportedException(type.ToString() + " not supported");
            }
        }

        public static IIndex LoadIndex(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory", "directory cannot be null");
            directory.Refresh();
            if (!directory.Exists)
                throw new DirectoryNotFoundException(directory.FullName + " does not exist");
            FileInfo[] topLevelFiles = directory.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            var toggleFile = topLevelFiles.FirstOrDefault(x => x.Name.Equals("toggle.txt", StringComparison.OrdinalIgnoreCase));
            var totalIndexHeaders = topLevelFiles.Count(x => x.Extension.Equals(".cfx", StringComparison.OrdinalIgnoreCase) || x.Extension.Equals(".cfs", StringComparison.OrdinalIgnoreCase));
            var segments = topLevelFiles.Count(x => x.Extension.Equals(".gen", StringComparison.OrdinalIgnoreCase));
            if (toggleFile == null) {
                // single
                if (segments == 0 && totalIndexHeaders == 0)
                    return new Index(directory) { IndexStructure = IndexType.None };
                return new Index(directory) { IndexStructure = IndexType.SingleIndex };
            }
            else {
                // double or cyclic
                DirectoryInfo mirrorDir = new DirectoryInfo(Path.Combine(directory.FullName, StaticValues.DirectoryMirror));
                DirectoryInfo dirA = new DirectoryInfo(Path.Combine(directory.FullName, StaticValues.DirectoryA));
                DirectoryInfo dirB = new DirectoryInfo(Path.Combine(directory.FullName, StaticValues.DirectoryB));

                if (!dirA.Exists || !dirB.Exists)
                    return new DoubleIndex(directory) { IndexStructure = IndexType.None };
                if (mirrorDir.Exists)
                    return new CyclicalIndex(directory);
                return new DoubleIndex(directory);
            }
        }

        #endregion Methods
    }
}