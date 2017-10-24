using System;
using System.IO;
using System.Collections.Generic;

namespace EasyTCP
{
	internal static partial class Ext
	{
		public delegate void dlgRead<T>(out T Value);

		public static void Read(this BinaryReader BR, out int Value) { Value = BR.ReadInt32(); }
		public static void Read(this BinaryReader BR, out bool Value) { Value = BR.ReadBoolean(); }
		public static void Read(this BinaryReader BR, out char Value) { Value = BR.ReadChar(); }
		public static void Read(this BinaryReader BR, out byte Value) { Value = BR.ReadByte(); }
		public static void Read(this BinaryReader BR, out sbyte Value) { Value = BR.ReadSByte(); }
		public static void Read(this BinaryReader BR, out short Value) { Value = BR.ReadInt16(); }
		public static void Read(this BinaryReader BR, out ushort Value) { Value = BR.ReadUInt16(); }
		public static void Read(this BinaryReader BR, out uint Value) { Value = BR.ReadUInt32(); }
		public static void Read(this BinaryReader BR, out long Value) { Value = BR.ReadInt64(); }
		public static void Read(this BinaryReader BR, out ulong Value) { Value = BR.ReadUInt64(); }
		public static void Read(this BinaryReader BR, out string Value) { Value = BR.ReadString(); }
		public static void Read(this BinaryReader BR, out float Value) { Value = BR.ReadSingle(); }
		public static void Read(this BinaryReader BR, out double Value) { Value = BR.ReadDouble(); }
		public static void Read(this BinaryReader BR, out decimal Value) { Value = BR.ReadDecimal(); }

		public static void WriteArray<T>(this BinaryWriter BW, ICollection<T> A, Action<T> Write)
		{
			BW.Write((ushort)A.Count);
			foreach (var X in A)
				Write(X);
		}

		public static T[] ReadArray<T>(this BinaryReader BR, dlgRead<T> Read)
		{
			var Count = BR.ReadUInt16();

			var A = new T[Count];

			for (ushort i = 0; i < Count; i++)
				Read(out A[i]);

			return A;
		}

		public static void Write(this BinaryWriter BW, Enum E) { BW.Write(E.GetHashCode()); }

		public static void Fire(this Action A) { if (A != null) A(); }
		public static void Fire<T>(this Action<T> A, T Value) { if (A != null) A(Value); }
		public static void Fire<T1, T2>(this Action<T1, T2> A, T1 V1, T2 V2) { if (A != null) A(V1, V2); }
	}
}
