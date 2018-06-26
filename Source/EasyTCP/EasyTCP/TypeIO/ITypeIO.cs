using System.IO;

namespace EasyTCP
{
	internal interface ITypeIO
	{
		object Read(BinaryReader BR);
		void Write(BinaryWriter BW, object Value);
	}
}
