using System;
using System.Collections.Generic;
using EasyTCP;

namespace SimpleMessenger
{
	internal class MyEasyTCP : EasyTCP.EasyTCP
	{
		public MyEasyTCP(int WriteBufferLength) : base(WriteBufferLength)
		{
			DefinePacketIO<Vector>(new VectorIO());

			DefinePacket<string>(0); // Name
			DefinePacket<string>(1); // Message
			DefinePacket<IReadOnlyCollection<int>>(2); // Random Numbers
			DefinePacket<Vector>(3);
			DefinePacket<Person>(4);
			DefinePacket<ByteArray>(5); // Picture
			DefinePacket<byte[]>(6); // Picture - Using an array of bytes
		}
	}
}
