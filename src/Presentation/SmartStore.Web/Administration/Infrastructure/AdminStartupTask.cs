﻿using SmartStore.ComponentModel;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Infrastructure
{
    public class AdminStartupTask : IStartupTask
    {
        public void Execute()
        {
			TypeConverterFactory.RegisterConverter<CsvConfiguration>(new CsvConfigurationConverter());
			TypeConverterFactory.RegisterConverter<ColumnMapConverter>(new ColumnMapConverter());

			WebAssetDefaultSettings.ScriptFilesPath = "~/Administration/Content/telerik/js";
			WebAssetDefaultSettings.StyleSheetFilesPath = "~/Administration/Content/telerik/css";
		}

        public int Order
        {
            get { return 100; }
        }
    }
}