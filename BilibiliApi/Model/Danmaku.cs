using System.Text.Json;

namespace BilibiliApi.Model
{
	public class Danmaku
	{
		/// <summary>
		/// 消息类型
		/// </summary>
		public MsgTypeEnum MsgType { get; set; }

		/// <summary>
		/// 弹幕内容
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.Comment"/></item>
		/// </list></para>
		/// </summary>
		public string CommentText { get; set; }

		/// <summary>
		/// 消息触发者用户名
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.Comment"/></item>
		/// <item><see cref="MsgTypeEnum.GiftSend"/></item>
		/// <item><see cref="MsgTypeEnum.Welcome"/></item>
		/// <item><see cref="MsgTypeEnum.WelcomeGuard"/></item>
		/// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
		/// </list></para>
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// 消息触发者用户ID
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.Comment"/></item>
		/// <item><see cref="MsgTypeEnum.GiftSend"/></item>
		/// <item><see cref="MsgTypeEnum.Welcome"/></item>
		/// <item><see cref="MsgTypeEnum.WelcomeGuard"/></item>
		/// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
		/// </list></para>
		/// </summary>
		public int UserID { get; set; }

		/// <summary>
		/// 用户舰队等级
		/// <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.Comment"/></item>
		/// <item><see cref="MsgTypeEnum.WelcomeGuard"/></item>
		/// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
		/// </list></para>
		/// </summary>
		public int UserGuardLevel { get; set; }

		/// <summary>
		/// 礼物名称
		/// </summary>
		public string GiftName { get; set; }

		/// <summary>
		/// 礼物数量
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.GiftSend"/></item>
		/// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
		/// </list></para>
		/// <para>此字段也用于标识上船 <see cref="MsgTypeEnum.GuardBuy"/> 的数量（月数）</para>
		/// </summary>
		public int GiftCount { get; set; }

		/// <summary>
		/// 该用户是否为房管（包括主播）
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.Comment"/></item>
		/// <item><see cref="MsgTypeEnum.GiftSend"/></item>
		/// </list></para>
		/// </summary>
		public bool IsAdmin { get; set; }

		/// <summary>
		/// 是否VIP用戶(老爷)
		/// <para>此项有值的消息类型：<list type="bullet">
		/// <item><see cref="MsgTypeEnum.Comment"/></item>
		/// <item><see cref="MsgTypeEnum.Welcome"/></item>
		/// </list></para>
		/// </summary>
		public bool IsVIP { get; set; }

		/// <summary>
		/// <see cref="MsgTypeEnum.LiveStart"/>,<see cref="MsgTypeEnum.LiveEnd"/> 事件对应的房间号
		/// </summary>
		public string RoomID { get; set; }

		/// <summary>
		/// 原始数据, 高级开发用
		/// </summary>
		public string RawData { get; set; }

		/// <summary>
		/// 内部用, JSON数据版本号 通常应该是2
		/// </summary>
		public int JSON_Version { get; set; }

		public Danmaku()
		{ }

		public Danmaku(string JSON)
		{
			// TODO: 检查验证
			RawData = JSON;
			JSON_Version = 2;
			using var document = JsonDocument.Parse(JSON);
			var obj = document.RootElement;
			var cmd = obj.GetProperty(@"cmd").GetString();
			switch (cmd)
			{
				case "LIVE":
					MsgType = MsgTypeEnum.LiveStart;
					RoomID = obj.GetProperty(@"roomid").GetString();
					break;
				case "PREPARING":
					MsgType = MsgTypeEnum.LiveEnd;
					RoomID = obj.GetProperty(@"roomid").GetString();
					break;
				case "DANMU_MSG":
					MsgType = MsgTypeEnum.Comment;
					CommentText = obj.GetProperty(@"info")[1].GetString();
					UserID = obj.GetProperty(@"info")[2][0].GetInt32();
					UserName = obj.GetProperty(@"info")[2][1].GetString();
					IsAdmin = obj.GetProperty(@"info")[2][2].GetString() == @"1";
					IsVIP = obj.GetProperty(@"info")[2][3].GetString() == @"1";
					UserGuardLevel = obj.GetProperty(@"info")[7].GetInt32();
					break;
				case "SEND_GIFT":
					MsgType = MsgTypeEnum.GiftSend;
					GiftName = obj.GetProperty(@"data").GetProperty(@"giftName").GetString();
					UserName = obj.GetProperty(@"data").GetProperty(@"uname").GetString();
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					GiftCount = obj.GetProperty(@"data").GetProperty(@"num").GetInt32();
					break;
				case "WELCOME":
				{
					MsgType = MsgTypeEnum.Welcome;
					UserName = obj.GetProperty(@"data").GetProperty(@"uname").GetString();
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					IsVIP = true;
					if (obj.TryGetProperty(@"data", out var data))
					{
						if (data.TryGetProperty(@"is_admin", out var isAdmin))
						{
							switch (isAdmin.ValueKind)
							{
								case JsonValueKind.String:
									IsAdmin = isAdmin.GetString() == @"1";
									break;
								case JsonValueKind.True:
									IsAdmin = true;
									break;
								case JsonValueKind.False:
									IsAdmin = false;
									break;
							}
						}
					}
					break;

				}
				case "WELCOME_GUARD":
				{
					MsgType = MsgTypeEnum.WelcomeGuard;
					UserName = obj.GetProperty(@"data").GetProperty(@"username").GetString();
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					UserGuardLevel = obj.GetProperty(@"data").GetProperty(@"guard_level").GetInt32();
					break;
				}
				case "GUARD_BUY":
				{
					MsgType = MsgTypeEnum.GuardBuy;
					UserID = obj.GetProperty(@"data").GetProperty(@"uid").GetInt32();
					UserName = obj.GetProperty(@"data").GetProperty(@"username").GetString();
					UserGuardLevel = obj.GetProperty(@"data").GetProperty(@"guard_level").GetInt32();
					GiftName = UserGuardLevel == 3 ? "舰长" : UserGuardLevel == 2 ? "提督" : UserGuardLevel == 1 ? "总督" : "";
					GiftCount = obj.GetProperty(@"data").GetProperty(@"num").GetInt32();
					break;
				}
				default:
				{
					MsgType = MsgTypeEnum.Unknown;
					break;
				}
			}
		}
	}
}
