using System;

using Packet = Config.EasyTCP.PacketData;

namespace EasyTCP
{
	internal static class Misc
	{
		public static string Arguments(this Packet P)
		{
			string S = "";
			var isFirst = true;
			foreach (var D in P.Data)
			{
				if (isFirst) isFirst = false;
				else S += ", ";

				S += D.Name;
			}
			return S;
		}

		private static string Signature(this Packet P, bool UseArray)
		{
			string S = "";
			var isFirst = true;
			foreach (var D in P.Data)
			{
				if (isFirst) isFirst = false;
				else S += ", ";

				string Type;
				if (D.isList)
					Type = UseArray ?
						D.Type + "[]" :
						string.Format("ICollection<{0}>", D.Type);
				else
					Type = D.Type;

				S += String.Format("{0} {1}", Type, D.Name);
			}
			return S;
		}

		public static string SendSignature(this Packet P) => P.Signature(false);
		public static string EventSignature(this Packet P) => P.Signature(true);
	}
}
