using EasyTCP;
using System.IO;

namespace SimpleMessenger
{
	internal class Vector
	{
		[EasyTCP(0)] public float x;
		[EasyTCP(1)] public float y;
		[EasyTCP(2)] public float z;

		public override string ToString() => $"<{x}, {y}, {z}>";
	}

	internal class VectorIO : ITypeIO
	{
		public object Read(BinaryReader BR)
			=> new Vector
			{
				x = BR.ReadSingle(),
				y = BR.ReadSingle(),
				z = -1
			};

		public void Write(BinaryWriter BW, object Value)
		{
			var Vec = (Vector)Value;

			BW.Write(Vec.x);
			BW.Write(Vec.y);
		}
	}
}
