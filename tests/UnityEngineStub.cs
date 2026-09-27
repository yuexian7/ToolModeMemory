// 让 MemoryStore 能脱离游戏程序集编译：只提供它唯一用到的 UnityEngine API。
// 故意在测试目录未设置时抛异常，避免离线测试误写到真实的
// %USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData。
namespace UnityEngine
{
	public static class Application
	{
		/// <summary>测试重定向目录（由测试用例赋值）。</summary>
		public static string TestDir;

		public static string persistentDataPath
		{
			get
			{
				if (string.IsNullOrEmpty(TestDir))
				{
					throw new System.InvalidOperationException(
						"StoreHarness: UnityEngine.Application.TestDir 未设置，拒绝访问真实用户目录");
				}
				return TestDir;
			}
		}
	}
}
