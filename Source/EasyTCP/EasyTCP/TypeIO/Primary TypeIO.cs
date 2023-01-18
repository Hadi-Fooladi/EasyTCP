using System.IO;

namespace EasyTCP
{
	class Int32TypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadInt32();
		public void Write(BinaryWriter bw, object value) => bw.Write((int)value);
	}

	class SingleTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadSingle();
		public void Write(BinaryWriter bw, object value) => bw.Write((float)value);
	}

	class StringTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadString();
		public void Write(BinaryWriter bw, object value) => bw.Write((string)value);
	}

	class ByteTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadByte();
		public void Write(BinaryWriter bw, object value) => bw.Write((byte)value);
	}

	class CharTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadChar();
		public void Write(BinaryWriter bw, object value) => bw.Write((char)value);
	}

	class BooleanTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadBoolean();
		public void Write(BinaryWriter bw, object value) => bw.Write((bool)value);
	}

	class Int16TypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadInt16();
		public void Write(BinaryWriter bw, object value) => bw.Write((short)value);
	}

	class Int64TypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadInt64();
		public void Write(BinaryWriter bw, object value) => bw.Write((long)value);
	}

	class UInt32TypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadUInt32();
		public void Write(BinaryWriter bw, object value) => bw.Write((uint)value);
	}

	class UInt16TypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadUInt16();
		public void Write(BinaryWriter bw, object value) => bw.Write((ushort)value);
	}

	class UInt64TypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadUInt64();
		public void Write(BinaryWriter bw, object value) => bw.Write((ulong)value);
	}

	class DoubleTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadDouble();
		public void Write(BinaryWriter bw, object value) => bw.Write((double)value);
	}

	class DecimalTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadDecimal();
		public void Write(BinaryWriter bw, object value) => bw.Write((decimal)value);
	}

	class SByteTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadSByte();
		public void Write(BinaryWriter bw, object value) => bw.Write((sbyte)value);
	}

	class ByteArrayTypeIO : ITypeIO
	{
		public object Read(BinaryReader br) => br.ReadBytes(br.ReadInt32());

		public void Write(BinaryWriter bw, object value)
		{
			var bytes = (byte[])value;

			bw.Write(bytes.Length);
			bw.Write(bytes);
		}
	}
}
