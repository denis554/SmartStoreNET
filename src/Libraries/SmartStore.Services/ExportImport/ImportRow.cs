﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Utilities.Reflection;

namespace SmartStore.Services.ExportImport
{
	public class ImportRow<T> where T : BaseEntity
	{
		private bool _initialized = false;
		private T _entity;
		private string _entityDisplayName;
		private int _position;
		private bool _isNew;
		private ImportRowInfo _rowInfo;

		private readonly IDataRow _row;

		public ImportRow(IDataRow row, int position)
		{
			_row = row;
			_position = position;
		}

		public void Initialize(T entity, string entityDisplayName)
		{
			_entity = entity;
			_entityDisplayName = entityDisplayName;
			_isNew = _entity.Id == 0;

			_initialized = true;
		}

		private void CheckInitialized()
		{
			if (_initialized)
			{
				throw Error.InvalidOperation("A row must be initialized before interacting with the entity or the data store");
			}
		}

		public bool IsTransient
		{
			get { return _entity.Id == 0; }
		}

		public bool IsNew
		{
			get { return _isNew; }
		}

		public IDataRow DataRow
		{
			get { return _row; }
		}

		public bool HasDataColumn(string name)
		{
			return _row.Table.HasColumn(name);
		}

		public T Entity
		{
			get { return _entity; }
		}

		public string EntityDisplayName
		{
			get { return _entityDisplayName; }
		}

		public bool NameChanged
		{
			get;
			set;
		}

		public int Position
		{
			get { return _position; }
		}

		public TProp GetDataValue<TProp>(string columnName)
		{
			object value;
			if (_row.TryGetValue(columnName, out value))
			{
				return value.Convert<TProp>(CultureInfo.CurrentCulture);
			}

			return default(TProp);
		}

		public bool SetProperty<TProp>(
			ImportResult result,
			T target,
			Expression<Func<T, TProp>> prop,
			TProp defaultValue = default(TProp),
			Func<object, TProp> converter = null)
		{
			// TBD: (MC) do not check for perf reason?
			//CheckInitialized();

			var pi = prop.ExtractPropertyInfo();
			var propName = pi.Name;

			try
			{
				var fastProp = FastProperty.GetProperty(target.GetUnproxiedType(), propName, PropertyCachingStrategy.EagerCached);

				object value;
				if (_row.TryGetValue(propName, out value))
				{
					// source contains field value. Set it.
					TProp converted;
					if (converter != null)
					{
						converted = converter(value);
					}
					else
					{
						if (value.ToString().ToUpper().Equals("NULL"))
						{
							// prop is set "explicitly" to null.
							converted = default(TProp);
						}
						else
						{
							converted = value.Convert<TProp>();
						}
					}

					fastProp.SetValue(target, converted);
					return true;
				}
				else
				{
					// source does not contain field data or is empty...
					if (IsTransient && defaultValue != null)
					{
						// ...but the entity is new. In this case
						// set the default value if given.
						fastProp.SetValue(target, defaultValue);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				result.AddWarning("Conversion failed: " + ex.Message, this.GetRowInfo(), propName);
			}

			return false;
		}

		public ImportRowInfo GetRowInfo()
		{
			if (_rowInfo == null)
			{
				_rowInfo = new ImportRowInfo(this.Position, this.EntityDisplayName);
			}

			return _rowInfo;
		}

		public override string ToString()
		{
			var str = "Pos: {0} - Name: {1}, IsNew: {2}, IsTransient: {3}".FormatCurrent(
				Position,
				EntityDisplayName.EmptyNull(),
				_initialized ? IsNew.ToString() : "-",
				_initialized ? IsTransient.ToString() : "-");
			return str;
		}
	}
}
