using System;
using System.IO;

namespace EasyTCP
{
	/// <summary>
	/// StreamWriter which supports Indentation
	/// </summary>
	internal class IndentedStreamWriter : StreamWriter
	{
		public int IndentationCount;

		public IndentedStreamWriter(string path) : base(path) { }

		public override void WriteLine(string value)
		{
			Indent();
			base.WriteLine(value);
		}

		public override void WriteLine(string format, object arg0) => WriteLine(string.Format(format, arg0));
		public override void WriteLine(string format, params object[] arg) => WriteLine(string.Format(format, arg));
		public override void WriteLine(string format, object arg0, object arg1) => WriteLine(string.Format(format, arg0, arg1));
		public override void WriteLine(string format, object arg0, object arg1, object arg2) => WriteLine(string.Format(format, arg0, arg1, arg2));

		public void Indent()
		{
			for (int i = 0; i < IndentationCount; i++)
				Write('\t');
		}

		public void Block(Action A)
		{
			WriteLine("{");
			Inside(A);
			WriteLine("}");
		}

		public void WriteDesc(string Desc)
		{
			if (string.IsNullOrEmpty(Desc)) return;

			WriteLine("/// <summary>");
			WriteLine("/// {0}", Desc);
			WriteLine("/// </summary>");
		}

		public void WriteParameterDesc(string Name, string Desc)
		{
			if (!string.IsNullOrEmpty(Desc))
				WriteLine("/// <param name=\"{0}\">{1}</param>", Name, Desc);
		}

		public void Inside(Action A)
		{
			IndentationCount++;
			A();
			IndentationCount--;
		}

		public void WriteBulk(string Bulk)
		{
			using (var SR = new StringReader(Bulk))
			{
				string S;
				while (null != (S = SR.ReadLine()))
					WriteLine(S);
			}
		}
	}
}
