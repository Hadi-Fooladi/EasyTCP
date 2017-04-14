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
	}
}
