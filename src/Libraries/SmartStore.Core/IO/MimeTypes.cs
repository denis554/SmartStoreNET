﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using Microsoft.Win32;

namespace SmartStore.Core.IO
{
    
    public static class MimeTypes
    {

        public static string MapNameToMimeType(string fileNameOrExtension)
        {
            return MimeMapping.GetMimeMapping(fileNameOrExtension);
        }

        /// <summary>
        /// Returns the (dotless) extensions for a mime type
        /// </summary>
        /// <param name="mimeType">The mime type</param>
        /// <returns>The corresponding file extension (without dot)</returns>
        public static string MapMimeTypeToExtension(string mimeType)
        {
            if (mimeType.IsEmpty())
                return null;

            string result = null;

            try
            {
				using (var key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false))
				{
					object value = key != null ? key.GetValue("Extension", null) : null;
					result = value != null ? value.ToString().Trim('.') : null;
				}
            }
            catch (SecurityException)
            {
                string[] parts = mimeType.Split('/');
                result = parts[parts.Length - 1];
                switch (result)
                {
                    case "pjpeg":
                        result = "jpg";
                        break;
                    case "x-png":
                        result = "png";
                        break;
                    case "x-icon":
                        result = "ico";
                        break;
                }
            }

            return result;
        }

    }

}
