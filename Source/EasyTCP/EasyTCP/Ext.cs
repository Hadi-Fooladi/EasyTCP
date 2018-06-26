using System;
using System.Linq;
using System.Collections;

namespace EasyTCP
{
	internal static class Ext
	{
		public static readonly Type CollectionType = typeof(ICollection);

		public static Type GetCollectionInterface(this Type T) => T.GetInterface(CollectionType.FullName);

		public static Type GetCollectionElementType(this Type T)
			=> T.IsGenericType ?
				T.GetGenericArguments().Single() :
				T.GetElementType();
	}
}
