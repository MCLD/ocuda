using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Models.Screenly.v11;

namespace Ocuda.Ops.Service.Abstract
{
    public interface IScreenlyClient
    {
        /// <summary>
        /// Add a slide to the specified Screenly digital display
        /// </summary>
        /// <param name="display">Populated <see cref="DigitalDisplay"/> object</param>
        /// <param name="filePath">Path to the slide to upload</param>
        /// <param name="assetModel">Populated <see cref="AssetModel"/> object with slideshow metadata</param>
        /// <returns>The Screenly AssetId string of the newly-added item</returns>
        Task<string> AddSlideAsync(DigitalDisplay display, string filePath, AssetModel assetModel);

        /// <summary>
        /// Get collection of details about slides currently in a Screenly digital display
        /// </summary>
        /// <param name="display">Populated <see cref="DigitalDisplay"/> object</param>
        /// <returns>An array of populated <see cref="AssetModel"/> objects representing current slides</returns>
        Task<AssetModel[]> GetCurrentSlidesAsync(DigitalDisplay display);

        /// <summary>
        /// Get details about single slide currently in a Screenly digital display
        /// </summary>
        /// <param name="display">Populated <see cref="DigitalDisplay"/> object</param>
        /// <param name="assetId">The Screenly AssetId string representing the slide</param>
        /// <returns>A populated <see cref="AssetModel"/> object representing that slides</returns>
        Task<AssetModel> GetSlideAsync(DigitalDisplay display, string assetId);

        /// <summary>
        /// Remove a single slide from a Screenly digital display
        /// </summary>
        /// <param name="display">Populated <see cref="DigitalDisplay"/> object</param>
        /// <param name="assetId">The Screenly AssetId string representing the slide</param>
        /// <returns>
        /// The text returned from the HTTP request to Screenly requesting the delete
        /// </returns>
        Task<string> RemoveSlideAsync(DigitalDisplay display, string assetId);

        /// <summary>
        /// Update the slideshow metadata for a slide in a Screenly digital display
        /// </summary>
        /// <param name="display">Populated <see cref="DigitalDisplay"/> object</param>
        /// <param name="assetId">The Screenly AssetId string representing the slide</param>
        /// <param name="assetModel">
        /// Populated <see cref="AssetModel"/> object with updated slideshow metadata
        /// </param>
        Task UpdateSlideAsync(DigitalDisplay display, string assetId, AssetModel assetModel);
    }
}