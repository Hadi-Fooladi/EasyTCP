using System;
using System.Reflection;
using System.Collections.Generic;

namespace EasyTCP
{
	internal class TypeIOs
	{
		static readonly IDictionary<Type, ITypeIO> s_primitiveIOs = new Dictionary<Type, ITypeIO>
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

		readonly Dictionary<Type, ITypeIO> _ioByType = new Dictionary<Type, ITypeIO>(s_primitiveIOs);
		readonly Dictionary<Type, ListIO> _listIOByElementType = new Dictionary<Type, ListIO>();

		public bool Exist(Type type) => _ioByType.ContainsKey(type);
		public ITypeIO Get(Type type) => _ioByType.GetValueOrNull(type);
		public ITypeIO GetOrCreate(Type type) => Get(type) ?? Create(type);

		public void Add(Type type, ITypeIO io) => _ioByType.Add(type, io);

		ITypeIO Create(Type type)
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
				var io = s_primitiveIOs[type.GetEnumUnderlyingType()];
				Add(type, io);
				return io;
			}

			if (type.IsCollection())
			{
				var elementType = type.GetCollectionElementType();

				// Check element type exist
				var io = _listIOByElementType.GetValueOrNull(elementType);
				if (io != null)
					Add(type, io);
				else
				{
					io = new ListIO(elementType, this);

					Add(type, io);
					_listIOByElementType.Add(elementType, io);
				}

				return io;
			}

			var cio = new CompositeTypeIO(type);
			Add(type, cio);
			cio.Init(this);
			return cio;
		}

		void CreateIfNotExist(Type type) { if (!Exist(type)) Create(type); }
	}
}
