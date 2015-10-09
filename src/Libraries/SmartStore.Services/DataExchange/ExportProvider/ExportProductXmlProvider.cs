﻿using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	/// <summary>
	/// Exports XML formatted product data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreProductXml")]
	[FriendlyName("SmartStore XML product export")]
	[IsHidden(true)]
	public class ExportProductXmlProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreProductXml"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Product; }
		}

		public string FileExtension
		{
			get { return "XML"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
				Indent = true,
				IndentChars = "\t"
			};

			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var xmlHelper = new ExportXmlHelper(writer, CultureInfo.InvariantCulture);

				writer.WriteStartDocument();
				writer.WriteStartElement("Products");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Data.ReadNextSegment())
				{
					var segment = context.Data.CurrentSegment;

					foreach (dynamic product in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						int productId = product.Id;

						context.ProcessRecord((int)product.Id, () => xmlHelper.WriteProduct(product, "Product"));
					}
				}

				writer.WriteEndElement();	// Products
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
