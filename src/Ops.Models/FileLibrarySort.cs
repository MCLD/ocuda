namespace Ocuda.Ops.Models
{
    public enum FileLibrarySort
    {
        /// <summary>
        /// Sorted alphabetically by filename, the default sort order.
        /// </summary>
        AlphabeticalName,

        /// <summary>
        /// Sorted by the date the file was created in the system.
        /// </summary>
        CreatedDate,

        /// <summary>
        /// Sorted by the date associated with the document descending expecing one document per
        /// month, then alphabetically for duplicates.
        /// </summary>
        DocumentDateMonthDescending,

        /// <summary>
        /// Sorted by the name of the thumbnail files associated with the document.
        /// </summary>
        ThumbnailsAlphabetical,
    }
}
