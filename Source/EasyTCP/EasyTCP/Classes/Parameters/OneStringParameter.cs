using System;

namespace EasyTCP
{
	internal class OneStringParameter : IParameter
	{
		public OneStringParameter(string Code, string CodeParams, string Desc)
		{
			this.Code = Code;
			this.Desc = Desc;
			this.CodeParams = CodeParams;
			CustomProcess = DefaultProcess;
		}

		public Action<string> CustomProcess;
		public string Value { get; private set; }

		private void DefaultProcess(string S) => Value = S;

		#region IParameter
		public string Code { get; }
		public string Desc { get; }
		public string CodeParams { get; }
		public void Process(string[] args, ref int ndx) => CustomProcess(args[ndx++]);
		#endregion
	}
}
