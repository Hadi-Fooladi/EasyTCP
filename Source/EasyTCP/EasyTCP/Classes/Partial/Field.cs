using EasyTCP;

namespace Config
{
	internal partial class Field
	{
		public void WriteRead(string VarName)
			=> Global.SW.WriteLine(isList ?
				$"{VarName} = BR.ReadArray<{Type}>(BR.Read);" :
				$"BR.Read(out {VarName});");


		public void WriteWrite(string VarName)
			=> Global.SW.WriteLine(isList ?
				$"BW.WriteArray({VarName}, BW.Write);" :
				$"BW.Write({VarName});");

		public void WriteRead() => WriteRead(Name);
		public void WriteWrite() => WriteWrite(Name);
	}
}
