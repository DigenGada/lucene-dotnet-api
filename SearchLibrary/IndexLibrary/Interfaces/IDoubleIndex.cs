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
namespace IndexLibrary.Interfaces
{
    using System.IO;

    [System.CLSCompliant(true)]
    public interface IDoubleIndex : IIndex
    {
        #region Properties

        DirectoryInfo DirectoryA
        {
            get;
        }

        DirectoryInfo DirectoryB
        {
            get;
        }

        FileInfo ToggleFile
        {
            get;
        }

        #endregion Properties

        #region Methods

        void FlipToggleSwitch();

        DirectoryInfo GetReadDirectory();

        string GetToggleSwitch();

        DirectoryInfo GetWriteDirectory();

        bool HasWriteIndexFiles();

        #endregion Methods
    }
}