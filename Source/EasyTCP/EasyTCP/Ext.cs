using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EasyTCP
{
	static class Ext
	{
		static readonly Type s_collectionType = typeof(ICollection);

		static readonly Type[] s_genericCollectionTypes =
		{
			typeof(ICollection<>),
			typeof(IReadOnlyCollection<>)
		};

		public static Type GetCollectionElementType(this Type type)
			=> type.IsGenericType ?
				type.GetGenericArguments().Single() :
				type.GetElementType();

		public static bool IsCollection(this Type type)
		{
			if (type.IsGenericType)
			{
				var def = type.GetGenericTypeDefinition();
				foreach (var gct in s_genericCollectionTypes)
					if (def == gct)
						return true;
			}

			return type.GetInterface(s_collectionType.Name) != null;
		}

		public static TValue GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
			where TValue : class
			=> dictionary.TryGetValue(key, out var value) ? value : null;
	}
}
