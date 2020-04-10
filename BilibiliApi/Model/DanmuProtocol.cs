using System;
using System.Net;

namespace BilibiliApi.Model
{
	public class DanmuProtocol
	{
		public int PacketLength;
		public short HeaderLength;
		public short Version;
		public int Operation;
		public int SeqId;

		public DanmuProtocol(byte[] buff)
		{
			PacketLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buff, 0));
			HeaderLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buff, 4));
			Version = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buff, 6));
			Operation = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buff, 8));
			SeqId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buff, 12));
		}
	}
}
