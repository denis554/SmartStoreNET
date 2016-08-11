﻿using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreDatabase")]
	[FriendlyName("Database")]
	[DisplayOrder(0)]
	public class DatabaseMediaStorageProvider : IMediaStorageProvider, ISupportsMediaMoving
	{
		private readonly IDbContext _dbContext;
		private readonly IRepository<MediaStorage> _mediaStorageRepository;

		public DatabaseMediaStorageProvider(
			IDbContext dbContext,
			IRepository<MediaStorage> mediaStorageRepository)
		{
			_dbContext = dbContext;
			_mediaStorageRepository = mediaStorageRepository;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreDatabase"; }
		}

		public byte[] Load(MediaItem media)
		{
			Guard.NotNull(media, nameof(media));

			if ((media.Entity.MediaStorageId ?? 0) != 0 && media.Entity.MediaStorage != null)
			{
				return media.Entity.MediaStorage.Data;
			}

			return new byte[0];
		}

		public void Save(MediaItem media, byte[] data)
		{
			Guard.NotNull(media, nameof(media));

			if (data == null || data.LongLength == 0)
			{
				// remove media storage if any
				if ((media.Entity.MediaStorageId ?? 0) != 0 && media.Entity != null && media.Entity.MediaStorage != null)
				{
					_mediaStorageRepository.Delete(media.Entity.MediaStorage);
				}
			}
			else
			{
				if (media.Entity.MediaStorage == null)
				{
					// entity has no media storage -> insert
					var newStorage = new MediaStorage { Data = data };

					_mediaStorageRepository.Insert(newStorage);

					if (newStorage.Id == 0)
					{
						// actually we should never get here
						_dbContext.SaveChanges();
					}

					media.Entity.MediaStorageId = newStorage.Id;

					_dbContext.SaveChanges();
				}
				else
				{
					// update existing media storage
					media.Entity.MediaStorage.Data = data;

					_mediaStorageRepository.Update(media.Entity.MediaStorage);
				}
			}
		}

		public void Remove(params MediaItem[] medias)
		{
			foreach (var media in medias)
			{
				if ((media.Entity.MediaStorageId ?? 0) != 0)
				{
					// this also nulls media.Entity.MediaStorageId
					_mediaStorageRepository.Delete(media.Entity.MediaStorage);
				}
			}
		}


		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaItem media)
		{
			Guard.NotNull(target, nameof(target));
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			if (media.Entity.MediaStorage != null)
			{
				// let target store data (into a file for example)
				target.Receive(context, media, media.Entity.MediaStorage.Data);

				// remove picture binary from DB
				try
				{
					_mediaStorageRepository.Delete(media.Entity.MediaStorage);
				}
				catch { }

				media.Entity.MediaStorageId = null;

				context.ShrinkDatabase = true;
			}
		}

		public void Receive(MediaMoverContext context, MediaItem media, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			// store data for later bulk commit
			if (data != null && data.LongLength > 0)
			{
				// requires autoDetectChanges set to true or remove explicit entity detaching
				media.Entity.MediaStorage = new MediaStorage { Data = data };
			}
		}

		public void OnCompleted(MediaMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
