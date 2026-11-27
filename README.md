<div align="center">

<img src="https://raw.githubusercontent.com/LiuYan-xwx/FlexSpace/refs/heads/master/FlexSpace/icon.png" alt="Plugin Icon" width="100" height="100">

# FlexSpace


![GitHub License](https://img.shields.io/github/license/LiuYan-xwx/FlexSpace)
![GitHub top language](https://img.shields.io/github/languages/top/LiuYan-xwx/FlexSpace)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/LiuYan-xwx/FlexSpace/total?label=%E6%80%BB%E4%B8%8B%E8%BD%BD%E9%87%8F)
![GitHub Repo stars](https://img.shields.io/github/stars/LiuYan-xwx/FlexSpace)

</div>

一个用于填充主界面剩余空间的 [ClassIsland](https://github.com/ClassIsland/ClassIsland) 插件，提供了一个 `弹性空白` 组件。

把它塞进主界面的一行里，就能把其他组件往两边撑开（比如 `时间` → `弹性空白` → `课表`），或者按比例在组件之间留出空白。

## 功能

- 提供 `弹性空白` 组件，自动填充同一行的剩余宽度
- 同一行放多个留白时，支持按权重分配剩余空间
- 支持指定目标行宽，设为 `0` 时自动跟随主界面可用宽度
- 支持「强制透明」，正常显示时完全透明（关掉后可走组件高级设置里的不透明度）
- 空间不够时会自动缩成 `0`；编辑模式下会显示为固定宽度的占位块，方便选中和拖动

## 安装

- 在插件市场搜索 `弹性空白` 或 `FlexSpace` 安装。
- 或者前往 [Releases](https://github.com/LiuYan-xwx/FlexSpace/releases) 下载 `.cipx` 文件手动安装。

## 使用方法

在主界面编辑模式下，把 `弹性空白` 添加到想要撑开的那一行，然后在组件设置里按需调整即可：

1. **权重**：默认 `1`（支持小数）。同一行有多个留白时按各自权重分剩余宽度，比如 `1` 和 `2` 就是按 `1:2` 分。
2. **目标行宽**：默认 `0`，会自动跟随主界面在当前屏幕上的可用宽度；填写正整数可手动指定行宽（单位为主界面缩放前的逻辑像素）。同一行有多个组件都填了固定值时，以最左侧可见组件的设置为准。
3. **强制透明**：默认开启，正常显示时完全透明（分离岛主题绘制的背景也会一起隐藏）。关闭后，使用组件高级设置中的不透明度。

## 注意事项

- 仅支持 ClassIsland 2.0+，且只能在主界面行里直接使用。
- 插件是通过沿 Avalonia 视觉树查找主界面行容器和布局节点、再直接覆盖外层 `ListBoxItem` 宽度来实现的。如果后续 ClassIsland 或部分主题修改了主界面模板结构，可能会出现失效的情况。

## 开发与构建

项目基于 `.NET 8` 和 `ClassIsland.PluginSdk`（`2.0.1.1`）。

在仓库根目录运行以下命令即可执行 Release 构建并生成 `.cipx` 插件包（输出在 `FlexSpace/cipx/FlexSpace.cipx`）：

```powershell
dotnet build FlexSpace/FlexSpace.csproj -c Release
```

## 反馈与相关

- 原始需求：[ClassIsland#1936](https://github.com/ClassIsland/ClassIsland/issues/1936)
- 遇到问题或有建议欢迎提 [Issue](https://github.com/LiuYan-xwx/FlexSpace/issues)。
