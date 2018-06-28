using EasyTCP;

namespace SimpleMessenger
{
	internal class Vector
	{
		[EasyTCP(0)] public float x;
		[EasyTCP(1)] public float y;
		[EasyTCP(2)] public float z;

		public override string ToString() => $"<{x}, {y}, {z}>";
	}
}
