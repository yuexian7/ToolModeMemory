using System;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 「这件资产 / 这个功能自己允不允许这个值」的纯判定（不碰游戏类型，离线可断言）。
	///
	/// 需求来源（owner，v0.6.0）：有些工具项在个别资产上是被限制的 —— 例如水管的高度
	/// 最高只能到 -10m。就算「高度」设成全局共用、别的资产是 0m，点到水管上也不可能变成 0m。
	/// owner 要求由代码**自动判断**并把限制写进规则，因为自定义资产也可能自带限制，
	/// 模组不可能一个个去适配。
	///
	/// 自动判断的依据全部来自资产自己的数据（反编译 Game.dll）：
	///   高度 = <c>Game.Prefabs.PlaceableNetData.m_ElevationRange</c>（Colossal.Mathematics.Bounds1，
	///          min/max 都是 float）—— 原版 <c>NetToolSystem.CheckElevationRange</c> 夹的就是它
	///          （decompiled Game.Tools/NetToolSystem.cs L6023-6028）；
	///   工具模式 / 对齐这些「可选集合」项，原版自己已经在按资产派生了：
	///          <c>GetUIModes</c>（L5402，按 upgradeOnly / allowGrid / allowReplace 生成可选项列表）
	///          与 <c>actualMode</c>（L5120，不被当前资产支持的 mode 一律按 Straight 执行）、
	///          <c>GetAvailableSnapMask</c> + <c>GetActualSnap</c>（对齐掩码）。
	/// 所以本类只负责把「范围」这一类限制统一成一处规则：超出范围的值既不写回这件资产，
	/// 也不记进记忆（记下来的话，它要么污染共用桶，要么变成这件资产永远用不了的一份值）。
	/// </summary>
	public struct ValueLimit
	{
		/// <summary>读到了资产自己声明的范围。false = 这一项对这个资产没有范围限制。</summary>
		public readonly bool Known;

		/// <summary>允许的最小值（含）。</summary>
		public readonly float Min;

		/// <summary>允许的最大值（含）。</summary>
		public readonly float Max;

		/// <summary>
		/// 判范围用的宽容度（米）。记忆把浮点按 ×100 存成整数（1cm），来回换算会有
		/// 亚厘米级误差；取 5cm 既能吃掉这个误差，又远小于任何实际游戏里的限制粒度
		/// （原版自己用的是 elevationStep 的一半，那太宽：默认步长 10m 会放行 5m 的超限值）。
		/// </summary>
		public const float kTolerance = 0.05f;

		public ValueLimit(bool known, float min, float max)
		{
			Known = known;
			Min = min;
			Max = max;
		}

		/// <summary>没有可读的限制（不限制任何值）。</summary>
		public static ValueLimit None { get { return new ValueLimit(false, 0f, 0f); } }

		/// <summary>min &gt; max 是残缺数据（资产没填 / 读坏），一律当「不限制」，绝不因此拒绝写回。</summary>
		public static ValueLimit FromRange(float min, float max)
		{
			if (float.IsNaN(min) || float.IsNaN(max) || float.IsInfinity(min) || float.IsInfinity(max))
			{
				return None;
			}
			if (min > max) return None;
			return new ValueLimit(true, min, max);
		}

		/// <summary>这个值能不能用在这件资产上。没有限制时永远 true。</summary>
		public bool Accepts(float value)
		{
			if (!Known) return true;
			if (float.IsNaN(value)) return false;
			return value >= Min - kTolerance && value <= Max + kTolerance;
		}

		/// <summary>
		/// owner 的完整规则：桶里的共用值超出本资产的限制 → 本资产不共用它；
		/// 值回到范围内（例如别的资产是 -20m，水管上限 -10m）→ 照旧共用。
		/// 这一条判的是「写回」；<see cref="ShouldSkipCapture"/> 判的是配套的「别把别人的值记到我头上」。
		/// </summary>
		public bool Blocks(float sharedValue)
		{
			return Known && !Accepts(sharedValue);
		}

		/// <summary>
		/// 捕获前的一问：这个值本资产自己够得着吗？
		/// 够不着（例如从道路切到水管，工具里还留着道路的 0m）就说明它是**上一件资产留下的**，
		/// 不是本资产的选择，记进任何桶（共用桶或本资产自己那份）都是错的：
		/// 记进共用桶会把别的资产拽过来，记进本资产那份会留下一个它永远用不了的值，
		/// 而且下一次捕获还会拿这个坏值把玩家真正选的数值挡掉。
		/// 原版不会出这个问题，因为它在切资产时就按新资产的范围夹过一次（CheckElevationRange）。
		/// 反过来，玩家在**这件资产上**主动改出来的值（水管改到 -30m）一律照常记 ——
		/// 共用范围下它本来就该带着整组一起变，这正是「共用」的定义，也与原版一致。
		/// </summary>
		public bool ShouldSkipCapture(float value)
		{
			return Known && !Accepts(value);
		}
	}
}
