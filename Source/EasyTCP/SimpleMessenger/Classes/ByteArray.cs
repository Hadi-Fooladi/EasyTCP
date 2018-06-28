using System.IO;

using EasyTCP;
using EasyTCP.Attributes;

namespace SimpleMessenger
{
	[IO(typeof(ByteArray))]
	internal class ByteArray : ITypeIO
	{
		public ByteArray() { }

		public ByteArray(byte[] B, int Start, int Count)
		{
			this.B = B;
			this.Start = Start;
			this.Count = Count;
		}

		public ByteArray(byte[] B) : this(B, 0, B.Length) { }

		public ByteArray(BinaryReader BR)
		{
			Start = 0;
			Count = BR.ReadInt32();
			B = BR.ReadBytes(Count);
		}

		public readonly byte[] B;
		public readonly int Start, Count;

		public object Read(BinaryReader BR) => new ByteArray(BR);

		public void Write(BinaryWriter BW, object Value)
		{
			var A = (ByteArray)Value;

			BW.Write(A.Count);
			BW.Write(A.B, A.Start, A.Count);
		}
	}
}
