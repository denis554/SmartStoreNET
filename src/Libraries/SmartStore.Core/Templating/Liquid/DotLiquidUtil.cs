﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartStore.Templating.Liquid
{
	internal static class DotLiquidUtil
	{
		private static readonly IDictionary<Type, Func<object, object>> _typeWrapperCache 
			= new Dictionary<Type, Func<object, object>>();

		internal static object CreateSafeObject(object value)
		{
			if (value == null)
			{
				return null;
			}

			var valueType = value.GetType();
			
			if (!_typeWrapperCache.TryGetValue(valueType, out var fn))
			{
				if (value is IDictionary<string, object> dict)
				{
					fn = x => new DictionaryDrop((IDictionary<string, object>)x);
				}
				else if (valueType.IsSequenceType() && !(value is string))
				{
					var genericArgs = valueType.GetGenericArguments();
					var isEnumerable = genericArgs.Length == 1 && valueType.IsSubClass(typeof(IEnumerable<>));
					if (isEnumerable)
					{
						var seqType = genericArgs[0];
						if (!IsSafeType(seqType))
						{
							fn = x => new EnumerableWrapper((IEnumerable)x);
						}
					}
				}
				else if (valueType.IsPlainObjectType())
				{
					fn = x => new ObjectDrop(x);
				}

				_typeWrapperCache[valueType] = fn;
			}

			return fn?.Invoke(value) ?? value;
		}

		public static bool IsSafeType(Type type)
		{
			return type.IsPredefinedType();
		}
	}
}
