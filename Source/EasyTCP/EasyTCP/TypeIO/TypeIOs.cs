using System;
using System.Collections.Generic;

namespace EasyTCP
{
	internal static class TypeIOs
	{
		private static readonly IReadOnlyDictionary<Type, ITypeIO> PrimaryMap = new Dictionary<Type, ITypeIO>
		{
			{ typeof(int), new Int32TypeIO() },
			{ typeof(float), new SingleTypeIO() },
			{ typeof(string), new StringTypeIO() },
			{ typeof(byte), new ByteTypeIO() },
			{ typeof(char), new CharTypeIO() },
			{ typeof(bool), new BooleanTypeIO() },
			{ typeof(short), new Int16TypeIO() },
			{ typeof(long), new Int64TypeIO() },
			{ typeof(uint), new UInt32TypeIO() },
			{ typeof(ushort), new UInt16TypeIO() },
			{ typeof(ulong), new UInt64TypeIO() },
			{ typeof(double), new DoubleTypeIO() },
			{ typeof(decimal), new DecimalTypeIO() },
			{ typeof(sbyte), new SByteTypeIO() }
		};

		private static readonly Dictionary<Type, ListIO> ListMap = new Dictionary<Type, ListIO>();
		private static readonly Dictionary<Type, ListIO> ListElementMap = new Dictionary<Type, ListIO>();
		private static readonly Dictionary<Type, CompositeTypeIO> CompositeMap = new Dictionary<Type, CompositeTypeIO>();

		public static bool Exist(Type T) => PrimaryMap.ContainsKey(T) || CompositeMap.ContainsKey(T) || ListMap.ContainsKey(T);
		public static ITypeIO Get(Type T) => PrimaryMap.GetValueOrNull(T) ?? CompositeMap.GetValueOrNull(T) as ITypeIO ?? ListMap.GetValueOrNull(T);

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
