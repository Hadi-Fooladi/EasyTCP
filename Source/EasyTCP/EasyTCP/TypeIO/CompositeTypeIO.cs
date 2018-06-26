using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace EasyTCP
{
	internal class CompositeTypeIO : ITypeIO
	{
		private readonly Type T;

		public CompositeTypeIO(Type T)
		{
			this.T = T;
			if (TypeIOs.Compostie[T] != null) return;

			TypeIOs.AddComposite(T, this);

			foreach (var F in T.GetFields())
			{
				var A = F.GetCustomAttribute<EasyTCPAttribute>();
				if (A == null) continue;

				var FieldType = F.FieldType;
				var IO = (TypeIOs.Primary[FieldType] ?? TypeIOs.Compostie[FieldType]) ?? new CompositeTypeIO(FieldType);

				Fields.Add(A.Order, new CField(F, IO));
			}
		}

		private static readonly SortedList<int, CField> Fields = new SortedList<int, CField>();

		#region ITypeIO Members
		public object Read(BinaryReader BR)
		{
			var Result = Activator.CreateInstance(T);

			foreach (var F in Fields.Values)
				F.Info.SetValue(Result, F.IO.Read(BR));

			return Result;
		}

		public void Write(BinaryWriter BW, object Value)
		{
			foreach (var F in Fields.Values)
				F.IO.Write(BW, F.Info.GetValue(Value));
		}
		#endregion

		private class CField
		{
			public readonly ITypeIO IO;
			public readonly FieldInfo Info;

			public CField(FieldInfo Info, ITypeIO IO)
			{
				this.IO = IO;
				this.Info = Info;
			}
		}

	}
}
