using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace EasyTCP
{
	internal class CompositeTypeIO : ITypeIO
	{
		public CompositeTypeIO(Type T) => this.T = T;

		#region Fields
		private readonly Type T;
		private readonly SortedList<int, CField> Fields = new SortedList<int, CField>();
		#endregion

		#region ITypeIO Members
		public void Init()
		{
			foreach (var F in T.GetFields())
			{
				var A = F.GetCustomAttribute<EasyTCPAttribute>();
				if (A != null)
					Fields.Add(A.Order, new CField(F));
			}
		}

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

		#region Nested Classes
		private class CField
		{
			public readonly ITypeIO IO;
			public readonly FieldInfo Info;

			public CField(FieldInfo Info)
			{
				this.Info = Info;
				IO = TypeIOs.GetOrCreate(Info.FieldType);
			}
		}
		#endregion
	}
}
