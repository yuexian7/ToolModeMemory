# Tool Mode Memory

记住工具面板设置，并按存档持久化。总开关默认打开；关掉后模组完全不参与 = 原版行为。

## 功能

- 每个存档一份记忆（`ModsData\ToolModeMemory\<存档名>.json`，文件名 = 「载入游戏」里看到的存档名；
  打开自动存档时记忆仍写进它**原存档名**的文件并覆盖它）
- 退出覆盖写入，下次进入自动恢复
- 「设置共用范围」：同组 / 同菜单 / 同类资产 / 全局共用 / 全局不共用
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

「同组」「同菜单」读工具栏的实时层级（`UIObjectData.m_Group` → `UIAssetCategoryData.m_Menu`，
会被 Asset UI Manager 这类模组改写，所以受「是否兼容其它模组」开关影响）；
「同类资产」只读资产自己的组件（`RoadData` / `TrackData` / `PathwayData` / `WaterwayData` /
`NetData.m_*Layers` / `UtilityObjectData` / 车道实体的 `*LaneData` / `ServiceObjectData` → `ServiceData`），
与那个开关无关。桥梁、埠头、交叉路口、停车场四类看的是用途标记
（`BridgeData`、`PlaceableNetData.m_PlacementFlags & ShoreLine`、`AssetStampData` + `SubNet`、
`ParkingFacilityData`），判据与原版资产编辑器的分类查询逐字一致
（`EditorAssetCategorySystem.GenerateRoadCategory` / `GenerateBridgeCategory`），
所以我们在编辑器里看到的那套「道路 / 桥梁 / 交叉路口」划分就是本模组的划分。
唯一的例外是地区主题 / 数据包两行：它们的值是筛选面板自己的状态，
仍按（菜单+分类）记录，否则原版换分类时的清空会被误当成玩家取消勾选。

键一律用资产/分类的稳定名字，不用实体索引，所以加装或移除 DLC、创意工坊后不会串记忆。

## 开发与验收纪律（每次改动都要过一遍）

**性能**：任何新代码都不许进入每帧路径做可避免的工作。

- 键、名字、分类判定一律走缓存，缓存只在「工具 / 资产真的换了」或隔 2 秒时重算
  （`ToolMemorySystem.RefreshKeyCache`），不允许每帧重算字符串键。
- 需要扫磁盘或资产库的事（例如自动存档回溯原存档名）只在进档回调里做一次，绝不放 `OnUpdate`。
- 反射解析外部模类型的时机不能早于对方加载：懒解析 + 重试 + 次数上限
  （`ToolMemoryBridge.Anarchy()`），解析成功后热路径只剩一次字段读。
- 写盘失败必须退避（10→20→…→300 秒），否则 Dirty 不清会导致**每帧**重试文件 IO。
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
| 模组设置 | 总开关（默认关）+ 12 个工具项 × (启用, 范围) |
| 关于 | 重置记忆 / 打开记忆文件夹 / 版本、作者、外链 |

可记忆项 = 道路绘制模式、吸附、平行道路、地下/高架、高度，建筑放置模式、对齐吸附、地下放置，
区域绘制、表面区域、水体、推土范围。地形与升级工具没有可记的面板状态（它们的“模式”就是选中的资产本身），
所以不在列表里。
