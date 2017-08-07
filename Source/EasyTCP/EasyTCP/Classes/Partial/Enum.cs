using EasyTCP;

namespace Config
{
	internal partial class Enum
	{
		public void Declare()
		{
			WriteDesc();

			var SW = Global.SW;
			SW.WriteLine($"internal enum {Name}");

			SW.Block(() =>
			{
				int i, n = Members.Count - 1;
				for (i = 0; i <= n; i++)
				{
					var M = Members[i];
					M.WriteDesc();
					SW.WriteLine(M + (i == n ? "" : ","));
				}
			});
		}
	}
}
