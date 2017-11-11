﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Media
{
	public class ImageCache : IImageCache
    {
		public const string IdFormatString = "0000000";

		private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 4;

		private readonly MediaSettings _mediaSettings;
		private readonly string _thumbsRootDir;
		private readonly IStoreContext _storeContext;
		private readonly HttpContextBase _httpContext;
		private readonly IMediaFileSystem _fileSystem;
		private readonly IImageProcessor _imageProcessor;

		public ImageCache(
			MediaSettings mediaSettings, 
			IStoreContext storeContext, 
			HttpContextBase httpContext,
			IMediaFileSystem fileSystem,
			IImageProcessor imageProcessor)
        {
            _mediaSettings = mediaSettings;
			_storeContext = storeContext;
			_httpContext = httpContext;
			_fileSystem = fileSystem;
			_imageProcessor = imageProcessor;

			_thumbsRootDir = "Thumbs/";

			Logger = NullLogger.Instance;
		}

		public ILogger Logger
		{
			get;
			set;
		}

		private ProcessImageQuery CreateProcessQuery(CachedImageResult cachedImage, byte[] source, ProcessImageQuery q)
		{
			var query = new ProcessImageQuery(q)
			{
				Source = source,
				Format = cachedImage.Extension,
				FileName = cachedImage.FileName,
				ExecutePostProcessor = false, // cachedImage.Extension == "jpg" || cachedImage.Extension == "jpeg" // TODO: (mc) Pick from settings!
			};

			return query;
		}

		private void UpdateCachedImage(ProcessImageResult result, CachedImageResult cachedImage)
		{
			if (cachedImage.Extension != result.FileExtension)
			{
				cachedImage.Path = Path.ChangeExtension(cachedImage.Path, result.FileExtension);
				cachedImage.Extension = result.FileExtension;
			}
		}

		public byte[] ProcessAndAddImageToCache(CachedImageResult cachedImage, byte[] source, ProcessImageQuery query)
		{
			byte[] buffer;

			if (!query.NeedsProcessing())
			{
				AddImageToCache(cachedImage, source);
				buffer = source;
			}
			else
			{
				query = CreateProcessQuery(cachedImage, source, query);

				using (var result = _imageProcessor.ProcessImage(query))
				{
					buffer = result.Result.GetBuffer();
					AddImageToCache(cachedImage, buffer);

					UpdateCachedImage(result, cachedImage);
				}
			}

			return buffer;
		}

		public async Task<byte[]> ProcessAndAddImageToCacheAsync(CachedImageResult cachedImage, byte[] source, ProcessImageQuery query)
		{
			byte[] buffer;

			if (!query.NeedsProcessing())
			{
				await AddImageToCacheAsync(cachedImage, source);
				buffer = source;
			}
			else
			{
				query = CreateProcessQuery(cachedImage, source, query);

				using (var result = _imageProcessor.ProcessImage(query))
				{
					buffer = result.Result.GetBuffer();
					await AddImageToCacheAsync(cachedImage, buffer);

					UpdateCachedImage(result, cachedImage);
				}
			}

			return buffer;
		}

		public void AddImageToCache(CachedImageResult cachedImage, byte[] buffer)
        {
			if (PrepareAddImageToCache(cachedImage, buffer))
			{
				var path = BuildPath(cachedImage.Path);

				// save file
				_fileSystem.WriteAllBytes(path, buffer);

				// Refresh info
				cachedImage.Exists = true;
				cachedImage.File = _fileSystem.GetFile(path);
			}
        }

		public Task AddImageToCacheAsync(CachedImageResult cachedImage, byte[] buffer)
		{
			if (PrepareAddImageToCache(cachedImage, buffer))
			{
				var path = BuildPath(cachedImage.Path);

				// save file
				var t = _fileSystem.WriteAllBytesAsync(path, buffer);
				t.ContinueWith(x =>
				{
					// Refresh info
					cachedImage.Exists = true;
					cachedImage.File = _fileSystem.GetFile(path);
				});

				return t;
			}

			return Task.FromResult(false);
		}

		private bool PrepareAddImageToCache(CachedImageResult cachedImage, byte[] buffer)
		{
			Guard.NotNull(cachedImage, nameof(cachedImage));

			if (buffer == null || buffer.Length == 0)
			{
				return false;
			}

			if (cachedImage.Exists)
			{
				_fileSystem.DeleteFile(BuildPath(cachedImage.Path));
			}

			// create folder if needed
			string imageDir = System.IO.Path.GetDirectoryName(cachedImage.Path);
			if (imageDir.HasValue())
			{
				_fileSystem.TryCreateFolder(BuildPath(imageDir));
			}

			return true;
		}

        public virtual CachedImageResult GetCachedImage(int? pictureId, string seoFileName, string extension, ProcessImageQuery query = null)
        {
			Guard.NotEmpty(extension, nameof(extension));

			extension = query?.GetResultExtension() ?? extension.TrimStart('.').ToLower();
			var imagePath = GetCachedImagePath(pictureId, seoFileName, extension, query);

			var file = _fileSystem.GetFile(BuildPath(imagePath));

			var result = new CachedImageResult(file)
			{
				Path = imagePath,
				Extension = extension,
				IsRemote = _fileSystem.IsCloudStorage
			};
			
            return result;
        }

		public virtual Stream OpenCachedImage(CachedImageResult cachedImage)
		{
			Guard.NotNull(cachedImage, nameof(cachedImage));

			return _fileSystem.GetFile(BuildPath(cachedImage.Path)).OpenRead();
		}

        public virtual string GetImageUrl(string imagePath, string storeLocation = null)
        {
			if (imagePath.IsEmpty())
                return null;

			var publicUrl = _fileSystem.GetPublicUrl(BuildPath(imagePath)).EmptyNull();
			if (publicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || publicUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				// absolute url
				return publicUrl;
			}

			var root = storeLocation;

			if (root.IsEmpty())
			{
				var cdnUrl = _storeContext.CurrentStore.ContentDeliveryNetwork;
				if (cdnUrl.HasValue() && !_httpContext.IsDebuggingEnabled && !_httpContext.Request.IsLocal)
				{
					root = cdnUrl;
				}
			}

			if (root.IsEmpty())
			{
				// relative url must start with a slash
				return publicUrl.EnsureStartsWith("/");
			}

			if (HostingEnvironment.IsHosted)
			{
				// strip out app path from public url if needed but do not strip away leading slash
				var appPath = HostingEnvironment.ApplicationVirtualPath.EmptyNull();
				if (appPath.Length > 0 && appPath != "/")
				{
					publicUrl = publicUrl.Substring(appPath.Length + 1);
				}
			}

			return root.TrimEnd('/', '\\') + publicUrl.EnsureStartsWith("/");
		}

		public virtual void DeleteCachedImages(Picture picture)
        {
            var filter = string.Format("{0}*.*", picture.Id.ToString(IdFormatString));

			var files = _fileSystem.SearchFiles(_thumbsRootDir, filter);
			foreach (var file in files)
			{
				_fileSystem.DeleteFile(file);
			}
		}

        public virtual void DeleteCachedImages()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foreach (var file in _fileSystem.ListFiles(_thumbsRootDir))
                    {
						if (!file.Name.IsCaseInsensitiveEqual("placeholder") && !file.Name.IsCaseInsensitiveEqual("placeholder.txt"))
						{
							_fileSystem.DeleteFile(file.Path);
						}
                    }
                    foreach (var dir in _fileSystem.ListFolders(_thumbsRootDir))
                    {
						_fileSystem.DeleteFolder(dir.Path);
					}

                    return;
                }
                catch (Exception ex)
                {
					Logger.Error(ex);
                }
            }
        }

        public void CacheStatistics(out long fileCount, out long totalSize)
        {
			fileCount = 0;
			totalSize = 0;

			if (!_fileSystem.FolderExists(_thumbsRootDir))
			{
				return;
			}

			fileCount = _fileSystem.SearchFiles(_thumbsRootDir, "*.*").Count();
			totalSize = _fileSystem.ListFolders(_thumbsRootDir).Sum(x => x.Size);
        }

		#region Utils

		/// <summary>
		/// Returns the file name with the subfolder (when multidirs are enabled)
		/// </summary>
		/// <param name="pictureId"></param>
		/// <param name="seoFileName">File name without extension</param>
		/// <param name="extension">Dot-less file extension</param>
		/// <param name="query"></param>
		/// <returns></returns>
		private string GetCachedImagePath(int? pictureId, string seoFileName, string extension, ProcessImageQuery query = null)
        {
            string imageFileName = null;

            string firstPart = "";
            if (pictureId.GetValueOrDefault() > 0)
            {
                firstPart = pictureId.Value.ToString(IdFormatString) + (seoFileName.IsEmpty() ? "" : "-");
            }

            if (firstPart.IsEmpty() && seoFileName.IsEmpty())
            {
                // files without name? No way!
                return null;
            }

            seoFileName = seoFileName.EmptyNull();

            if (query == null || !query.NeedsProcessing())
            {
                imageFileName = String.Concat(firstPart, seoFileName);
            }
            else
            {
				imageFileName = String.Concat(firstPart, seoFileName, query.CreateHash());
			}

            if (_mediaSettings.MultipleThumbDirectories && imageFileName != null && imageFileName.Length > MULTIPLE_THUMB_DIRECTORIES_LENGTH)
            {
                // Get the first four letters of the file name
                var subDirectoryName = imageFileName.Substring(0, MULTIPLE_THUMB_DIRECTORIES_LENGTH);
                imageFileName = String.Concat(subDirectoryName, "/", imageFileName);
            }

            return String.Concat(imageFileName, ".", extension);
        }

		private string BuildPath(string imagePath)
		{
			if (imagePath.IsEmpty())
				return null;

			return String.Concat(_thumbsRootDir, imagePath);
		}

		private static string GetCleanFileExtension(string url)
        {
            var extension = System.IO.Path.GetExtension(url);
            if (extension != null)
            {
                return extension.TrimStart('.').ToLower();
            }

            return string.Empty;
        }

        #endregion

    }

}
