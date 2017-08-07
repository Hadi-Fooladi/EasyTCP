namespace Config
{
	internal partial class EnumMember
	{
		public override string ToString() => Value.HasValue ? $"{Name} = {Value}" : Name;
	}
}
