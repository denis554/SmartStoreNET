﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using SmartStore.Services.Seo;
using BundleTransformer.Core;
using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Bundles;
using SmartStore.Core;

namespace SmartStore.Web.Framework.UI
{

    public enum BundleType
    {
        Script,
        Stylesheet
    }

    public interface IBundleBuilder
    {
        string Build(BundleType type, IEnumerable<string> files);
    }

    public class BundleBuilder : IBundleBuilder
    {

        private static readonly object s_lock = new object();
        private readonly IStoreContext _storeContext;

        public BundleBuilder(IStoreContext storeContext)
        {
            _storeContext = storeContext;
        }

        //public string Build(BundleType type, IEnumerable<string> files)
        //{
        //    if (files == null || !files.Any())
        //        return string.Empty;

        //    string bundleVirtualPath = this.GetBundleVirtualPath(type, files);
        //    var bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
        //    if (bundleFor == null)
        //    {
        //        lock (s_lock)
        //        {
        //            bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
        //            if (bundleFor == null)
        //            {
        //                Bundle bundle = (type == BundleType.Script) ? 
        //                    new ScriptBundle(bundleVirtualPath) as Bundle: 
        //                    new StyleBundle(bundleVirtualPath) as Bundle;
        //                bundle.Orderer = new NullBundleOrderer();
        //                //bundle.EnableFileExtensionReplacements = false;

        //                if (type == BundleType.Script)
        //                {
        //                    bundle.Include(files.ToArray());
        //                }
        //                else
        //                {
        //                    files.Each(x => bundle.Include(x, new CssRewriteUrlTransform()));
        //                }

        //                BundleTable.Bundles.Add(bundle);
        //                BundleTable.Bundles.IgnoreList.Clear();
        //            }
        //        }
        //    }

        //    if (type == BundleType.Script)
        //        return Scripts.Render(bundleVirtualPath).ToString();

        //    return Styles.Render(bundleVirtualPath).ToString();
        //}

        public string Build(BundleType type, IEnumerable<string> files)
        {
            if (files == null || !files.Any())
                return string.Empty;

            string bundleVirtualPath = this.GetBundleVirtualPath(type, files);
            var bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
            if (bundleFor == null)
            {
                lock (s_lock)
                {
                    bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
                    if (bundleFor == null)
                    {
                        var nullOrderer = new NullOrderer();

                        var hasLessFiles = files.Any(x => x.EndsWith(".less", StringComparison.OrdinalIgnoreCase));

                        Bundle bundle = (type == BundleType.Script) ?
                            new CustomScriptBundle(bundleVirtualPath) as Bundle :
                            new SmartStyleBundle(bundleVirtualPath, hasLessFiles ? _storeContext.CurrentStore.Id : (int?)null) as Bundle;
                        bundle.Orderer = nullOrderer;

                        bundle.Include(files.ToArray());

                        BundleTable.Bundles.Add(bundle);
                    }
                }
            }

            if (type == BundleType.Script)
                return Scripts.Render(bundleVirtualPath).ToString();

            return Styles.Render(bundleVirtualPath).ToString();
        }

        protected virtual string GetBundleVirtualPath(BundleType type, IEnumerable<string> files)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("parts");

            string prefix = "~/bundles/js/";
            string postfix = ".js";
            if (type == BundleType.Stylesheet)
            {
                prefix = "~/bundles/css/";
                postfix = ".css";
            }

            // compute hash
            var hash = "";
            using (SHA256 sha = new SHA256Managed())
            {
                var hashInput = "";
                foreach (var file in files.OrderBy(x => x))
                {
                    hashInput += file;
                    hashInput += ",";
                }

                byte[] input = sha.ComputeHash(Encoding.Unicode.GetBytes(hashInput));
                hash = HttpServerUtility.UrlTokenEncode(input);
            }
            // ensure only valid chars
            hash = SeoExtensions.GetSeName(hash);

            var sb = new StringBuilder(prefix);
            sb.Append(hash);
            sb.Append(postfix);
            return sb.ToString();
        }

    }

    public class SmartStyleBundle : Bundle
    {
        private readonly int? _storeId;
        
        public SmartStyleBundle(string virtualPath, int? storeId)
		  : this(virtualPath, null, storeId)
		{ }

        public SmartStyleBundle(string virtualPath, string cdnPath, int? storeId)
			: base(virtualPath, cdnPath, new IBundleTransform[] { BundleTransformerContext.Current.GetCssTransformerInstance() })
		{
			Builder = new NullBuilder();
            _storeId = storeId;
		}

        public override string GetCacheKey(BundleContext context)
        {
            if (_storeId.HasValue)
            {
                return "{0}_{1}".FormatInvariant(base.GetCacheKey(context), _storeId.Value);
            }
            return base.GetCacheKey(context);
        }
    }

}
