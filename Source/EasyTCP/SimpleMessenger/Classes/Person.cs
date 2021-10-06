using System;
using System.Text;
using System.Collections.Generic;

using EasyTCP;

namespace SimpleMessenger
{
	internal class Person
	{
		[EasyTCP(0)] public string Name;

		[EasyTCP(1)] public int Age;

		[EasyTCP(2)] public IReadOnlyCollection<Person> Children;

		[EasyTCP(3)] public Gender Gender;

		public Person() { }

		public Person(int MaxAge)
		{
			var Rnd = Global.Rnd;

			Name = RandomName;
			Age = Rnd.Next(MaxAge) + 1;

			Gender = (Gender)(Age / 25);

			List<Person> L = new List<Person>();
			if (Age > 21)
			{
				int nChild = Rnd.Next(Age < 30 ? 2 : 5);

				int ChildMaxAge = Age - 20;
				while (nChild-- > 0)
					L.Add(new Person(ChildMaxAge));
			}

			Children = L;
		}

		private static string RandomName
		{
			get
			{
				int n = Global.Rnd.Next(10) + 1;
				var SB = new StringBuilder(n);
				while (n-- > 0)
					SB.Append(Char.ConvertFromUtf32('a' + Global.Rnd.Next(26)));
				return SB.ToString();
			}
		}

		public override string ToString() => Show(0);

		public string Show(int Indention)
		{
			var SB = new StringBuilder();

			SB.AppendLine();
			for (int i = 0; i < Indention; i++) SB.Append(' ');

			SB.Append($"{Name} ({Age}) [{Gender}]");

			Indention += 3;
			foreach (var Child in Children)
				SB.Append(Child.Show(Indention));

			return SB.ToString();
		}
	}
}
