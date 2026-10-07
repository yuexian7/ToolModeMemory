# Tool Mode Memory

记住工具面板设置，并按存档持久化。总开关默认打开；关掉后模组完全不参与 = 原版行为。

## 功能

- 每个存档一份记忆（`ModsData\ToolModeMemory\<存档名>.json`，文件名 = 「载入游戏」里看到的存档名；
  打开自动存档时记忆仍写进它**原存档名**的文件并覆盖它）
- 退出覆盖写入，下次进入自动恢复
- 「设置共用范围」：同组 / 同菜单 / 同类资产 / 全局共用 / 全局不共用
- 尊重资产自己的取值限制：共用的数值超出该资产允许的范围时，那件资产不被改写，共用数值也不被它反向污染
- 「记忆管理」：重置记忆、打开记忆文件夹
- 12 种官方语言
- 零 Harmony，不修改游戏文件

## 本地构建

```powershell
dotnet build -c Release
```

部署到游戏 Mods 目录由官方工具链 `DeployWIP` 完成。  
离线回归测试（不需要游戏，也不写真机目录）：

```powershell
dotnet run -c Release --project tests\StoreHarness
```

## 共用范围

游戏工具栏只有三层：**菜单 → 分类 → 资产**。

| 档位 | 含义 |
|---|---|
| 同组 | 同一菜单下的同一分类（= 原版道路工具的粒度） |
| 同菜单 | 整个菜单共用（道路、教育…） |
| 同类资产 | **不**按工具栏分类，而按资产自己服务的对象归群（`AssetClass.cs`）：小巷 / 各车道数的道路 / 高架 / 隧道同为车行，小型停车场与大型停车场同为一类，步行道、地铁轨道、火车轨道、电缆各算一类，小学与中学同为教育服务；**桥梁、埠头、交叉路口各自单独成类**（与道路不同类）；判不出用途就退回按单个资产记 |
| 全局共用 | 所有资产共用一份 |
| 全局不共用 | 每个资产各记一份 |

「同组」「同菜单」**读工具栏的实时层级**（`UIObjectData.m_Group` → `UIAssetCategoryData.m_Menu`）：
Asset UI Manager 这类模组重排过分组，本模组就跟着重排后的分组共用，不再提供「按原版分组」的开关
（v0.6.0 起取消）。做不了回退的原因很直接 —— 工具栏数据只有这一份，被改写之后原地不存在
「原版那份」，冻结第一次解析到的位置也换不回原版分组。
「同类资产」不看工具栏摆它在哪，只读资产自己的组件（`RoadData` / `TrackData` / `PathwayData` /
`WaterwayData` / `NetData.m_*Layers` / `UtilityObjectData` / 车道实体的 `*LaneData` /
`ServiceObjectData` → `ServiceData`），所以它本来就与其它模组的改排无关。桥梁、埠头、交叉路口、停车场四类看的是用途标记
（`BridgeData`、`PlaceableNetData.m_PlacementFlags & ShoreLine`、`AssetStampData` + `SubNet`、
`ParkingFacilityData`），判据与原版资产编辑器的分类查询逐字一致
（`EditorAssetCategorySystem.GenerateRoadCategory` / `GenerateBridgeCategory`），
所以我们在编辑器里看到的那套「道路 / 桥梁 / 交叉路口」划分就是本模组的划分。
唯一的例外是地区主题 / 数据包两行：它们的值是筛选面板自己的状态，
仍按（菜单+分类）记录，否则原版换分类时的清空会被误当成玩家取消勾选。

键一律用资产/分类的稳定名字，不用实体索引，所以加装或移除 DLC、创意工坊后不会串记忆。

## 资产自带的取值限制（v0.6.0）

「全局共用」和资产自身的限制会打架：地下管线这类资产只能在某个高度以下（水管最高只到地面以下），
而共用的记忆值可能是 0m。规则由 `Memory/ValueLimit.cs` 统一成一处，两侧各一条：

- **写回**（`ToolMemoryBridge.ApplyElevationField`）：共用值超出这件资产的范围就一个字节都不写。
  面板绑的正是 `tool.elevation`（`Game.UI.InGame/ToolUISystem.cs:149`），写进去就是把面板设成
  这件资产做不到的 0m；原版会在每帧的 `InitializeRaycast` 里按新资产的范围夹一次
  （`NetToolSystem.CheckElevationRange` L6023-6030、`InitializeRaycast` L5928-5937），
  我们不写 = 面板直接停在它自己允许的那个值上，不用等一帧去纠正一个假值。
- **捕获**（`ToolMemorySystem.CaptureNow`）：判的是**这个值本身**够不够得着。
  从道路切到水管时工具里留着的是道路的 0m，那不属于水管，记下去要么污染共用桶（把道路一起拽走），
  要么变成这件资产永远用不了的死值，还会挡掉玩家后面真正选的数值。
  玩家在**这件资产上**主动改出来的合法值（水管改到 -30m）一律照常记 —— 共用范围带着整组一起变，
  这正是「共用」的定义，也与原版共用同一个 `m_DesiredElevation` 的行为一致。

限制一律**自动读资产自己的数据**，不写死任何资产名：
高度取 `Game.Prefabs.PlaceableNetData.m_ElevationRange`（`Bounds1`），按 prefab 缓存（`s_LimitCache`，
`ForgetNames()` 时清空）。读不到 / 数据残缺（min>max、NaN、无穷）一律当「不限制」，
绝不让「读不到限制」退化成「什么都不写」。判定带 5cm 宽容度（`ValueLimit.kTolerance`），
只为吃掉浮点与 1cm 存储的往返误差，远小于任何可用的高度档位。

