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
			PacketLength = buff.ToInt32(0);
			HeaderLength = buff.ToInt16(4);
			Version = buff.ToInt16(6);
			Operation = buff.ToInt32(8);
			SeqId = buff.ToInt32(12);
		}
	}
}
