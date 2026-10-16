using System;
using System.IO;
using System.Text;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// guid → 存档名 索引文件（<c>_index.txt</c>）的读取与写回。
	/// 只有 System.IO，不碰任何游戏类型，所以 tests/StoreHarness 能直接编译它并断言。
	///
	/// 这个类为什么存在（v0.6.2 第三轮审查，实机日志为证）：
	/// CS2 的 <c>Game.Modding.ModManager</c> 造模组实例用的是
	/// <c>FormatterServices.GetUninitializedObject(item)</c>（Game.dll，Load 方法 L120-123）——
	/// **不运行任何构造函数**，所以模组入口类上的实例字段初始化器一行都不会执行。
	/// 0.4.0~0.6.1 期间 <c>ToolModeMemoryMod.m_Index</c> 就靠字段初始化器赋值，于是恒为 null：
	/// 每次进档在 <c>m_Index.Set(...)</c> 抛 NullReferenceException（玩家日志
	/// <c>entry.identify failed: ... ToolModeMemoryMod.cs:380</c>），存盘时同样一条
	/// （<c>save.index failed: ... :608</c>）；Set 先炸 → 写文件那步永远走不到 →
	/// <c>_index.txt</c> 从来没被生成过 → 读档也永远读不到东西，自锁死。
	/// 索引这条通道只是兜底（主认名走存档元数据），所以功能上一直没暴露，但日志里常年挂着两条 WARN。
	///
	/// 结论：**索引对象一定不是 null 这条规则不能写在模组入口的字段声明上**，
	/// 必须写在这里，并且能在不开游戏的情况下被断言住（回归用例见 StoreHarness [26]）。
	/// </summary>
	public static class SaveIndexFile
	{
		/// <summary>新建一份空索引。模组入口必须在 OnLoad 里显式调用它，不能靠字段初始化器。</summary>
		public static SaveIndex New()
		{
			return new SaveIndex();
		}

		/// <summary>
		/// 拿来就用的兜底：null 就当场补一份（幂等，已有条目一条不丢）。
		/// </summary>
		public static SaveIndex Ensure(SaveIndex current)
		{
			return current != null ? current : new SaveIndex();
		}

		/// <summary>
		/// 从磁盘读索引。**永远返回非 null**，调用方因此可以无条件 <c>m_Index = Load(...)</c>。
		/// 三条分支：
		///  - 文件读到了：以磁盘内容为准（<see cref="SaveIndex.Parse"/> 自己会跳过脏行，不抛）；
		///  - 文件不存在：把 <paramref name="current"/> 兜底成可用对象原样返回，一条不清；
		///  - 读盘抛异常（占用 / 没权限 / 路径坏）：同上，保留现有条目，异常类型名经
		///    <paramref name="error"/> 传给调用方写日志——绝不能把一次 IO 抖动变成整局没有索引。
		/// </summary>
		/// <param name="path">索引文件路径；null 当作「没有文件」。</param>
		/// <param name="current">当前索引，允许是 null（正是 CS2 不跑构造函数那种局面）。</param>
		/// <param name="error">null = 没出错；否则 = 异常类型名。</param>
		public static SaveIndex Load(string path, SaveIndex current, out string error)
		{
			error = null;
			SaveIndex fallback = Ensure(current);
			if (string.IsNullOrEmpty(path)) return fallback;
			try
			{
				if (File.Exists(path))
				{
					return SaveIndex.Parse(File.ReadAllText(path, Encoding.UTF8));
				}
			}
			catch (Exception ex)
			{
				error = ex.GetType().Name;
			}
			return fallback;
		}

		/// <summary>
		/// 写索引文件。成功返回 true；任何失败只报告不抛（索引写不下去只是少一条兜底通道，
		/// 不该把进档 / 存盘流程带下水）。<paramref name="index"/> 为 null 时先兜底再写，
		/// 绝不因为 null 抛在外面。
		/// </summary>
		/// <param name="directory">数据目录（不存在就建）。</param>
		/// <param name="path">索引文件路径。</param>
		/// <param name="index">要写的索引，允许 null。</param>
		/// <param name="error">null = 没出错；否则 = 异常类型名。</param>
		public static bool Write(string directory, string path, SaveIndex index, out string error)
		{
			error = null;
			try
			{
				SaveIndex idx = Ensure(index);
				if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
				if (string.IsNullOrEmpty(path))
				{
					error = "EmptyPath";
					return false;
				}
				File.WriteAllText(path, idx.Serialize(), new UTF8Encoding(false));
				return true;
			}
			catch (Exception ex)
			{
				error = ex.GetType().Name;
				return false;
			}
		}
	}
}
