using System;
using System.IO;
using System.Collections;

namespace EasyTCP
{
	internal class ListIO : ITypeIO
	{
		public ListIO(Type ElementType, TypeIOs IOs)
		{
			this.ElementType = ElementType;
			ElementIO = IOs.GetOrCreate(ElementType);
		}

		private readonly Type ElementType;
		private readonly ITypeIO ElementIO;

		public object Read(BinaryReader BR)
		{
			int i, n = BR.ReadInt32();

			var A = Array.CreateInstance(ElementType, n);

			for (i = 0; i < n; i++)
				A.SetValue(ElementIO.Read(BR), i);

			return A;
		}

		public void Write(BinaryWriter BW, object Value)
		{
			var C = (ICollection)Value;

			BW.Write(C.Count);
			foreach (var X in C)
				ElementIO.Write(BW, X);
		}
	}
}
