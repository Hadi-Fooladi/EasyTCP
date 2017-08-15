using System;
using System.IO;

namespace EasyTCP
{
	/// <summary>
	/// StreamWriter which supports Indentation
	/// </summary>
	internal class IndentedStreamWriter : StreamWriter
	{
		public IndentedStreamWriter(string path) : base(path) { }

		public int IndentationCount;

		/// <summary>
		/// true if there is a blank line before current line
		/// </summary>
		private bool BlankLine;

		/// <summary>
		/// Used to remove blank line after '{'
		/// </summary>
		private bool OpenBrace;

		public override void WriteLine() => BlankLine = true;

		public override void WriteLine(string value)
		{
			if (BlankLine)
			{
				if (!OpenBrace)
					base.WriteLine();

				BlankLine = false;
			}
			Indent();
			OpenBrace = false;
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
			OpenBrace = true;
			Inside(A);
			BlankLine = false; // Removing blank line before '}'
			WriteLine("}");
			WriteLine();
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
					if (S == "") WriteLine();
					else WriteLine(S);
			}
		}

		public void Region(string Name, Action A)
		{
			WriteLine();
			WriteLine($"#region {Name}");
			OpenBrace = true;
			A();
			BlankLine = false;
			WriteLine("#endregion");
			WriteLine();
		}

		public void TryCatch(Action Try, Action Catch)
		{
			WriteLine("try");
			Block(Try);
			BlankLine = false;
			WriteLine("catch (Exception E)");
			Block(Catch);
		}
	}
}
