using System;
using System.Collections.Generic;

namespace EasyTCP
{
	internal static class TypeIOs
	{
		private static readonly IReadOnlyDictionary<Type, ITypeIO> PrimaryMap = new Dictionary<Type, ITypeIO>
		{
			{ typeof(int), new Int32TypeIO() },
			{ typeof(float), new SingleTypeIO() },
			{ typeof(string), new StringTypeIO() }
		};

		private static readonly Dictionary<Type, ITypeIO> CompositeMap = new Dictionary<Type, ITypeIO>();

		public static readonly Indexer
			Primary = new Indexer(PrimaryMap),
			Composite = new Indexer(CompositeMap);

		public static AllIndexer All = new AllIndexer();

		public static ITypeIO GetIO<T>() => All[typeof(T)];
		public static ITypeIO GetPrimary<T>() => Primary[typeof(T)];
		public static ITypeIO GetComposite<T>() => Composite[typeof(T)];

		public static void AddComposite(Type T, ITypeIO IO) => CompositeMap.Add(T, IO);

		public static IEnumerable<CompositeTypeIO> CollectionIOs
		{
			get
			{
				foreach (CompositeTypeIO IO in CompositeMap.Values)
					if (IO.isCollection)
						yield return IO;
			}
		}

		public class Indexer
		{
			private readonly IReadOnlyDictionary<Type, ITypeIO> Map;

			public Indexer(IReadOnlyDictionary<Type, ITypeIO> Map) => this.Map = Map;

			public ITypeIO this[Type T] => Map.TryGetValue(T, out var IO) ? IO : null;
		}

		public class AllIndexer
		{
			public ITypeIO this[Type T] => Primary[T] ?? Composite[T];
		}
	}
}
