# Balatro Mobile Maker

本项目旨在让 *Balatro* 玩家能够在移动设备上游玩。它提供了一条**非盗版**的途径，通过将 *Steam* 版本的 *Balatro* 转换为 Android APK。

Balatro Mobile Maker 还支持在 PC 和 Android 设备之间自动传输存档（目前仅支持 **Windows** 和 **Android**）。

> 本项目仍处于 beta 阶段！如遇到 bug，请在 [Issues](https://github.com/SplitGemini/balatro-mobile-maker/issues) 中反馈。

## 安装与使用

### 1. 构建 APK

1. **下载** 最新版本的 [**balatro-mobile-maker**](https://github.com/SplitGemini/balatro-mobile-maker/releases)。
2. **解压** 下载的压缩包到任意文件夹。
3. **放置游戏文件** — 将 Steam 中的 `Balatro.exe` 或 `Game.love` 复制到 `balatro-mobile-maker` 同级目录下。
   - 工具可以自动从默认 Steam 目录检测 `Balatro.exe`，但手动放置更可靠。
4. **运行** `balatro-mobile-maker` 并按提示操作：
   - 选择是否在构建完成后清理临时文件。
   - 确认 Android 构建。
5. 等待构建完成。输出文件为同目录下的签名版 **`balatro.apk`**。

> External Storage 补丁会自动应用。存档和 Mod 存储在 `/storage/emulated/0/Documents/Balatro/game/save/`。

### 2. 在 Android 上安装

**方式 A：手动安装**

1. 将 **`balatro.apk`** 复制到 Android 设备（通过 USB、云盘等方式）。
2. 在 Android 上打开文件管理器，找到 APK 并点击安装。
3. 如提示，允许来自此来源的安装。

**方式 B：通过 ADB 自动安装**

1. 在 Android 设备上开启 [USB 调试](https://developer.android.com/studio/debug/dev-options)。
2. 通过 USB 将设备连接到电脑。
3. 当工具询问 *"是否自动安装 balatro.apk？"* 时，回答 **Y**。
4. 如果手机上出现权限对话框，允许 USB 调试。

### 3. 授予所有文件访问权限（必需）

安装 APK 后，**必须**手动授予所有文件访问权限，否则游戏无法读写存档和 Mod：

1. 打开 **设置 → 应用 → 特殊应用权限 → 所有文件访问权限**。
2. 在列表中找到并点击 **Balatro**。
3. 开启 **允许访问以管理所有文件**。

> 应用**不会**自动弹窗申请此权限。如果跳过，应用将无法启动。

### 4. 安装 Steamodded（必需）

**你必须安装 [Steamodded](https://github.com/Steamodded/smods/)**。没有它，游戏在 Android 上很可能崩溃或出现严重的渲染/输入问题。

**安装步骤：**

1. 在 PC（或直接在 Android）上下载最新版 **Steamodded**：[GitHub Releases](https://github.com/Steamodded/smods/releases)。
2. 解压下载的压缩包，会得到类似 `smods-X.X.X` 的文件夹。
3. 将整个 **`smods` 文件夹**复制到 Android 设备的以下路径：
   ```
   /storage/emulated/0/Documents/Balatro/game/save/Mods/
   ```
   - 使用 Android 上的文件管理器，或
   - 通过 USB 连接后直接复制。
4. 启动游戏。如果安装正确，启动时会出现 Mod 加载界面。

> 遇到问题请参阅 [Steamodded Wiki](https://github.com/Steamodded/smods/wiki)。

### 5.（可选）传输存档

如果你想在手机上继续 Steam 的进度（或反向传输）：

**前提：** Android 设备已开启 USB 调试并连接到 PC。

**推送存档（PC → Android）：**

- 当工具询问 *"是否将 Steam 存档传输到 Android 设备？"* 时，回答 **Y**。
- 工具会在覆盖前自动在 Android 上创建备份。

**拉取存档（Android → PC）：**

- 当工具询问 *"是否从 Android 设备拉取存档？"* 时，回答 **Y**。
- 工具会先备份 PC 上的存档，然后将 Android 存档拉取到 Steam 存档目录。

> 覆盖前会自动创建备份。PC 和 Android 各保留最多 **10 个按日期命名的备份**（`-backup-YYYYMMDD`），旧备份会自动清理。

**替代方案：持续同步**

如果你想让 PC 和 Android 的存档保持实时同步，可以使用 [Syncthing](https://syncthing.net/) 或 [FolderSync](https://play.google.com/store/apps/details?id=dk.tacit.android.foldersync.lite) 等工具：

- **PC 路径：** `%AppData%\Balatro\`（Windows）
- **Android 路径：** `/storage/emulated/0/Documents/Balatro/game/save/`

将两个文件夹配置为同步对，即可自动保持进度一致，无需手动推送或拉取。

## Mod 与存档支持

**External Storage** 补丁自动应用。游戏直接读写 `/storage/emulated/0/Documents/Balatro/game/save/`，在 Android 上管理 Mod 和存档非常方便。

- **Mod 目录：** `/storage/emulated/0/Documents/Balatro/game/save/Mods`
  - 像 PC 一样放置 mod 文件或 `lovely` 注入器文件。
- **推荐 Mod：** [SilkTouch](https://github.com/HuyTheKiller/SilkTouch) — 一个实用的 Balatro 生活质量 Mod。
- **备份：** PC 和 Android 各保留最多 **10 个按日期命名的备份**。旧备份自动清理。
- **排除的同步路径：** `Mods/lovely/log`、`Mods/lovely/dump`、`Mods/lovely/game-dump`

## 注意事项

- 工具默认假设 `Balatro.exe` 或 `Game.love` 位于 *Steam* 默认目录。如果不是，请手动将其复制到 `balatro-mobile-maker` 同级目录。
- 工具会自动下载 [7-Zip](https://www.7-zip.org/)。

### Android 依赖

- [OpenJDK](https://www.microsoft.com/openjdk)
- [APK Tool](https://apktool.org/)
- [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/)
- [love-android-embed.apk](https://github.com/SplitGemini/love-android/)
- [Balatro-APK-Patch](https://github.com/SplitGemini/balatro-mobile-maker/releases/tag/tools)
- [Android Developer Bridge](https://developer.android.com/tools/adb)（可选，用于自动安装/存档传输）
- [Lovely Injector (Android)](https://github.com/SplitGemini/lovely-injector)

## 致谢（排名不分先后）

- [所有贡献者](https://github.com/SplitGemini/balatro-mobile-maker/graphs/contributors)
- [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer) 开发者
- [LÖVE](https://love2d.org/) 开发者
- [7-Zip](https://www.7-zip.org/) 开发者
- [APKTool](https://apktool.org/) 开发者
- [Balatro](https://www.playbalatro.com/) 开发者

## 许可证

- [7-Zip](https://github.com/ip7z/7zip/blob/main/DOC/License.txt) 采用 GNU LGPL 许可证。
- 本项目使用 [APKTool](https://github.com/iBotPeaches/Apktool/blob/master/LICENSE.md)。
- 本项目使用 [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/blob/main/LICENSE)。
- 本项目使用 [LÖVE](https://github.com/love2d/love/blob/main/license.txt)。
- 本项目使用 [OpenJDK](https://www.microsoft.com/openjdk)。
