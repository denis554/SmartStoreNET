﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Data
{
	
	public class ImportProgressInfo
	{

		public int TotalRecords
		{
			get;
			set;
		}

		public int TotalProcessed
		{
			get;
			set;
		}
		 
		public double ProcessedPercent
		{
			get
			{
				return (double)((TotalProcessed / TotalRecords) * 100);
			}
		}

		public int NewRecords
		{
			get;
			set;
		}

		public int ModifiedRecords
		{
			get;
			set;
		}

		public TimeSpan ElapsedTime
		{
			get;
			set;
		}

		public int TotalWarnings
		{
			get;
			set;
		}

		public int TotalErrors
		{
			get;
			set;
		}
	}

}
