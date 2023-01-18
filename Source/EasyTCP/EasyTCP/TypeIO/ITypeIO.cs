using System.IO;

namespace EasyTCP
{
	public interface ITypeIO
	{
		object Read(BinaryReader br);
		void Write(BinaryWriter bw, object value);
	}
}
