using XmlExt;
using System;
using System.Xml;
using System.Collections.Generic;

namespace Config
{
	public class EasyTCP
	{
		public readonly Version Version;
		public static readonly Version ExpectedVersion = new Version("1.0");

		public readonly string ClassName;

		public readonly List<PacketData> Packet = new List<PacketData>();

		public EasyTCP(string Filename)
		{
			var Doc = new XmlDocument();
			Doc.Load(Filename);

			var Node = Doc.SelectSingleNode("EasyTCP");

			// Check version
			Version = new Version(Node.Attr("Version"));
			if (Version.Major != ExpectedVersion.Major || Version.Minor < ExpectedVersion.Minor)
				throw new Exception("Version Mismatch");

			ClassName = Node.Attr("ClassName");

			foreach (XmlNode X in Node.SelectNodes("Packet"))
				Packet.Add(new PacketData(X));
		}

		public class PacketData
		{
			public readonly int Code;

			/// <summary>
			/// Used for method, event and delegate names (Must be unique)
			/// </summary>
			public readonly string Name;

			public readonly string Desc;

			public readonly List<DataData> Data = new List<DataData>();

			public PacketData(XmlNode Node)
			{
				Code = Node.iAttr("Code");
				Name = Node.Attr("Name");
				Desc = Node.Attr("Desc", "");

				foreach (XmlNode X in Node.SelectNodes("Data"))
					Data.Add(new DataData(X));
			}

			public struct DataData
			{
				public readonly string Name;

				public readonly string Type;

				public readonly string Desc;

				public readonly bool isList;

				public DataData(XmlNode Node)
				{
					Name = Node.Attr("Name");
					Type = Node.Attr("Type");
					Desc = Node.Attr("Desc", "");
					isList = Node.ynAttr("isList", false);
				}
			}
		}
	}
}
