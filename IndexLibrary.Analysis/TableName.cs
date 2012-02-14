namespace IndexLibrary.Analysis
{
    #region Enumerations

    /// <summary>
    /// The possible list of tables that can be written to and read from for analysis 
    /// </summary>
    /// <remarks>
    /// Used explicitly for the <see cref="IndexLibrary.Analysis.AnalysisWriter"/> class
    /// </remarks>
    [System.CLSCompliant(true)]
    public enum TableName
    {
        /// <summary>
        /// Represents the IndexInfo table
        /// </summary>
        IndexAnalysis,
        /// <summary>
        /// Represents the ReadInfo table
        /// </summary>
        ReadAnalysis,
        /// <summary>
        /// Represents the SearchInfo table
        /// </summary>
        SearchAnalysis
    }

    #endregion Enumerations
}