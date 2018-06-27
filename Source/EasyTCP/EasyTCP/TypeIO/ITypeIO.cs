using System.IO;

namespace EasyTCP
{
	internal interface ITypeIO
	{
		void Init();

		object Read(BinaryReader BR);
		void Write(BinaryWriter BW, object Value);
	}
}
