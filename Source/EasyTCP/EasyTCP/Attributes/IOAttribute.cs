using System;

namespace EasyTCP.Attributes
{
	/// <summary>
	/// Used for doing customized IO (e.g. huge arrays)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class IOAttribute : Attribute
	{
		/// <param name="IOType">Must implement 'ITypeIO' and must have a default constructor</param>
		public IOAttribute(Type IOType) => this.IOType = IOType;

		public readonly Type IOType;
	}
}
