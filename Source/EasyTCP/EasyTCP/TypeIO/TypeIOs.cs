using System;
using System.Collections.Generic;

namespace EasyTCP
{
	internal static class TypeIOs
	{
		private static readonly IReadOnlyDictionary<Type, ITypeIO> Primary = new Dictionary<Type, ITypeIO>
		{
			{ typeof(int), new Int32TypeIO() },
			{ typeof(float), new SingleTypeIO() },
			{ typeof(string), new StringTypeIO() }
		};

		private static readonly Dictionary<Type, ListIO> ListMap = new Dictionary<Type, ListIO>();
		private static readonly Dictionary<Type, ListIO> ListElementMap = new Dictionary<Type, ListIO>();
		private static readonly Dictionary<Type, CompositeTypeIO> CompositeMap = new Dictionary<Type, CompositeTypeIO>();

		public static bool Exist(Type T) => Primary.ContainsKey(T) || CompositeMap.ContainsKey(T) || ListMap.ContainsKey(T);
		public static ITypeIO Get(Type T) => Primary.GetValueOrNull(T) ?? CompositeMap.GetValueOrNull(T) as ITypeIO ?? ListMap.GetValueOrNull(T);

		public static ITypeIO GetOrCreate(Type T) => Get(T) ?? Create(T);

		private static ITypeIO Create(Type T)
		{
			if (T.IsCollection())
			{
				var ElementType = T.GetCollectionElementType();

				// Check element type exist
				var IO = ListElementMap.GetValueOrNull(ElementType);
				if (IO != null)
					ListMap.Add(T, IO);
				else
				{
					IO = new ListIO(ElementType);
					ListMap.Add(T, IO);
					ListElementMap.Add(ElementType, IO);

					CreateIfNotExist(ElementType);
				}

				return IO;
			}

			var CIO = new CompositeTypeIO(T);
			CompositeMap.Add(T, CIO);
			CIO.Init();
			return CIO;
		}

		private static void CreateIfNotExist(Type T) { if (!Exist(T)) Create(T); }
	}
}
