using System;
using System.IO;
using System.Collections;

namespace EasyTCP
{
	class ListIO : ITypeIO
	{
		public ListIO(Type elementType, TypeIOs container)
		{
			_elementType = elementType;
			_elementIO = container.GetOrCreate(elementType);
		}

		readonly Type _elementType;
		readonly ITypeIO _elementIO;

		public object Read(BinaryReader br)
		{
			int i, n = br.ReadInt32();
			var array = Array.CreateInstance(_elementType, n);

			for (i = 0; i < n; i++)
				array.SetValue(_elementIO.Read(br), i);

			return array;
		}

		public void Write(BinaryWriter bw, object value)
		{
			var collection = (ICollection)value;

			bw.Write(collection.Count);
			foreach (var item in collection)
				_elementIO.Write(bw, item);
		}
	}
}
