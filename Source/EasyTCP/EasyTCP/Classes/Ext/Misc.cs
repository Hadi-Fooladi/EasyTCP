using System;

using Packet = Config.EasyTCP.PacketData;

namespace EasyTCP
{
	internal static class Misc
	{
		public static string Arguments(this Packet P) => Signature(P, true);
		public static string Signature(this Packet P, bool HideType = false)
		{
			string S = "";
			var isFirst = true;
			foreach (var D in P.Data)
			{
				if (isFirst) isFirst = false;
				else S += ", ";

				string Type;
				if (HideType) Type = "";
				else
					Type = (D.isList ? string.Format("IReadOnlyCollection<{0}>", D.Type) : D.Type) + " ";

				S += String.Format("{0}{1}", Type, D.Name);
			}
			return S;
		}
	}
}
