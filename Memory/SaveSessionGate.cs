using System;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 「本局现在能不能动记忆」的三条闸门，纯逻辑（时钟由调用方传进来，离线可断言）。
	///
	/// 为什么需要它 —— 0.4.0 玩家反馈的两条症状其实都出在进档那一刻的时序上：
	///
	/// 1) **不在存档里就不许捕获**。捕获只看总开关，于是回主菜单 / 开机进主菜单时
	///    <c>OnLeavingGame()</c> 照样跑了一遍捕获，把主菜单工具面板的状态写进了
	///    <c>_unsaved_xxxx.json</c>（2026-10-07 玩家日志里就有这条：16:41:46「Memory saved:
	///    ..._unsaved_9d40cc53.json」）。更危险的是「进档回调半路失败、名字还停在上一个存档」时，
	///    这份主菜单状态会被当成那个存档的记忆写下去。
	///
	/// 2) **进档后有一小段稳定期**。原版在 <c>onGamePreload</c> 里 <c>ResetToolPreferences()</c>
	///    （NetToolSystem.cs:5686），载入完才轮到我们写回；但进档时 <c>ToolSystem.activeTool</c>
	///    被 <c>ToolSystem.PreDeserialize</c> 置回 DefaultToolSystem（ToolSystem.cs:613），
	///    工具面板真正成型还要再晚几帧。稳定期里每帧写回、并且**先不捕获**，
	///    否则会把原版刚重置出来的默认值当成玩家的选择记进记忆文件 ——
	///    那才是「下一次进档全是出厂值」的永久化路径：这次记错，下次读到错。
	///
	/// 3) 捕获与稳定期都以「在存档里」为前提；<see cref="Close"/> 之后一律不许写记忆。
	/// </summary>
	public sealed class SaveSessionGate
	{
		/// <summary>
		/// 进档稳定期（秒）。够覆盖「载入完成 → 工具成型 → UI 第一次重画」这一段，
		/// 又短到玩家不会在这期间已经改完一项设置（改了也会被写回，下一段正常记录）。
		/// </summary>
		public const float kSettleSeconds = 1.0f;

		private bool m_InSave;
		private float m_SettleUntil = float.MinValue;

		/// <summary>当前是否真的在存档里（编辑器 / 主菜单不算）。</summary>
		public bool InSave { get { return m_InSave; } }

		/// <summary>打开这一局：进入稳定期。<paramref name="now"/> 用与 <see cref="AllowCapture"/> 同一只时钟。</summary>
		public void Open(float now)
		{
			Open(now, kSettleSeconds);
		}

		public void Open(float now, float settleSeconds)
		{
			m_InSave = true;
			m_SettleUntil = settleSeconds <= 0f ? float.MinValue : now + settleSeconds;
		}

		/// <summary>离开这一局：稳定期与闸门一起作废，下次进档重新计时。</summary>
		public void Close()
		{
			m_InSave = false;
			m_SettleUntil = float.MinValue;
		}

		/// <summary>还在稳定期里吗（此期间每帧写回、不捕获）。</summary>
		public bool Settling(float now)
		{
			return m_InSave && now < m_SettleUntil;
		}

		/// <summary>可以正常捕获吗：在存档里且稳定期已过。</summary>
		public bool AllowCapture(float now)
		{
			return m_InSave && now >= m_SettleUntil;
		}

		/// <summary>
		/// 离开存档前那一次强制捕获：稳定期也要记（玩家可能是进档后一秒内就退出的，
		/// 此刻面板上的值就是他看到的值），但不在存档里时仍然不记。
		/// </summary>
		public bool AllowFinalCapture()
		{
			return m_InSave;
		}
	}
}
