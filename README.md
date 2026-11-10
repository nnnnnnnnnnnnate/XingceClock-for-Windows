# 行测计时器 · Windows 版（For Windows）

一个给自己用的行测倒计时工具：清单中每一项 = 名称 + 分钟数，可以单独跑一项，
也可以按清单顺序整串“流水线”走完，每段到点自动循环响铃。

**平台：Windows（WinForms / .NET Framework 4.x）**，只在 Windows 上运行。
Android 版见 [XingceClock-for-Android](https://github.com/nnnnnnnnnnnnate/XingceClock-for-Android)。

## 功能

- 闹钟清单：添加 / 修改 / 删除 / 上移 / 下移
- 双击清单项直接开始单项计时
- **流水线整串走完**：按顺序逐段倒计时，每段结束响铃 5 秒后自动进入下一项
- 计时窗口置顶，切到别的窗口也看得见
- 暂停 / 继续、跳过本段、停止
- 到点循环播放系统闹钟音效，并显示“立即开始下一项 / 知道了”
- 默认清单按行测整卷 120 分钟配置，可随意修改
- 总时长和项数实时统计

## 运行

直接双击 `行测计时器.exe`（Windows 10/11 自带 .NET Framework 4.x，无需安装）。
也可以点 `使用说明.txt` 看详细操作。

清单保存在程序同目录的 `stages.txt`，一行一项，格式为：

```
名称<TAB>分钟数
```

例如：

```
资料分析	30
涂卡检查	5
```

删掉 `stages.txt` 后重新打开会恢复默认清单。该文件属于个人数据，已被 `.gitignore` 忽略。

## 从源码编译

`build.bat` 使用 Windows 自带的 C# 编译器（`csc.exe`），不需要联网或安装 SDK：

```bat
build.bat
```

生成 `XingceTimer.exe`。仓库里的 `行测计时器.exe` 是同一份源码的已编译版本
（功能相同，只是文件名不同），可以直接下载运行。

## 源码结构

| 文件 | 作用 |
| --- | --- |
| `Program.cs` | 入口、DPI 适配、`--shot` 自查截图 |
| `MainForm.cs` | 清单管理主界面 |
| `EditDialog.cs` | 添加/修改闹钟对话框 |
| `RunForm.cs` | 置顶计时窗口、响铃、流水线推进 |
| `Stage.cs` | 阶段模型与 `stages.txt` 读写 |
| `build.bat` | 使用系统自带 `csc.exe` 编译 |
| `使用说明.txt` | 面向使用者的说明 |

## 已知限制

- 仅 Windows（WinForms）。
- 计时基于系统时钟，电脑休眠恢复后按真实经过时间计算。
- 响铃使用系统 `Media` 目录里的闹钟/铃声 WAV；如果系统没有这些文件，会退回系统提示音。
- 没有安装包/自动更新，直接复制 exe 即可使用。

## 许可证

MIT License，见 [LICENSE](LICENSE)。

