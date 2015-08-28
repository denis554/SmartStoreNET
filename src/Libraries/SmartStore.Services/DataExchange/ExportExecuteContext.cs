﻿using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public interface IExportExecuteContext
	{
		IExportSegmenter Data { get; }

		ILogger Log { get; }

		bool IsCanceled { get; }

		ExpandoObject Store { get; }

		string Folder { get; }

		string FileNamePattern { get; }

		object ConfigurationData { get; }

		Dictionary<string, object> CustomProperties { get; set; }

		int SuccessfulExportedRecords { get; set; }

		string GetFilePath(string fileNameSuffix = null);
	}


	public class ExportExecuteContext : IExportExecuteContext
	{
		private CancellationToken _cancellation;

		internal ExportExecuteContext(CancellationToken cancellation, string folder)
		{
			_cancellation = cancellation;
			Folder = folder;

			CustomProperties = new Dictionary<string, object>();
		}

		public IExportSegmenter Data { get; internal set; }

		public ILogger Log { get; internal set; }

		public bool IsCanceled
		{
			get { return _cancellation.IsCancellationRequested; }
		}

		public ExpandoObject Store { get; internal set; }

		public string Folder { get; private set; }

		public string FileNamePattern { get; internal set; }

		public object ConfigurationData { get; internal set; }

		public Dictionary<string, object> CustomProperties { get; set; }

		public int SuccessfulExportedRecords { get; set; }

		public string GetFilePath(string fileNameSuffix = null)
		{
			var fileName = FileNamePattern.FormatInvariant(
				(Data.FileIndex + 1).ToString("D5"),
				SeoHelper.GetSeName(fileNameSuffix.EmptyNull(), true, false)
			);

			return Path.Combine(Folder, fileName);
		}
	}
}
