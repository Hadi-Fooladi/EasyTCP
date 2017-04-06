namespace EasyTCP
{
	internal interface IParameter
	{
		string Code { get; }
		string Desc { get; }

		string CodeParams { get; }

		/// <param name="args">Application arguments array</param>
		/// <param name="ndx">
		/// Index of the argument which must be processed.<br />
		/// After processing, <param name="ndx" /> must be updated proportionally to point to the next argument.
		/// </param>
		void Process(string[] args, ref int ndx);
	}
}
