using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace EasyTCP
{
	internal class CompositeTypeIO : ITypeIO
	{
		public CompositeTypeIO(Type T)
		{
			this.T = T;
			if (TypeIOs.Composite[T] != null) return;

			TypeIOs.AddComposite(T, this);

			var CT = T.GetCollectionInterface();
			if (CT != null)
			{
				isCollection = true;
				GetOrCreate(ListType = T.GetCollectionElementType());
				return;
			}

			foreach (var F in T.GetFields())
			{
				var A = F.GetCustomAttribute<EasyTCPAttribute>();
				if (A != null)
					Fields.Add(A.Order, new CField(F));
			}
		}

		#region Fields
		private static readonly SortedList<int, CField> Fields = new SortedList<int, CField>();

		private readonly Type T;

		public readonly Type ListType;
		public readonly bool isCollection;
		#endregion

		#region Public Methods
		public static ITypeIO GetOrCreate(Type T) => TypeIOs.All[T] ?? new CompositeTypeIO(T);
		#endregion

		#region ITypeIO Members
		public object Read(BinaryReader BR)
		{
			if (isCollection)
			{
				int i, n = BR.ReadInt32();

				var IO = TypeIOs.All[ListType];
				var A = Array.CreateInstance(ListType, n);

				for (i = 0; i < n; i++)
					A.SetValue(IO.Read(BR), i);

				return A;
			}

			var Result = Activator.CreateInstance(T);

			foreach (var F in Fields.Values)
				F.Info.SetValue(Result, F.IO.Read(BR));

			return Result;
		}

		public void Write(BinaryWriter BW, object Value)
		{
			if (isCollection)
			{
				var C = (ICollection)Value;
				var IO = TypeIOs.All[ListType];

				BW.Write(C.Count);
				foreach (var X in C)
					IO.Write(BW, X);
			}
			else
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
				IO = GetOrCreate(Info.FieldType);
			}
		}
		#endregion
	}
}
