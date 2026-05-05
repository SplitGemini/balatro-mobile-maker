# Balatro Mobile Maker

[中文文档](README_zh-CN.md)

The goal of this project is to allow *Balatro* fans to play *Balatro* on their mobile devices. This project provides a **NON-PIRACY** avenue for players to do so, by converting their *Steam* copy of *Balatro* to a mobile app.

Balatro Mobile Maker also supports automatically transferring your saves back and forth between your *Steam* copy of the game and your mobile device (save transfer features only officially available on **Windows** and **Android**, for now).

> Keep in mind that Balatro Mobile Maker is still in beta! Please report any bugs you encounter in the [issues section](https://github.com/SplitGemini/balatro-mobile-maker/issues). If you encounter bugs with the latest release, try the previous release.

## Setup & Usage

### 1. Build the APK

1. **Download** the latest release of [**balatro-mobile-maker**](https://github.com/SplitGemini/balatro-mobile-maker/releases) for your OS (Windows/Linux/macOS).
2. **Extract** the downloaded archive to a folder of your choice.
3. **Place your game file** — copy `Balatro.exe` (from Steam) or `Game.love` into the same folder as `balatro-mobile-maker`.
   - The tool can auto-detect `Balatro.exe` from the default Steam directory, but manual placement is more reliable.
4. **Run** `balatro-mobile-maker` and follow the CLI prompts:
   - Choose whether to clean up temporary files after build.
   - Confirm Android build.
5. Wait for the build to complete. The output is a signed **`balatro.apk`** in the same folder.

> The External Storage patch is applied automatically. Saves and mods are stored in `/storage/emulated/0/Documents/Balatro/game/save/`.

### 2. Install on Android

**Option A: Manual install**

1. Copy **`balatro.apk`** to your Android device (via USB, cloud storage, etc.).
2. On Android, open the file manager, locate the APK, and tap to install.
3. If prompted, allow installation from this source.

**Option B: Auto-install via ADB**

1. Enable [USB Debugging](https://developer.android.com/studio/debug/dev-options) on your Android device.
2. Connect your device to PC via USB.
3. When the tool asks *"Would you like to automatically install balatro.apk?"*, answer **Y**.
4. If a permission dialog appears on your phone, allow USB debugging.

### 3. Grant All-Files Access (Required)

After installing the APK, you **must** grant all-files access so the game can read/write saves and mods in the shared Documents folder:

1. Open **Settings → Apps → Special app access → All files access**.
2. Find and tap **Balatro** in the list.
3. Toggle **Allow access to manage all files** to ON.

> The app will **not** auto-prompt for this permission. If skipped, the app will not start.

### 4. Install Steamodded (Required)

**You MUST install [Steamodded](https://github.com/Steamodded/smods/)**. Without it, the game will likely crash or have severe rendering/input issues on Android.

**Installation steps:**

1. On your PC (or directly on Android), download the latest **Steamodded** release from [GitHub Releases](https://github.com/Steamodded/smods/releases).
2. Extract the downloaded archive. You should get a folder like `smods-X.X.X`.
3. Copy the **entire `smods` folder** (or individual mod files) to your Android device at:
    ```
    /storage/emulated/0/Documents/Balatro/game/save/Mods/
    ```
   - Use a file manager on Android, or
   - Connect via USB and copy directly.
4. Launch the game. If installed correctly, a mod loader screen should appear on startup.

> See the [Steamodded Wiki](https://github.com/Steamodded/smods/wiki) for troubleshooting.

### 5. (Optional) Transfer Saves

If you want to continue your Steam progress on mobile (or vice versa):

**Prerequisites:** Enable USB Debugging on your Android device and connect it to your PC.

**Push saves (PC → Android):**

- When the tool asks *"Would you like to transfer saves from your Steam copy?"*, answer **Y**.
- The tool automatically creates a backup on Android before overwriting.

**Pull saves (Android → PC):**

- When asked *"Would you like to pull saves from your Android device?"*, answer **Y**.
- The tool backs up your PC saves first, then pulls the Android saves into the Steam save folder.

> Backups are created automatically before overwriting. Up to **10 dated backups** (`-backup-YYYYMMDD`) are kept on both PC and Android. Old backups are pruned automatically.

**Alternative: Continuous Sync**

For seamless cross-device progress, you can set up continuous sync between your PC and Android save folders using tools like [Syncthing](https://syncthing.net/) or [FolderSync](https://play.google.com/store/apps/details?id=dk.tacit.android.foldersync.lite):

- **PC path:** `%AppData%\Balatro\` (Windows)
- **Android path:** `/storage/emulated/0/Documents/Balatro/game/save/`

Configure both folders as a shared/synced pair. This way your progress stays in sync automatically without needing to manually push or pull.

## Mods & Save Support

The **External Storage** patch is automatically applied. The game reads from and writes to `/storage/emulated/0/Documents/Balatro/game/save/`, making mods and save management easy on Android.

- **Mods Directory:** `/storage/emulated/0/Documents/Balatro/game/save/Mods`
  - Place your mod files or `lovely` injector files here just like on PC.
- **Recommended Mod:** [SilkTouch](https://github.com/SplitGemini/SilkTouch) — A Balatro mod that brings touch controls from mobile version to PC/Mac.
- **Backups:** Up to **10 dated backups** (`-backup-YYYYMMDD`) are kept on both PC and Android. Old backups are pruned automatically.
- **Excluded sync paths:** `Mods/lovely/log`, `Mods/lovely/dump`, `Mods/lovely/game-dump`

## Notes

- This script assumes that **Balatro.exe** or **Game.love** is located in the default *Steam* directory. If it is not, simply copy your **Balatro.exe** or **Game.love** to the same folder as **balatro-mobile-maker**.
- This script will automatically download [7-Zip](https://www.7-zip.org/).

### Android Dependencies

- [OpenJDK](https://www.microsoft.com/openjdk)
- [APK Tool](https://apktool.org/)
- [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/)
- [love-android-embed.apk](https://github.com/SplitGemini/love-android/)
- [Balatro-APK-Patch](https://github.com/SplitGemini/balatro-mobile-maker/releases/tag/tools)
- [Android Developer Bridge](https://developer.android.com/tools/adb) (optional, for auto-install / save transfer)
- [Lovely Injector (Android)](https://github.com/SplitGemini/lovely-injector)

## Recognition (in no particular order)

- [Every contributor](https://github.com/SplitGemini/balatro-mobile-maker/graphs/contributors)
- Developers of [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer)
- Developers of [LÖVE](https://love2d.org/)
- Developers of [7-Zip](https://www.7-zip.org/)
- Developers of [APKTool](https://apktool.org/)
- Developers of [Balatro](https://www.playbalatro.com/)

## License

- [7-Zip](https://github.com/ip7z/7zip/blob/main/DOC/License.txt) is licensed under the GNU LGPL license.
- This project uses [APKTool](https://github.com/iBotPeaches/Apktool/blob/master/LICENSE.md).
- This project uses [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/blob/main/LICENSE).
- This project uses [LÖVE](https://github.com/love2d/love/blob/main/license.txt).
- This project uses [OpenJDK](https://www.microsoft.com/openjdk).
