using BilibiliApi.Enum;
using System.Diagnostics;
using System.Text.Json;

namespace BilibiliApi.Model
{
	public class Danmaku
	{
		/// <summary>
		/// 消息类型
		/// </summary>
		public MsgType MsgType = MsgType.Unknown;

		/// <summary>
		/// 弹幕内容
		/// </summary>
		public string CommentText;

		/// <summary>
		/// 消息触发者用户名
		/// </summary>
		public string UserName;

		/// <summary>
		/// 消息触发者用户ID
		/// </summary>
		public int UserID;

		/// <summary>
		/// 用户舰队等级
		/// <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
		/// </summary>
		public int UserGuardLevel;

		/// <summary>
		/// 礼物名称
		/// </summary>
		public string GiftName;

		/// <summary>
		/// 礼物数量
		/// </summary>
		public int GiftCount;

		/// <summary>
		/// 该用户是否为房管（包括主播）
		/// </summary>
		public bool IsAdmin;

		/// <summary>
		/// 是否VIP用戶(老爷)
		/// </summary>
		public bool IsVIP;

		/// <summary>
		/// 事件对应的房间号
		/// </summary>
		public string RoomID;

		/// <summary>
		/// 原始数据, 高级开发用
		/// </summary>
		public string RawData;

		/// <summary>
		/// 内部用, JSON数据版本号 通常应该是2
		/// </summary>
		public int JsonVersion;

		public Danmaku(string json)
		{
			RawData = json;
			JsonVersion = 2;

			Debug.WriteLine(RawData);

			using var document = JsonDocument.Parse(json);
			var obj = document.RootElement;

			if (!obj.TryGetProperty(@"cmd", out var cmdJson))
			{
				return;
			}
			var cmd = cmdJson.GetString();
			switch (cmd)
			{
				case @"LIVE":
					MsgType = MsgType.LiveStart;
					RoomID = obj.GetProperty(@"roomid").GetRawText();
					break;
				case @"PREPARING":
					MsgType = MsgType.LiveEnd;
					RoomID = obj.GetProperty(@"roomid").GetRawText();
					break;
				case @"DANMU_MSG":
					MsgType = MsgType.Comment;
					CommentText = obj.GetProperty(@"info")[1].GetString();
					UserID = obj.GetProperty(@"info")[2][0].GetInt32();
					UserName = obj.GetProperty(@"info")[2][1].GetString();
					IsAdmin = obj.GetProperty(@"info")[2][2].GetRawText() == @"1";
					IsVIP = obj.GetProperty(@"info")[2][3].GetRawText() == @"1";
					UserGuardLevel = obj.GetProperty(@"info")[7].GetInt32();
					break;
				case @"SEND_GIFT":
					MsgType = MsgType.GiftSend;
					GiftName = obj.GetProperty(@"data").GetProperty(@"giftName").GetString();
					UserName = obj.GetProperty(@"data").GetProperty(@"uname").GetString();
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					GiftCount = obj.GetProperty(@"data").GetProperty(@"num").GetInt32();
					break;
				case @"WELCOME":
				{
					MsgType = MsgType.Welcome;
					UserName = obj.GetProperty(@"data").GetProperty(@"uname").GetString();
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					IsVIP = true;
					if (obj.TryGetProperty(@"data", out var data))
					{
						if (data.TryGetProperty(@"is_admin", out var isAdmin))
						{
							IsAdmin = isAdmin.GetBoolean();
						}
						else if (data.TryGetProperty(@"isadmin", out var isadmin))
						{
							IsAdmin = isadmin.GetRawText() == @"1";
						}
					}
					break;

				}
				case @"WELCOME_GUARD":
				{
					MsgType = MsgType.WelcomeGuard;
					UserName = obj.GetProperty(@"data").GetProperty(@"username").GetString();
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					UserGuardLevel = obj.GetProperty(@"data").GetProperty(@"guard_level").GetInt32();
					break;
				}
				case @"GUARD_BUY":
				{
					MsgType = MsgType.GuardBuy;
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					UserName = obj.GetProperty(@"data").GetProperty(@"username").GetString();
					UserGuardLevel = obj.GetProperty(@"data").GetProperty(@"guard_level").GetInt32();
					GiftName = UserGuardLevel switch
					{
						3 => @"舰长",
						2 => @"提督",
						1 => @"总督",
						_ => string.Empty
					};
					GiftCount = obj.GetProperty(@"data").GetProperty(@"num").GetInt32();
					break;
				}
				default:
				{
					MsgType = MsgType.Unknown;
					break;
				}
			}
		}
	}
}
