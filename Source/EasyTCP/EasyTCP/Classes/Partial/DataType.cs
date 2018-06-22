using EasyTCP;

namespace Config
{
	internal partial class DataType
	{
		public void Declare()
		{
			if (!Implement) return;

			var SW = Global.SW;

			SW.WriteLine($"internal{(Partial ? " partial" : "")} {(isClass ? "class" : "struct")} {Name}");
			SW.Block(() =>
			{
				string FieldType(Field F) => F.isList ? $"IList<{F.Type}>" : F.Type;

				// Declaring Fields
				foreach (var F in Fields)
					SW.WriteLine($"public {FieldType(F)} {F.Name};");
			});
		}
	}
}
