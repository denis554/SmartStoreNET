using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageResizer;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media.Storage;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
	public partial class PictureService : IPictureService
    {
        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 4;
		private const string STATIC_IMAGE_PATH = "~/Content/Images";

		private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageResizerService _imageResizerService;
        private readonly IImageCache _imageCache;
		private readonly Provider<IMediaStorageProvider> _storageProvider;

		private string _staticImagePath;

        public PictureService(
            IRepository<Picture> pictureRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService, 
            ILogger logger, 
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IImageResizerService imageResizerService,
            IImageCache imageCache,
			IProviderManager providerManager)
        {
            _pictureRepository = pictureRepository;
            _productPictureRepository = productPictureRepository;
            _settingService = settingService;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _mediaSettings = mediaSettings;
            _imageResizerService = imageResizerService;
            _imageCache = imageCache;

			var systemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);

			_storageProvider = providerManager.GetProvider<IMediaStorageProvider>(systemName);
        }

		#region Utilities

		protected virtual string StaticImagePath
		{
			get { return _staticImagePath ?? (_staticImagePath = CommonHelper.MapPath(STATIC_IMAGE_PATH, false)); }
		}

		protected virtual string GetDefaultImageFileName(PictureType defaultPictureType = PictureType.Entity)
		{
			string defaultImageFileName;

			switch (defaultPictureType)
			{
				case PictureType.Entity:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.jpg");
					break;
				case PictureType.Avatar:
					defaultImageFileName = _settingService.GetSettingByKey("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
					break;
				default:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.jpg");
					break;
			}

			return defaultImageFileName;
		}

		protected internal virtual string GetProcessedImageUrl(
			object source, // byte[], string or Picture
			string seoFileName,
			string extension,
			int targetSize = 0,
			string storeLocation = null)
		{
			var resizeSettings = new ResizeSettings();
			if (targetSize > 0)
			{
				resizeSettings.MaxWidth = targetSize;
				resizeSettings.MaxHeight = targetSize;
			}

			var picture = source as Picture;

			var cachedImage = _imageCache.GetCachedImage(
				picture?.Id,
				seoFileName,
				extension,
				resizeSettings);

			if (!cachedImage.Exists)
			{
				lock (String.Intern(cachedImage.Path))
				{
					byte[] buffer = null;

					try
					{
						if (source is string)
						{
							// static default image
							buffer = File.ReadAllBytes((string)source);
						}
						else if (source is Picture)
						{
							buffer = LoadPictureBinary((Picture)source);
						}
						else if (source is byte[])
						{
							buffer = (byte[])source;
						}

						if (buffer == null || buffer.Length == 0)
						{
							return string.Empty;
						}
					}
					catch (Exception exception)
					{
						_logger.Error("Error reading media file '{0}'.".FormatInvariant(source), exception);
						return string.Empty;
					}

					try
					{
						_imageCache.ProcessAndAddImageToCache(cachedImage, buffer, targetSize);
					}
					catch (Exception exception)
					{
						_logger.Error("Error processing/writing media file '{0}'.".FormatInvariant(cachedImage.Path), exception);
						return string.Empty;
					}
				}
			}

			var url = _imageCache.GetImageUrl(cachedImage.Path, storeLocation);
			return url;
		}

		protected internal virtual async Task<string> GetProcessedImageUrlAsync(
			object source, // byte[], string or Picture
			string seoFileName,
			string extension,
			int targetSize = 0,
			string storeLocation = null)
		{
			var resizeSettings = new ResizeSettings();
			if (targetSize > 0)
			{
				resizeSettings.MaxWidth = targetSize;
				resizeSettings.MaxHeight = targetSize;
			}

			var picture = source as Picture;

			var cachedImage = _imageCache.GetCachedImage(
				picture?.Id,
				seoFileName,
				extension,
				resizeSettings);

			if (!cachedImage.Exists)
			{
				byte[] buffer = null;

				try
				{
					if (source is string)
					{
						// static default image
						buffer = File.ReadAllBytes((string)source);
					}
					else if (source is Picture)
					{
						buffer = await LoadPictureBinaryAsync((Picture)source);
					}
					else if (source is byte[])
					{
						buffer = (byte[])source;
					}

					if (buffer == null || buffer.Length == 0)
					{
						return string.Empty;
					}
				}
				catch (Exception exception)
				{
					_logger.Error("Error reading media file '{0}'.".FormatInvariant(source), exception);
					return string.Empty;
				}

				try
				{
					await _imageCache.ProcessAndAddImageToCacheAsync(cachedImage, buffer, targetSize);
				}
				catch (Exception exception)
				{
					_logger.Error("Error processing/writing media file '{0}'.".FormatInvariant(cachedImage.Path), exception);
					return string.Empty;
				}
			}

			var url = _imageCache.GetImageUrl(cachedImage.Path, storeLocation);
			return url;
		}

		#endregion

		#region Methods

		public virtual byte[] ValidatePicture(byte[] pictureBinary)
		{
			var originalSize = GetPictureSize(pictureBinary);
			var maxSize = _mediaSettings.MaximumImageSize;

			if (originalSize.IsEmpty || (originalSize.Height <= maxSize && originalSize.Width <= maxSize))
			{
				return pictureBinary;
			}

			using (var resultStream = _imageResizerService.ResizeImage(new MemoryStream(pictureBinary), maxSize, maxSize, _mediaSettings.DefaultImageQuality))
			{
				return resultStream.GetBuffer();
			}
		}

		public virtual byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<Picture> pictures, out int equalPictureId)
		{
			equalPictureId = 0;
			try
			{
				foreach (var picture in pictures)
				{
					var otherPictureBinary = LoadPictureBinary(picture);

					using (var myStream = new MemoryStream(pictureBinary))
					using (var otherStream = new MemoryStream(otherPictureBinary))
					{
						if (myStream.ContentsEqual(otherStream))
						{
							equalPictureId = picture.Id;
							return null;
						}
					}
				}

				return pictureBinary;
			}
			catch
			{
				return null;
			}
		}

		public virtual string GetPictureSeName(string name)
        {
            return SeoHelper.GetSeName(name, true, false);
        }

		public virtual Picture SetSeoFilename(int pictureId, string seoFilename)
		{
			var picture = GetPictureById(pictureId);

			// update if it has been changed
			if (picture != null && seoFilename != picture.SeoFilename)
			{
				UpdatePicture(picture, LoadPictureBinary(picture), picture.MimeType, seoFilename, true, false);
			}

			return picture;
		}

		public virtual byte[] LoadPictureBinary(Picture picture)
        {
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.Load(picture.ToMedia());
        }

		public virtual Task<byte[]> LoadPictureBinaryAsync(Picture picture)
		{
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.LoadAsync(picture.ToMedia());
		}

		public virtual Size GetPictureSize(byte[] pictureBinary)
		{
			if (pictureBinary == null || pictureBinary.Length == 0)
			{
				return new Size();
			}

			Size size;
			var stream = new MemoryStream(pictureBinary);

			try
			{
				using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
				{
					size = ImageHeader.GetDimensions(reader);
				}
			}
			catch (Exception)
			{
				// something went wrong with fast image access,
				// so get original size the classic way
				using (var b = new Bitmap(stream))
				{
					size = new Size(b.Width, b.Height);
				}
			}
			finally
			{
				stream.Dispose();
			}

			return size;
		}

		public virtual string GetPictureUrl(
            int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            return GetPictureUrl(GetPictureById(pictureId), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }

		public virtual Task<string> GetPictureUrlAsync(
			int pictureId,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			return GetPictureUrlAsync(GetPictureById(pictureId), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
		}

		public virtual string GetPictureUrl(
            Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
			var url = PrepareGetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);

			if (url.IsEmpty() && picture != null)
			{
				url = GetProcessedImageUrl(
					picture,
					picture.SeoFilename,
					MimeTypes.MapMimeTypeToExtension(picture.MimeType),
					targetSize,
					storeLocation);
			}

			return url;
        }

		public virtual Task<string> GetPictureUrlAsync(
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			var url = PrepareGetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);

			if (url.IsEmpty() && picture != null)
			{
				return GetProcessedImageUrlAsync(
					picture,
					picture.SeoFilename,
					MimeTypes.MapMimeTypeToExtension(picture.MimeType),
					targetSize,
					storeLocation);
			}

			return Task.FromResult(url);
		}

		private string PrepareGetPictureUrl(
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			if (picture == null)
			{
				if (showDefaultPicture)
				{
					return GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
				}
				else
				{
					return string.Empty;
				}
			}

			if (picture.IsNew)
			{
				_imageCache.DeleteCachedImages(picture);

				// we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
				UpdatePicture(
					picture,
					LoadPictureBinary(picture),
					picture.MimeType,
					picture.SeoFilename,
					false,
					false);
			}

			return string.Empty;
		}

		public virtual string GetDefaultPictureUrl(
			int targetSize = 0,
			PictureType defaultPictureType = PictureType.Entity,
			string storeLocation = null)
		{
			var defaultImageFileName = GetDefaultImageFileName(defaultPictureType);
			var filePath = Path.Combine(StaticImagePath, defaultImageFileName);

			var url = GetProcessedImageUrl(
				filePath,
				Path.GetFileNameWithoutExtension(filePath),
				Path.GetExtension(filePath),
				targetSize,
				storeLocation);

			return url;
		}

        public virtual Picture GetPictureById(int pictureId)
        {
            if (pictureId == 0)
                return null;

            var picture = _pictureRepository.GetById(pictureId);
            return picture;
        }

        public virtual IPagedList<Picture> GetPictures(int pageIndex, int pageSize)
        {
            var query = from p in _pictureRepository.Table
                        orderby p.Id descending
                        select p;

            var pics = new PagedList<Picture>(query, pageIndex, pageSize);
            return pics;
        }

        public virtual IList<Picture> GetPicturesByProductId(int productId, int recordsToReturn = 0)
        {
            if (productId == 0)
                return new List<Picture>();

            var query = from p in _pictureRepository.Table
						join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
                        orderby pp.DisplayOrder
                        where pp.ProductId == productId
                        select p;

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = query.ToList();
            return pics;
        }

		public virtual Multimap<int, Picture> GetPicturesByProductIds(int[] productIds, int? maxPicturesPerProduct = 1)
		{
			Guard.NotEmpty(productIds, nameof(productIds));
			if (maxPicturesPerProduct.HasValue)
			{
				Guard.IsPositive(maxPicturesPerProduct.Value, nameof(maxPicturesPerProduct));
			}

			int take = maxPicturesPerProduct ?? int.MaxValue;

			var query = from pp in _productPictureRepository.TableUntracked
						where productIds.Contains(pp.ProductId)
						group pp by pp.ProductId into g
						select new
						{
							ProductId = g.Key,
							Pictures = g.OrderBy(x => x.DisplayOrder).Take(take).Select(x => x.Picture)
						};

			var result = query.ToList();

			var map = new Multimap<int, Picture>();
			
			foreach (var ppm in result)
			{
				map.AddRange(ppm.ProductId, ppm.Pictures);
			}

			return map;
		}

		public virtual IList<Picture> GetPicturesByIds(int[] pictureIds)
		{
			Guard.NotNull(pictureIds, nameof(pictureIds));

			var query = _pictureRepository.Table
				.Where(x => pictureIds.Contains(x.Id));

			return query.ToList();
		}

		public virtual void DeletePicture(Picture picture)
		{
			Guard.NotNull(picture, nameof(picture));

			// delete thumbs
			_imageCache.DeleteCachedImages(picture);

			// delete from storage
			_storageProvider.Value.Remove(picture.ToMedia());

			// delete entity
			_pictureRepository.Delete(picture);

			// event notification
			_eventPublisher.EntityDeleted(picture);
		}

		public virtual Picture InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			bool isTransient = true,
			bool validateBinary = true)
        {
			mimeType = mimeType.EmptyNull();
			mimeType = mimeType.Truncate(20);

			seoFilename = seoFilename.Truncate(100);

            if (validateBinary)
            {
                pictureBinary = ValidatePicture(pictureBinary);
            }

            var picture = _pictureRepository.Create();
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.IsNew = isNew;
			picture.IsTransient = isTransient;
			picture.UpdatedOnUtc = DateTime.UtcNow;

            _pictureRepository.Insert(picture);

			// save to storage
			_storageProvider.Value.Save(picture.ToMedia(), pictureBinary);

			// event notification
			_eventPublisher.EntityInserted(picture);

            return picture;
        }

        public virtual void UpdatePicture(
			Picture picture,
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			bool validateBinary = true)
        {
			if (picture == null)
				return;

			mimeType = mimeType.EmptyNull().Truncate(20);
			seoFilename = seoFilename.Truncate(100);

            if (validateBinary)
            {
                pictureBinary = ValidatePicture(pictureBinary);
            }

            // delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
            {
                _imageCache.DeleteCachedImages(picture);
            }

            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.IsNew = isNew;
			picture.UpdatedOnUtc = DateTime.UtcNow;

            _pictureRepository.Update(picture);

			// save to storage
			_storageProvider.Value.Save(picture.ToMedia(), pictureBinary);

			// event notification
			_eventPublisher.EntityUpdated(picture);
        }

        #endregion
    }
}
