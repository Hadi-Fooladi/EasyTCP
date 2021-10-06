using System;
using System.Reflection;
using System.Collections.Generic;

namespace EasyTCP
{
	internal class TypeIOs
	{
		private static readonly IDictionary<Type, ITypeIO> PrimitiveIOs = new Dictionary<Type, ITypeIO>
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

		private ITypeIO Create(Type type)
		{
			var ioAttr = type.GetCustomAttribute<Attributes.IOAttribute>();
			if (ioAttr != null)
			{
				var io = (ITypeIO)Activator.CreateInstance(ioAttr.IOType);
				Add(type, io);
				return io;
			}

			if (type.IsEnum)
			{
				var io = PrimitiveIOs[type.GetEnumUnderlyingType()];
				Add(type, io);
				return io;
			}

			if (type.IsCollection())
			{
				var elementType = type.GetCollectionElementType();

				// Check element type exist
				var io = ListElementMap.GetValueOrNull(elementType);
				if (io != null)
					Add(type, io);
				else
				{
					io = new ListIO(elementType, this);

					Add(type, io);
					ListElementMap.Add(elementType, io);
				}

				return io;
			}

			var cio = new CompositeTypeIO(type);
			Add(type, cio);
			cio.Init(this);
			return cio;
		}

		private void CreateIfNotExist(Type T) { if (!Exist(T)) Create(T); }
	}
}
