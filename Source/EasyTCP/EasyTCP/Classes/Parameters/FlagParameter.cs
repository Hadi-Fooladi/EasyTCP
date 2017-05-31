namespace EasyTCP
{
	/// <summary>
	/// Used to check whether the code exist in the command line arguments or not
	/// </summary>
	internal class FlagParameter : IParameter
	{
		public FlagParameter() { }
		public FlagParameter(string Code, string Desc)
		{
			this.Code = Code;
			this.Desc = Desc;
		}

		public bool Exist { get; private set; }

		#region IParameter
		public string Code { get; set; }
		public string Desc { get; set; }
		public string CodeParams => "";
		public void Process(string[] args, ref int ndx) => Exist = true;
		#endregion
	}
}
