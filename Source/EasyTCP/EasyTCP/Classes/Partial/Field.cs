using EasyTCP;

namespace Config
{
	internal partial class Field
	{
		public void WriteRead(IndentedStreamWriter SW, string VarName)
			=> SW.WriteLine(isList ?
				$"{VarName} = BR.ReadArray<{Type}>(BR.Read);" :
				$"BR.Read(out {VarName});");


		public void WriteWrite(IndentedStreamWriter SW, string VarName)
			=> SW.WriteLine(isList ?
				$"BW.WriteArray({VarName}, BW.Write);" :
				$"BW.Write({VarName});");

		public void WriteRead(IndentedStreamWriter SW) => WriteRead(SW, Name);
		public void WriteWrite(IndentedStreamWriter SW) => WriteWrite(SW, Name);
	}
}
