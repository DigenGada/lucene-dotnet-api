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
namespace IndexLibrary.Analysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Thread safe analysis singleton that sends events from the core classes to each subscribed <see cref="IndexLibrary.Analysis.IAnalysisWriter"/> instance.
    /// </summary>
    [System.CLSCompliant(true)]
    public static class LibraryAnalysis
    {
        #region Fields

        /// <summary>
        /// List of <see cref="IndexLibrary.Analysis.IAnalysisWriter"/>'s that are currently subscribing to library events.
        /// </summary>
        private static List<IAnalysisWriter> analysisWriters = new List<IAnalysisWriter>();

        /// <summary>
        /// The synchronization object for this singleton.
        /// </summary>
        private static volatile object syncRoot = new object();

        /// <summary>
        /// The total number of items within <c>analysisWriters</c>.
        /// </summary>
        private static int totalWriters = 0;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Fires the specified info to each subscribed <see cref="IAnalysisWriter"/>.
        /// </summary>
        /// <param name="info">The info to pass to each subscriber.</param>
        public static void Fire(IndexInfo info)
        {
            List<IAnalysisWriter> writers = null;
            lock (syncRoot)
                writers = new List<IAnalysisWriter>(analysisWriters);
            if (writers == null)
                return;

            int totalWriters = writers.Count;
            if (totalWriters == 0)
                return;

            if (totalWriters % 2 == 1) {
                writers[totalWriters - 1].AddIndexInfo(info);
                totalWriters--;
            }

            for (int i = 0; i < totalWriters; i += 2) {
                writers[i].AddIndexInfo(info);
                writers[i + 1].AddIndexInfo(info);
            }
        }

        /// <summary>
        /// Fires the specified info to each subscribed <see cref="IAnalysisWriter"/>.
        /// </summary>
        /// <param name="info">The info to pass to each subscriber.</param>
        public static void Fire(SearchInfo info)
        {
            List<IAnalysisWriter> writers = null;
            lock (syncRoot)
                writers = new List<IAnalysisWriter>(analysisWriters);
            if (writers == null)
                return;

            int totalWriters = writers.Count;
            if (totalWriters == 0)
                return;

            if (totalWriters % 2 == 1) {
                writers[totalWriters - 1].AddSearchInfo(info);
                totalWriters--;
            }

            for (int i = 0; i < totalWriters; i += 2) {
                writers[i].AddSearchInfo(info);
                writers[i + 1].AddSearchInfo(info);
            }
        }

        /// <summary>
        /// Fires the specified info to each subscribed <see cref="IAnalysisWriter"/>.
        /// </summary>
        /// <param name="info">The info to pass to each subscriber.</param>
        public static void Fire(ReadInfo info)
        {
            List<IAnalysisWriter> writers = null;
            lock (syncRoot)
                writers = new List<IAnalysisWriter>(analysisWriters);
            if (writers == null)
                return;

            int totalWriters = writers.Count;
            if (totalWriters == 0)
                return;

            if (totalWriters % 2 == 1) {
                writers[totalWriters - 1].AddReadInfo(info);
                totalWriters--;
            }

            for (int i = 0; i < totalWriters; i += 2) {
                writers[i].AddReadInfo(info);
                writers[i + 1].AddReadInfo(info);
            }
        }

        /// <summary>
        /// Subscribes the specified writer to library events.
        /// </summary>
        /// <param name="writer">The writer to add to the subscription list.</param>
        public static void Subscribe(IAnalysisWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            lock (syncRoot) {
                int containingIndex = -1;
                for (int i = 0; i < totalWriters; i++) {
                    if (analysisWriters[i].AnalyticsId.Equals(writer.AnalyticsId, StringComparison.Ordinal)) {
                        containingIndex = i;
                        break;
                    }
                }

                if (containingIndex == -1) {
                    analysisWriters.Add(writer);
                    totalWriters++;
                }
            }
        }

        /// <summary>
        /// Enables or disables read events for all subscribers.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        public static void TrackReads(bool enable)
        {
            List<IAnalysisWriter> writers = null;
            lock (syncRoot)
                writers = new List<IAnalysisWriter>(analysisWriters);
            if (writers == null)
                return;

            int totalWriters = writers.Count;
            if (totalWriters == 0)
                return;

            if (totalWriters % 2 == 1) {
                writers[totalWriters - 1].TrackReads = enable;
                totalWriters--;
            }

            for (int i = 0; i < totalWriters; i += 2) {
                writers[i].TrackReads = enable;
                writers[i + 1].TrackReads = enable;
            }
        }

        /// <summary>
        /// Enables or disables search events for all subscribers.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        public static void TrackSearches(bool enable)
        {
            List<IAnalysisWriter> writers = null;
            lock (syncRoot)
                writers = new List<IAnalysisWriter>(analysisWriters);
            if (writers == null)
                return;

            int totalWriters = writers.Count;
            if (totalWriters == 0)
                return;

            if (totalWriters % 2 == 1) {
                writers[totalWriters - 1].TrackSearches = enable;
                totalWriters--;
            }

            for (int i = 0; i < totalWriters; i += 2) {
                writers[i].TrackSearches = enable;
                writers[i + 1].TrackSearches = enable;
            }
        }

        /// <summary>
        /// Enables or disables write events for all subscribers.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        public static void TrackWrites(bool enable)
        {
            List<IAnalysisWriter> writers = null;
            lock (syncRoot)
                writers = new List<IAnalysisWriter>(analysisWriters);
            if (writers == null)
                return;

            int totalWriters = writers.Count;
            if (totalWriters == 0)
                return;

            if (totalWriters % 2 == 1) {
                writers[totalWriters - 1].TrackWrites = enable;
                totalWriters--;
            }

            for (int i = 0; i < totalWriters; i += 2) {
                writers[i].TrackWrites = enable;
                writers[i + 1].TrackWrites = enable;
            }
        }

        /// <summary>
        /// Unsubscribes the specified writer from library events.
        /// </summary>
        /// <param name="writer">The writer to unsubscribe.</param>
        public static void Unsubscribe(IAnalysisWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer", "writer cannot be null");
            lock (syncRoot) {
                int containingIndex = -1;
                for (int i = 0; i < totalWriters; i++) {
                    if (analysisWriters[i].AnalyticsId.Equals(writer.AnalyticsId, StringComparison.Ordinal)) {
                        containingIndex = i;
                        break;
                    }
                }

                if (containingIndex != -1) {
                    analysisWriters.Remove(writer);
                    totalWriters--;
                }
            }
        }

        #endregion Methods
    }
}