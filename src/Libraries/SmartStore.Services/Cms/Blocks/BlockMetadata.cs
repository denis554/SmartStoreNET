﻿using System;

namespace SmartStore.Services.Cms.Blocks
{
	/// <summary>
	/// Applies metadata to concrete block types which implement <see cref="IBlock"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class BlockAttribute : Attribute
	{
		public BlockAttribute(string systemName)
		{
			Guard.NotNull(systemName, nameof(systemName));

			SystemName = systemName;
		}

		/// <summary>
		/// The block system name, e.g. 'html', 'picture' etc.
		/// </summary>
		public string SystemName { get; set; }

		/// <summary>
		/// The english friendly name of the block
		/// </summary>
		public string FriendlyName { get; set; }

		/// <summary>
		/// The icon class name of the block, e.g. 'fa fa-sitemap'
		/// </summary>
		public string Icon { get; set; }

		/// <summary>
		/// The order of display
		/// </summary>
		public int DisplayOrder { get; set; }

		public bool IsInternal { get; set; }
	}

	/// <summary>
	/// Represents block registration metadata
	/// </summary>
	public interface IBlockMetadata
	{
		string AreaName { get; }
		string SystemName { get; }
		string FriendlyName { get; }
		string Icon { get; }
		int DisplayOrder { get; }
		bool IsInternal { get; }
		bool IsInbuilt { get; }
		Type BlockClrType { get; }
		Type BlockHandlerClrType { get; }
	}

	public class BlockMetadata : IBlockMetadata
	{
		public string AreaName { get; set; }
		public string SystemName { get; set; }
		public string FriendlyName { get; set; }
		public string Icon { get; set; }
		public int DisplayOrder { get; set; }
		public bool IsInternal { get; set; }
		public bool IsInbuilt { get; set; }
		public Type BlockClrType { get; set; }
		public Type BlockHandlerClrType { get; set; }
	}
}
