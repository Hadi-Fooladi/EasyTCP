using System.IO;

namespace EasyTCP
{
	internal class Int32TypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadInt32();
		public void Write(BinaryWriter BW, object Value) => BW.Write((int)Value);
	}

	internal class SingleTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadSingle();
		public void Write(BinaryWriter BW, object Value) => BW.Write((float)Value);
	}

	internal class StringTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadString();
		public void Write(BinaryWriter BW, object Value) => BW.Write((string)Value);
	}

	internal class ByteTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadByte();
		public void Write(BinaryWriter BW, object Value) => BW.Write((byte)Value);
	}

	internal class CharTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadChar();
		public void Write(BinaryWriter BW, object Value) => BW.Write((char)Value);
	}

	internal class BooleanTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadBoolean();
		public void Write(BinaryWriter BW, object Value) => BW.Write((bool)Value);
	}

	internal class Int16TypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadInt16();
		public void Write(BinaryWriter BW, object Value) => BW.Write((short)Value);
	}

	internal class Int64TypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadInt64();
		public void Write(BinaryWriter BW, object Value) => BW.Write((long)Value);
	}

	internal class UInt32TypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadUInt32();
		public void Write(BinaryWriter BW, object Value) => BW.Write((uint)Value);
	}

	internal class UInt16TypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadUInt16();
		public void Write(BinaryWriter BW, object Value) => BW.Write((ushort)Value);
	}

	internal class UInt64TypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadUInt64();
		public void Write(BinaryWriter BW, object Value) => BW.Write((ulong)Value);
	}

	internal class DoubleTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadDouble();
		public void Write(BinaryWriter BW, object Value) => BW.Write((double)Value);
	}

	internal class DecimalTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadDecimal();
		public void Write(BinaryWriter BW, object Value) => BW.Write((decimal)Value);
	}

	internal class SByteTypeIO : ITypeIO
	{
		public object Read(BinaryReader BR) => BR.ReadSByte();
		public void Write(BinaryWriter BW, object Value) => BW.Write((sbyte)Value);
	}
}

