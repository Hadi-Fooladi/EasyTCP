using System.IO;

namespace EasyTCP
{
	internal class ByteArray
	{
		public ByteArray(byte[] B, int Start, int Count)
		{
			this.B = B;
			this.Start = Start;
			this.Count = Count;
		}

		public ByteArray(BinaryReader BR)
		{
			Start = 0;
			Count = BR.ReadInt32();
			B = BR.ReadBytes(Count);
		}

		public readonly byte[] B;
		public readonly int Start, Count;
	}
}
