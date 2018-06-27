using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EasyTCP
{
	internal static class Ext
	{
		public static readonly Type CollectionType = typeof(ICollection);

		public static readonly Type[] GenericCollectionTypes =
		{
			typeof(ICollection<>),
			typeof(IReadOnlyCollection<>)
		};

		public static Type GetCollectionElementType(this Type T)
			=> T.IsGenericType ?
				T.GetGenericArguments().Single() :
				T.GetElementType();

		public static bool IsCollection(this Type T)
		{
			if (T.IsGenericType)
			{
				var GTD = T.GetGenericTypeDefinition();
				foreach (var GCT in GenericCollectionTypes)
					if (GTD == GCT)
						return true;
			}

			return T.GetInterface(CollectionType.Name) != null;
		}

		public static TValue GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> Dic, TKey Key)
			where TValue : class
			=> Dic.TryGetValue(Key, out var Value) ? Value : null;
	}
}
