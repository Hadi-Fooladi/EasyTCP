using System;
using System.Reflection;
using System.Collections.Generic;

namespace EasyTCP
{
	internal class TypeIOs
	{
		private static readonly IDictionary<Type, ITypeIO> PrimitiveIOs = new Dictionary<Type, ITypeIO>()
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
			{ typeof(sbyte), new SByteTypeIO() },
			{ typeof(byte[]), new ByteArrayTypeIO() }
		};

		private readonly Dictionary<Type, ITypeIO> Map = new Dictionary<Type, ITypeIO>(PrimitiveIOs);

		private readonly Dictionary<Type, ListIO> ListElementMap = new Dictionary<Type, ListIO>();

		public bool Exist(Type T) => Map.ContainsKey(T);
		public ITypeIO Get(Type T) => Map.GetValueOrNull(T);

		public ITypeIO GetOrCreate(Type T) => Get(T) ?? Create(T);

		public void Add(Type T, ITypeIO IO) => Map.Add(T, IO);

		private ITypeIO Create(Type T)
		{
			var IOA = T.GetCustomAttribute<Attributes.IOAttribute>();
			if (IOA != null)
			{
				var IO = (ITypeIO)Activator.CreateInstance(IOA.IOType);
				Add(T, IO);
				return IO;
			}

			if (T.IsCollection())
			{
				var ElementType = T.GetCollectionElementType();

				// Check element type exist
				var IO = ListElementMap.GetValueOrNull(ElementType);
				if (IO != null)
					Add(T, IO);
				else
				{
					IO = new ListIO(ElementType, this);

					Add(T, IO);
					ListElementMap.Add(ElementType, IO);
				}

				return IO;
			}

			var CIO = new CompositeTypeIO(T);
			Add(T, CIO);
			CIO.Init(this);
			return CIO;
		}

		private void CreateIfNotExist(Type T) { if (!Exist(T)) Create(T); }
	}
}