工具模式、对齐、配色这类「可选集合」项不需要这套规则：原版本来就按资产派生可选项，
我们写回时走的正是它自己的判据（`GetUIModes` L5402 / `actualMode` L5120 /
`GetAvailableSnapMask` + `GetActualSnap`，配色看 `m_HasPlacementColor`）。

## 进档与退档的流程纪律（v0.5.0）

原版把 `onGameLoadingComplete` 一次性交给我们，进档要做完四件事：认存档名、读记忆、
把系统启用状态对齐、写回面板。v0.4.0 把这四件事写在同一个 `try` 里，
任何一步抛出异常，后面三步全部没跑 —— 玩家看到的就是「模组开着，进档却什么都不生效；
把开关拨一遍又好了」。现在（`ToolModeMemoryMod.EnterSave`）：

- 每一步各自 `RunStep()`，失败只记本步的日志，后续步骤照跑；
- 认名失败时必须落到**本局专属**的临时名（`StartUnnamedSession`），
  绝不沿用上一个存档的文件名 —— game→game 直接切档不经过主菜单；
- 所有 `catch` 一律经 `Fail()` 输出**堆栈前 5 帧**，不再只留一个异常类名。

捕获闸门（`Memory/SaveSessionGate.cs`，纯逻辑、离线可断言）：

- 不在存档里不捕获、不落盘：主菜单 / 编辑器里 `OnLeavingGame()` 曾经照样跑捕获，
  并在记忆文件夹里留下对不上号的 `_unsaved_xxxx.json`；
- 进档后 1 秒是稳定期：**每帧写回、不捕获**。原版在 `onGamePreload` 里
  `ResetToolPreferences()`，工具成型又比进档回调晚几帧，
  这段时间一捕获就会把原版默认值记成玩家的选择，下一次进档读到的就是这份被污染的记忆；
- 总开关在存档内被重新打开时同样进入稳定期；
- 退出存档前那一次捕获不受稳定期限制（`AllowFinalCapture`），玩家秒进秒退也不丢记录。

## 开发与验收纪律（每次改动都要过一遍）

**性能**：任何新代码都不许进入每帧路径做可避免的工作。

- 键、名字、分类判定一律走缓存，缓存只在「工具 / 资产真的换了」或隔 2 秒时重算
  （`ToolMemorySystem.RefreshKeyCache`），不允许每帧重算字符串键。
- 需要扫磁盘或资产库的事（例如自动存档回溯原存档名）只在进档回调里做一次，绝不放 `OnUpdate`。
- 反射解析外部模类型的时机不能早于对方加载：懒解析 + 重试 + 次数上限
  （`ToolMemoryBridge.Anarchy()`），解析成功后热路径只剩一次字段读。
- 写盘失败必须退避（10→20→…→300 秒），否则 Dirty 不清会导致**每帧**重试文件 IO。
- **每帧路径不许有可避免的堆分配**：游戏跑 Mono/Boehm（`boot.config` 里的 `gc-max-time-slice`
  就是它的增量回收参数），不分代，每帧扔几百字节会在长时间游玩后换成全堆扫描卡顿。
  值类型「按资产派生」的一律按 prefab 记忆（`s_HasColorCache` 等，`ForgetNames()` 清、`kCacheCap` 封顶），
  不要每帧反射取值（`FieldInfo.GetValue` 对值类型必装箱）。已知保留两处并写清理由：
  `m_DesiredElevation`（要的就是跨资产那个意图值，1 盒/帧）与 Anarchy 的四项（外来类型，免装箱只能降频探测，
  风险不对称）。
- 新增项要先在 `tests/StoreHarness` 里补断言，再谈实机。

**卸载安全**（取消订阅 / 删模组文件夹之后，下次进游戏不许报错，也不许留下副作用）：

- 每个挂上去的东西都要有对应的摘掉动作，并在 `IMod.OnDispose` 里执行：
  `GameManager.onGameLoadingComplete/onGameSaveLoad/onGamePreload`、`Application.quitting`、
  `AppDomain.CurrentDomain.ProcessExit`、原版 `ToolSystem.EventToolChanged/EventPrefabChanged`
  （`ToolMemorySystem.Detach()`，不能只依赖 `OnDestroy`：World 不重建就不会调用）、
  12 个本地化词条源（`RemoveLocaleSources()`，本地化管理器活得比我们久）。
- 静态缓存里存的游戏对象在退出前 `ToolMemoryBridge.ForgetNames()` 清掉；
  静态 `ToolModeMemorySettings.Instance` / `Store` 置 null，让任何迟到调用都自然早退。
- 不往存档里写任何东西，不改游戏文件，不用 Harmony —— 卸载后原版行为自然完整。

## 设置摘要

| 标签页 | 内容 |
|---|---|
| 模组设置 | 总开关（出厂默认打开）+ 12 个工具项 × (是否恢复, 设置共用范围) |
| 关于 | 重置记忆 / 打开记忆文件夹 / 版本、作者、外链 |

可记忆项 = 道路绘制模式、吸附、平行道路、地下/高架、高度，建筑放置模式、对齐吸附、地下放置，
区域绘制、表面区域、水体、推土范围。地形与升级工具没有可记的面板状态（它们的“模式”就是选中的资产本身），
所以不在列表里。
