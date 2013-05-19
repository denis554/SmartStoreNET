namespace SmartStore.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture item type
    /// </summary>
    public enum PictureType : int
    {
        /// <summary>
        /// Entities (products, categories, manufacturers)
        /// </summary>
        Entity = 1,
        /// <summary>
        /// Avatar
        /// </summary>
        Avatar = 10,
    }

    // codehint: sm-add
    public enum ThumbnailScaleMode
    {
        Auto,
        UseWidth,
        UseHeight
    }
}
