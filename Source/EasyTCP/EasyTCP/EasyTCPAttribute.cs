using System;

namespace EasyTCP
{
	[AttributeUsage(AttributeTargets.Field)]
	public class EasyTCPAttribute : Attribute
	{
		public readonly int Order;

		public EasyTCPAttribute(int Order) => this.Order = Order;
	}
}
