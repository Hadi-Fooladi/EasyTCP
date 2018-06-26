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
			DefinePacket<List<int>>(2); // Random Numbers
		}
	}
}
