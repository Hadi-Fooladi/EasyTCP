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
}

