using System.IO;

namespace SimpleMessenger
{
	internal static class MiscExt
	{
		public static void Write(this BinaryWriter BW, ByteArray A)
		{
			BW.Write(A.Count);
			BW.Write(A.B, A.Start, A.Count);
		}

		public static void Read(this BinaryReader BR, out ByteArray A) => A = new ByteArray(BR);
	}
}
