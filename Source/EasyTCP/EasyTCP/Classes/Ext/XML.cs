using System;
using System.Xml;

namespace XmlExt
{
	/// <summary>
	/// Useful extension methods for working with XML documents
	/// </summary>
	public static class XmlExt
	{
		public static bool ConvertToYN(string S) => string.Equals(S, "Yes", StringComparison.OrdinalIgnoreCase);

		public static void Attr<T>(this XmlNode Node, string Name, out T Value, T Default, Func<string, T> Converter)
		{
			var A = Node.Attributes[Name];
			Value = A == null ? Default : Converter(A.Value);
		}

		public static T Attr<T>(this XmlNode Node, string Name, T Default, Func<string, T> Converter)
		{
			var A = Node.Attributes[Name];
			return A == null ? Default : Converter(A.Value);
		}

		public static string Attr(this XmlNode Node, string Name) => Node.Attributes[Name].Value;
		public static bool ynAttr(this XmlNode Node, string Name) => ConvertToYN(Node.Attr(Name));
		public static int iAttr(this XmlNode Node, string Name) => Convert.ToInt32(Node.Attr(Name));
		public static char chAttr(this XmlNode Node, string Name) => Convert.ToChar(Node.Attr(Name));
		public static float fAttr(this XmlNode Node, string Name) => Convert.ToSingle(Node.Attr(Name));
		public static Version verAttr(this XmlNode Node, string Name) => Node.Attr(Name, null, S => new Version(S));

		public static string Attr(this XmlNode Node, string Name, string Default) => Node.Attr(Name, Default, S => S);
		public static bool ynAttr(this XmlNode Node, string Name, bool Default) => Node.Attr(Name, Default, ConvertToYN);
		public static int iAttr(this XmlNode Node, string Name, int Default) => Node.Attr(Name, Default, Convert.ToInt32);
		public static char chAttr(this XmlNode Node, string Name, char Default) => Node.Attr(Name, Default, Convert.ToChar);
		public static float fAttr(this XmlNode Node, string Name, float Default) => Node.Attr(Name, Default, Convert.ToSingle);

		public static XmlElement AppendNode(this XmlNode Node, string Name)
		{
			var E = Node.OwnerDocument.CreateElement(Name);
			Node.AppendChild(E);
			return E;
		}

		public static XmlElement AppendNode(this XmlDocument Doc, string Name)
		{
			var E = Doc.CreateElement(Name);
			Doc.AppendChild(E);
			return E;
		}

		public static XmlAttribute AddAttr(this XmlNode Node, string Name, string Value)
		{
			XmlAttribute A = Node.OwnerDocument.CreateAttribute(Name);
			A.Value = Value;
			Node.Attributes.Append(A);
			return A;
		}

		public static XmlAttribute AddAttr(this XmlNode Node, string Name, object Value) => Node.AddAttr(Name, Value.ToString());
		public static XmlAttribute AddAttr(this XmlNode Node, string Name, bool Value) => Node.AddAttr(Name, Value ? "Yes" : "No");

		#region Nullable Types
		public static void Attr(this XmlNode Node, string Name, out int? Value) => Value = Node.iAttr(Name);
		public static void Attr(this XmlNode Node, string Name, out bool? Value) => Value = Node.ynAttr(Name);
		public static void Attr(this XmlNode Node, string Name, out char? Value) => Value = Node.chAttr(Name);
		public static void Attr(this XmlNode Node, string Name, out float? Value) => Value = Node.fAttr(Name);

		public static void Attr(this XmlNode Node, string Name, out bool? Value, bool? Default) => Node.Attr(Name, out Value, Default, S => ConvertToYN(S));
		public static void Attr(this XmlNode Node, string Name, out int? Value, int? Default) => Node.Attr(Name, out Value, Default, S => Convert.ToInt32(S));
		public static void Attr(this XmlNode Node, string Name, out char? Value, char? Default) => Node.Attr(Name, out Value, Default, S => Convert.ToChar(S));
		public static void Attr(this XmlNode Node, string Name, out float? Value, float? Default) => Node.Attr(Name, out Value, Default, S => Convert.ToSingle(S));
		#endregion
	}
}