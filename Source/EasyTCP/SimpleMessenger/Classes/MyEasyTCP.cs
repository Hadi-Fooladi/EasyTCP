using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessenger
{
	internal class MyEasyTCP : EasyTCP.EasyTCP
	{
		public MyEasyTCP(int WriteBufferLength) : base(WriteBufferLength)
		{
			DefinePacket<string>(0); // Name
			DefinePacket<string>(1); // Message
			DefinePacket<IReadOnlyCollection<int>>(2); // Random Numbers
			DefinePacket<Vector>(3);
			DefinePacket<Person>(4);
		}
	}
}
