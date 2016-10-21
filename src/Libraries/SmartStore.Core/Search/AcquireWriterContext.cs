﻿namespace SmartStore.Core.Search
{
	public class AcquireWriterContext
	{
		public enum AcquirementReason
		{
			Indexing,
			Optimize,
			Delete
		}

		public AcquireWriterContext(AcquirementReason reason)
		{
			Reason = reason;
			LanguageSeoCodes = new string[0];
			CurrencyCodes = new string[0];
		}

		/// <summary>
		/// Reason for writer acquirement
		/// </summary>
		public AcquirementReason Reason { get; private set; }

		/// <summary>
		/// Indicates whether old and new search index uses different languages
		/// </summary>
		public bool? HasDifferentLanguages { get; set; }

		/// <summary>
		/// SEO codes of languages used for indexing
		/// </summary>
		public string[] LanguageSeoCodes { get; set; }

		/// <summary>
		/// Currency codes used for indexing
		/// </summary>
		public string[] CurrencyCodes { get; set; }
	}
}
