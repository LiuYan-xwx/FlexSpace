# 弹性空白 · FlexSpace

在 ClassIsland 主界面的一行中添加「弹性空白」，就能把剩余空间留在指定组件之间。例如，按「时间 → 弹性空白 → 课表」排列，可以把时间和课表推向两侧。

## 使用方法

升级时，请先禁用或卸载改名前的插件，再安装新包。新插件的 ID 为 `liuyanxwx.flexspace`，沿用原有组件 GUID，已有主界面布局可以继续使用。

安装插件后，在主界面编辑模式下，把「弹性空白」直接添加到要撑开的那一行，再在「组件设置」中调整以下选项。

- **权重**默认是 `1`，可以用小数。同一行有多个留白时，按权重分配剩余宽度。两个留白的权重为 `1` 和 `2` 时，分别占剩余宽度的三分之一和三分之二。
- **目标行宽**默认是 `0`，会跟随 ClassIsland 在当前屏幕上的可用宽度。填写正整数可指定行宽，单位是 ClassIsland 主界面缩放前的逻辑像素。同一行填写了多个固定值时，采用排列顺序中第一个可见且填写了固定值的留白组件。
- **强制透明**默认开启，留白正常显示时完全透明。关闭后，可以在「高级设置」里调整该组件的不透明度。每块留白都可以单独设置。

空间不足时，留白会收缩到零。主界面编辑时，每块留白显示为固定宽度的占位块，方便选中和拖动。

## 实现原理

### 沿视觉树找行容器

ClassIsland 的主界面行使用横向 `StackPanel`。它按子项测量出来的宽度依次排列，空白组件要撑开一行，得有地方给它分配一个实际的宽度。

这里用了点「黑魔法」。`FlexSpaceComponent` 加载后，沿着 Avalonia 视觉树向上找宿主给它包的 `ListBoxItem`，再找到该列表项所在的 `StackPanel`。插件通过 `ComponentPresenter` 的根组件标记、`ListBox.Tag` 上的 `MainWindowLineSettings` 和面板的 `components-panel` 样式类识别主界面行。同一面板里的留白共用一个 `FlexSpaceRow`，由它订阅该行的 `LayoutUpdated`。

### 计算一行还有多少空间

`FlexSpaceRow` 从同一条祖先链上找三个 ClassIsland 模板节点。

- `WorkingRoot.Width` 提供 ClassIsland 已算好的主界面可用逻辑宽度。
- `RootLayoutTransformControl.LayoutTransform` 提供 ClassIsland 自己的横向缩放倍率。自动行宽用 `WorkingRoot.Width` 除以该倍率，换算到组件行的坐标系。
- `GridContentRoot.Margin` 提供行内容区左右两边的留白。

布局更新后，插件读取同一行普通组件外层子项的 `DesiredSize.Width`，加上行边距与面板间距，再计算剩余空间。按规则隐藏的弹性空白会缩到零，其他可见留白重新分配空间。

```text
剩余宽度 = max(0, 目标行宽 - 普通组件总宽度 - 左右边距 - 子项间距)
单块留白宽度 = 剩余宽度 × 自身权重 ÷ 同行可见留白的总权重
```

例如，主界面可用宽度为 `1500`，应用缩放为 `125%`，自动目标行宽就是 `1200`。普通组件占 `300`，左右边距和间距共 `24`，剩下 `876`。权重为 `1:2` 的两块留白分别分到 `292` 和 `584`。Avalonia 开启布局取整时，插件按 DPI 对累计分配额取整，避免每块单独进位后把整行撑宽。

### 把结果写回外层列表项

真正撑开这一行的是对 `ListBoxItem.Width` 的临时覆盖。插件用 Avalonia 的 `SetValue` 和 `BindingPriority.Animation` 强设外层列表项的宽度，让分配值包含组件展示器本身的边距。行面板在下一轮布局里按这个宽度摆放后续组件。宽度变化时才重新写值，以减少重复布局。

宽度为零时，会临时启用该列表项的 `ClipToBounds`，挡住可能伸出边界的主题背景。组件脱离视觉树时，插件撤销属性覆盖并取消订阅组件设置。最后一块留白移除后，`FlexSpaceRow` 再解除对面板的订阅。

编辑模式使用 `96` 个逻辑像素宽的占位块。退出编辑后，行宽重新按当前布局计算。

开启「强制透明」时，插件会在正常显示期间把 `ListBoxItem.Opacity` 临时设为 `0`，使分离岛主题绘制的背景也完全透明。关闭开关或进入编辑模式时，插件撤销覆盖，组件继续使用 ClassIsland 高级设置中保存的不透明度。

### 对 ClassIsland 模板的依赖

这套实现依赖 ClassIsland 主界面的视觉树层级、节点名称和横向 `StackPanel` 的测量行为。组件只能在主界面行里直接使用。找不到所需的宿主节点时，插件会跳过该行的宽度分配。ClassIsland 或主题如果改了这些模板，插件可能需要跟着适配。

## 构建

项目面向 `.NET 8`，使用 `ClassIsland.PluginSdk 2.0.1.1`。在项目根目录运行以下命令，生成的安装包位于 `FlexSpace/cipx/FlexSpace.cipx`。SDK 的打包步骤通过 `pwsh` 生成校验摘要。

```powershell
dotnet build FlexSpace/FlexSpace.csproj -c Release
```

[原始需求 ClassIsland #1936](https://github.com/ClassIsland/ClassIsland/issues/1936)
