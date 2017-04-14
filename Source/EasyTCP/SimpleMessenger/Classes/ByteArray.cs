using System.IO;

namespace SimpleMessenger
{
	internal class ByteArray
	{
		public ByteArray(byte[] B, int Start, ushort Count)
		{
			this.B = B;
			this.Start = Start;
			this.Count = Count;
		}

		public ByteArray(BinaryReader BR)
		{
			Start = 0;
			Count = BR.ReadUInt16();
			B = BR.ReadBytes(Count);
		}

		public readonly byte[] B;
		public readonly int Start;
		public readonly ushort Count;
	}
}
