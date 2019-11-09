namespace BilibiliApi.Model
{
	public enum MsgTypeEnum
	{
		/// <summary>
		/// 弹幕
		/// </summary>
		Comment,

		/// <summary>
		/// 礼物
		/// </summary>
		GiftSend,

		/// <summary>
		/// 欢迎老爷
		/// </summary>
		Welcome,

		/// <summary>
		/// 直播开始
		/// </summary>
		LiveStart,

		/// <summary>
		/// 直播結束
		/// </summary>
		LiveEnd,
		/// <summary>
		/// 其他
		/// </summary>
		Unknown,
		/// <summary>
		/// 欢迎船员
		/// </summary>
		WelcomeGuard,
		/// <summary>
		/// 购买船票（上船）
		/// </summary>
		GuardBuy
	}
}