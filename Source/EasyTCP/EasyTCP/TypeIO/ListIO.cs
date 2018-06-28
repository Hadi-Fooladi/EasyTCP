using System;
using System.IO;
using System.Collections;

namespace EasyTCP
{
	internal class ListIO : ITypeIO
	{
		public ListIO(Type ElementType) => this.ElementType = ElementType;

		private readonly Type ElementType;

		public object Read(BinaryReader BR)
		{
			int i, n = BR.ReadInt32();

			var IO = TypeIOs.Get(ElementType);
			var A = Array.CreateInstance(ElementType, n);

			for (i = 0; i < n; i++)
				A.SetValue(IO.Read(BR), i);

			return A;
		}

		public void Write(BinaryWriter BW, object Value)
		{
			var C = (ICollection)Value;
			var IO = TypeIOs.Get(ElementType);

			BW.Write(C.Count);
			foreach (var X in C)
				IO.Write(BW, X);
		}
	}
}
