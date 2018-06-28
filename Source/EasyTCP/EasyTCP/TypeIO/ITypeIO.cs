using System.IO;

namespace EasyTCP
{
	public interface ITypeIO
	{
		object Read(BinaryReader BR);
		void Write(BinaryWriter BW, object Value);
	}
}
