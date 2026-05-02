# Balatro Mobile Maker

The goal of this project is to allow *Balatro* fans to play *Balatro* on their mobile devices. This project provides a **NON-PIRACY** avenue for players to do so, by converting their *Steam* copy of *Balatro* to a mobile app.

Balatro Mobile Maker also supports automatically transferring your saves back and forth between your *Steam* copy of the game and your mobile device (save transfer features only officially available on **Windows** and **Android**, for now).

> Keep in mind that Balatro Mobile Maker is still in beta! Please report any bugs you encounter in the [issues section](https://github.com/SplitGemini/balatro-mobile-maker/issues). If you encounter bugs with the latest release, try the previous release.

## Quick Start Guide

Please review the **Notes** section before you begin.

1. Download or compile [**balatro-mobile-maker**](https://github.com/SplitGemini/balatro-mobile-maker/releases).
2. Run **balatro-mobile-maker**.
3. Follow the prompts to apply optional patches. If you're unsure, always select **Y**.

### For Android
- Copy the resulting **balatro.apk** to your Android device, or allow the program to automatically install using [USB Debugging](https://developer.android.com/studio/debug/dev-options).
- Optionally, allow the program to automatically transfer your saves from your *Steam* copy of *Balatro* using [USB Debugging](https://developer.android.com/studio/debug/dev-options).

### For iOS
- Sideload **balatro.ipa** using [AltStore](https://altstore.io/).
- Optionally, [copy your saves to your iOS device](https://github.com/SplitGemini/balatro-mobile-maker/issues/64#issuecomment-2094660508).

## Optional Patches

| Patch | Description |
|-------|-------------|
| **FPS Cap** | Caps FPS to a desired number (or to the device's native refresh rate — recommended for battery performance). |
| **Landscape Orientation** | Locks the game to landscape orientation (recommended, since portrait orientation does not behave very well). |
| **High DPI** | Enables [High DPI graphics mode in Love](https://love2d.org/wiki/love.window.setMode) (recommended for iOS). |
| **CRT Shader Disable** | Disables the CRT shader (recommended for Pixel and some other devices). |
| **External Storage** | Allows saving to `/storage/emulated/0/Documents/Balatro` so saves and mods can be managed outside the app sandbox (**recommended for modding**). |

## Mods Support

Mods are supported via the **External Storage** patch. When enabled, the game reads from and writes to the shared Documents folder, making it easier to manage mods on Android.

- **Mods Directory (Android):** `/storage/emulated/0/Documents/Balatro/game/save/Mods`
  - Place your mod files or `lovely` injector files here just like on PC.
- **Backups:** The save transfer feature keeps up to **10 dated backups** (`-backup-YYYYMMDD`) on both your PC and Android device. Old backups are pruned automatically.
- **Excluded Paths:** The following debug folders are excluded from sync to reduce backup size and transfer time:
  - `save/Mods/lovely/log`
  - `save/Mods/lovely/dump`
  - `save/Mods/lovely/game-dump`

> **Tip:** If you install the latest **[Steamodded](https://github.com/Steamodded/smods/)**, you generally do **not** need to apply most other Optional Patches (e.g., FPS Cap, Landscape, High DPI, CRT Disable), as Steamodded handles these settings automatically.

### External Storage Setup (Android)

After installing the APK, you must manually grant all-files access:

1. Go to **Settings → Apps → Special app access → All files access → Balatro**.
2. Grant **Allow access to manage all files**.
   - The app will not automatically prompt for storage permissions. This is the only way to enable external storage without rewriting parts of the LOVE2D Android wrapper.
3. Save files will automatically save to `/storage/emulated/0/Documents/Balatro`.
   - This path is hardcoded in `Patching.cs`; you can try to change it if you want, but your mileage may vary.

## Notes

- This script assumes that **Balatro.exe** or **Game.love** is located in the default *Steam* directory. If it is not, simply copy your **Balatro.exe** or **Game.love** to the same folder as **balatro-mobile-maker**.
- This script will automatically download [7-Zip](https://www.7-zip.org/).

### Android Dependencies
- [OpenJDK](https://www.microsoft.com/openjdk)
- [APK Tool](https://apktool.org/)
- [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/)
- [love-11.5-android-embed.apk](https://github.com/love2d/love-android/)
- [Balatro-APK-Patch](https://github.com/SplitGemini/balatro-mobile-maker/releases/tag/Additional-Tools-1.0)
- [Android Developer Bridge](https://developer.android.com/tools/adb) (optional, for auto-install / save transfer)

### iOS Dependencies
- [Balatro-IPA-Base](https://github.com/SplitGemini/balatro-mobile-maker/releases/tag/Additional-Tools-1.0)

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
