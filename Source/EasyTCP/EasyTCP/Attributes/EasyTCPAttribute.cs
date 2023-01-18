using System;

namespace EasyTCP
{
	[AttributeUsage(AttributeTargets.Field)]
	public class EasyTCPAttribute : Attribute
	{
		public readonly int Order;

		public EasyTCPAttribute(int order) { Order = order; }
	}
}
