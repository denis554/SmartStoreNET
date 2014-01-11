﻿
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Media
{
    public class MediaSettings : ISettings
    {
        public int AvatarPictureSize { get; set; }
        public int ProductThumbPictureSize { get; set; }
        public int ProductDetailsPictureSize { get; set; }
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }
        public int AssociatedProductPictureSize { get; set; }
        public int CategoryThumbPictureSize { get; set; }
        public int ManufacturerThumbPictureSize { get; set; }
        public int CartThumbPictureSize { get; set; }
        public int MiniCartThumbPictureSize { get; set; }
        public int AutoCompleteSearchThumbPictureSize { get; set; }

        public bool DefaultPictureZoomEnabled { get; set; }
        public string PictureZoomType { get; set; }

        public int MaximumImageSize { get; set; }

        /// <summary>
        /// Geta or sets a default quality used for image generation
        /// </summary>
        public int DefaultImageQuality { get; set; }

        /// <summary>
        /// Geta or sets a vaue indicating whether single (/media/thumbs/) or multiple (/media/thumbs/0001/ and /media/thumbs/0002/) directories will used for picture thumbs
        /// </summary>
        public bool MultipleThumbDirectories { get; set; }
    }
}