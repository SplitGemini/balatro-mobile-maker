using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text.RegularExpressions;
using static balatro_mobile_maker.Constants;
using static balatro_mobile_maker.Tools;
using static balatro_mobile_maker.Program;

namespace balatro_mobile_maker;

/// <summary>
/// Command line UI for Balatro APK Maker.
/// </summary>
// NOTE: Much should be refactored out of UI logic land, and, into a Controller which we can query state from.
internal class View
{

    private bool _androidBuild;
    private bool _iosBuild;
    private bool _addLovely;

    private static bool _cleaup;

    static bool gameProvided;

    private static readonly string androidSavePath = "/storage/emulated/0/Documents/Balatro/game/save";
    private static readonly string[] excludePaths = ["Mods/lovely/log", "Mods/lovely/dump", "Mods/lovely/game-dump"];

    /// <summary>
    /// Start CLI operation.
    /// </summary>
    public void Begin()
    {
        Log("====Balatro APK Maker====\n");

        //Initial prompts
        _cleaup = AskQuestion("Would you like to automatically clean up once complete?");
        _verboseMode = AskQuestion("Would you like to enable extra logging information?");

        //If balatro.apk or balatro.ipa already exists, ask before beginning build process again
        if (!(fileExists("balatro.apk") || fileExists("balatro.ipa")) || AskQuestion("A previous build was found... Would you like to build again?"))
        {
            _androidBuild = AskQuestion("Would you like to build for Android?");
            if(_androidBuild)
            {
                _addLovely = AskQuestion("Would you like to add lovely injector?");
            }
            _iosBuild = AskQuestion("Would you like to build for iOS (experimental)?");


            if (_androidBuild || _iosBuild)
            {
                #region Download tools
                if (_androidBuild)
                {
                    #region Android tools
                    //Downloading tools. Handled in threads to allow simultaneous downloads
                    Thread[] downloadThreads =
                    [
                        new Thread(() => { TryDownloadFile("OpenJDK", Platform.getOpenJDKDownloadLink(), "openjdk"); }),

                        new Thread(() => { TryDownloadFile("APKTool", ApktoolLink, "apktool.jar"); }),
                        new Thread(() => { TryDownloadFile("uber-apk-signer", UberapktoolLink, "uber-apk-signer.jar"); }),
                        new Thread(() => { TryDownloadFile("Balatro-APK-Patch", BalatroApkPatchLink, "Balatro-APK-Patch.zip"); }),
                        new Thread(() => { TryDownloadFile("Love2D APK", Love2dApkLink, "love-11.5-android-embed.apk"); }),
                        new Thread(() => { TryDownloadFile("Lovely Injector (Android)", LibLovelyAndroidLink, "lovely-aarch64-linux-android.tar.gz"); })
                    ];

                    //Start all the downloads
                    foreach (var t in downloadThreads) t.Start();

                    //Wait for all the downloads to complete
                    foreach (var t in downloadThreads) t.Join();

                    #endregion
                }

                if (_iosBuild)
                {
                    #region iOS Tools
                    //Downloading tools. Handled in threads to allow simultaneous downloads
                    Thread[] downloadThreads =
                    [
                        new Thread(() => { TryDownloadFile("iOS Base", IosBaseLink, "balatro-base.ipa"); })
                    ];

                    //Start all the downloads
                    foreach (var t in downloadThreads) t.Start();

                    //Wait for all the downloads to complete
                    foreach (var t in downloadThreads) t.Join();
                    #endregion
                }
                #endregion

                #region Prepare workspace
                #region Find and extract Balatro.exe

                gameProvided = Platform.gameExists();

                if (gameProvided)
                    Log("Game found!");
                else
                {
                    //Game not provided

                    //Try to locate automatically
                    if (Platform.tryLocateGame())
                        Log("Game copied!");
                    else
                    {
                        //Game not provided, and could not be located
                        Log("Could not find Balatro.exe! Please place it in this folder, then try again!");
                        Exit();
                    }
                }

                Log("Extracting Balatro.exe...");
                if (directoryExists("Balatro"))
                {
                    //Delete the Balatro folder if it already exists
                    Log("Balatro directory already exists! Deleting Balatro directory...");
                    tryDelete("Balatro");
                }

                //Extract Balatro.exe
                extractZip("Balatro.exe", "Balatro");

                //Check for failure
                if (!directoryExists("Balatro"))
                {
                    Log("Failed to extract Balatro.exe!");
                    Exit();
                }
                #endregion

                if (_androidBuild)
                {
                    #region Extract APK
                    Log("Unpacking Love2D APK with APK Tool...");
                    if (directoryExists("balatro-apk"))
                    {
                        //Delete the balatro-apk folder if it already exists
                        Log("balatro-apk directory already exists! Deleting balatro-apk directory...");
                        tryDelete("balatro-apk");
                    }

                    //Unpack Love2D APK
                    useTool(ProcessTools.Java, "-jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" d -o balatro-apk love-11.5-android-embed.apk");

                    //Check for failure
                    if (!directoryExists("balatro-apk"))
                    {
                        Log("Failed to unpack Love2D APK with APK Tool!");
                        Exit();
                    }
                    #endregion

                    #region APK patch
                    Log("Extracting patch zip...");
                    if (directoryExists("Balatro-APK-Patch"))
                    {
                        Log("Balatro-APK-Patch directory already exists! Deleting Balatro-APK-Patch directory...");
                        tryDelete("Balatro-APK-Patch");
                    }

                    //Extract Balatro-APK-Patch
                    extractZip("Balatro-APK-Patch.zip", "Balatro-APK-Patch");

                    if (!directoryExists("Balatro-APK-Patch"))
                    {
                        Log("Failed to extract Balatro-APK-Patch");
                        Exit();
                    }

                    //Base APK patch
                    Log("Patching APK folder...");
                    //This isn't pretty, but I'm planning to change how icons are done at some point. So this is fine for now.
                    fileCopy("Balatro-APK-Patch/AndroidManifest.xml", "balatro-apk/AndroidManifest.xml");
                    Tools.Log("Injecting external storage permissions...");
                    string manifestPath = "balatro-apk/AndroidManifest.xml";
                    if (File.Exists(manifestPath))
                    {
                        string manifestContent = File.ReadAllText(manifestPath);

                        //inject permissions and legacy storage flag
                        string permissionBlock = "<uses-permission android:name=\"android.permission.READ_EXTERNAL_STORAGE\"/>\n" +
                                                 "<uses-permission android:name=\"android.permission.WRITE_EXTERNAL_STORAGE\"/>\n" +
                                                 "<uses-permission android:name=\"android.permission.MANAGE_EXTERNAL_STORAGE\"/>\n" +
                                                 "<application android:requestLegacyExternalStorage=\"true\" android:preserveLegacyExternalStorage=\"true\"";

                        manifestContent = manifestContent.Replace("<application", permissionBlock);

                        // Update version from version.jkr
                        string versionFile = "Balatro/version.jkr";
                        if (File.Exists(versionFile))
                        {
                            string version = File.ReadAllLines(versionFile)[0].Trim();
                            Tools.Log("Setting APK version to " + version + "...");
                            manifestContent = System.Text.RegularExpressions.Regex.Replace(
                                manifestContent,
                                "android:versionName=\"[^\"]*\"",
                                "android:versionName=\"" + version + "\"");
                        }

                        File.WriteAllText(manifestPath, manifestContent);
                    }
                    PatchAndroidTargetSdkForExternalStorage();
                    fileCopy("Balatro-APK-Patch/res/drawable-hdpi/love.png", "balatro-apk/res/drawable-hdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-mdpi/love.png", "balatro-apk/res/drawable-mdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-xhdpi/love.png", "balatro-apk/res/drawable-xhdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-xxhdpi/love.png", "balatro-apk/res/drawable-xxhdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-xxxhdpi/love.png", "balatro-apk/res/drawable-xxxhdpi/love.png");
                    #endregion
                }

                #region Lovely injector

                if(_addLovely)
                {
                    var smali = File.ReadAllText("balatro-apk/smali/org/love2d/android/GameActivity.smali");
                    // Insert a loadLibrary call
                    smali = smali.Replace("invoke-direct {p0}, Lorg/libsdl/app/SDLActivity;-><init>()V", "const-string v0, \"lovely\"\n    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n    invoke-direct {p0}, Lorg/libsdl/app/SDLActivity;-><init>()V");
                    File.WriteAllText("balatro-apk/smali/org/love2d/android/GameActivity.smali", smali);
                    Directory.CreateDirectory("lovely-aarch64-linux-android");
                    var stream = new GZipStream(File.OpenRead("lovely-aarch64-linux-android.tar.gz"), CompressionMode.Decompress);
                    TarFile.ExtractToDirectory(stream, "lovely-aarch64-linux-android", true);
                    fileCopy("lovely-aarch64-linux-android/liblovely.so", "balatro-apk/lib/arm64-v8a/liblovely.so");
                }

                #endregion

                if (_iosBuild)
                {
                    #region Prepare IPA
                    Log("Preparing iOS Base...");
                    fileMove("balatro-base.ipa", "balatro-base.zip");
                    #endregion
                }

                #endregion

                #region Patch
                Log("Patching...");
                Patching.Begin();
                #endregion

                #region Building

                #region Balatro.exe -> game.love
                Log("Packing Balatro folder...");
                compressZip("Balatro/.", "balatro.zip");

                if (!fileExists("balatro.zip"))
                {
                    Log("Failed to pack Balatro folder!");
                    Exit();
                }

                Log("Moving archive...");
                if (_androidBuild)
                    fileCopy("balatro.zip", "balatro-apk/assets/game.love");

                if (_iosBuild)
                    fileCopy("balatro.zip", "game.love");
                #endregion

                if (_androidBuild)
                {
                    #region Packing APK
                    Log("Repacking APK...");
                    useTool(ProcessTools.Java, "-jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" b -o balatro.apk balatro-apk");

                    if (!fileExists("balatro.apk"))
                    {
                        Log("Failed to pack Balatro apk!");
                        Exit();
                    }
                    #endregion

                    #region Signing APK
                    Log("Signing APK...");
                    useTool(ProcessTools.Java, "-jar uber-apk-signer.jar -a balatro.apk");

                    if (!fileExists("balatro-aligned-debugSigned.apk"))
                    {
                        Log("Failed to sign APK!");
                        Exit();
                    }

                    Log("Renaming unsigned apk...");
                    fileMove("balatro.apk", "balatro-unsigned.apk");

                    Log("Renaming signed apk...");
                    fileMove("balatro-aligned-debugSigned.apk", "balatro.apk");
                    #endregion
                }

                if (_iosBuild)
                {
                    #region Packing IPA
                   
                    Log("Repacking iOS app...");
                    ModifyZip();

                    fileMove("balatro-base.zip", "balatro.ipa");
                    #endregion
                }
                Log("Build successful!");
                #endregion
            }
        }

        //TODO: Implement for OSX and Linux!!!
        if ((!_iosBuild || _androidBuild) && Platform.isWindows)
        {
            #region Android options
            #region Auto-install
            if (fileExists("balatro.apk") && AskQuestion("Would you like to automaticaly install balatro.apk on your Android device?"))
            {
                PrepareAndroidPlatformTools();

                Log("Attempting to install. If prompted, please allow the USB Debugging connection on your Android device.");

                useTool(ProcessTools.ADB, "install balatro.apk");
                if (_addLovely)
                {
                    GrantLovelyExternalStorageAccess();
                }
                useTool(ProcessTools.ADB, "kill-server");
            }
            else if (fileExists("balatro.apk") && _addLovely)
            {
                Log("Lovely mods need external storage access on Android 11+.");
                Log("After installing, run: adb shell appops set " + AndroidPackageName + " MANAGE_EXTERNAL_STORAGE allow");
            }
            #endregion

            #region Save transfer

            if (directoryExists(Environment.GetEnvironmentVariable("AppData") + "\\Balatro") && AskQuestion("Would you like to transfer saves from your Steam copy of Balatro to your Android device?"))
            {
                Log("Thanks to TheCatRiX for figuring out save transfers!");

                PrepareAndroidPlatformTools();

                Log("Attempting to transfer saves. If prompted, please allow the USB Debugging connection on your Android device.");

                // Backup existing save on Android device before overwriting
                Log("Backing up existing save files on Android device...");
                string dateSuffix = DateTime.Now.ToString("yyyyMMdd");
                string backupPath = androidSavePath + "-backup-" + dateSuffix;
                useTool(ProcessTools.ADB, "shell rm -rf " + backupPath);
                useTool(ProcessTools.ADB, "shell cp -r " + androidSavePath + " " + backupPath);

                // Prune old backups, keep the most recent 10
                string adbOutput = Tools.RunADBWithOutput("shell ls -1 -d " + androidSavePath + "-backup-* 2>/dev/null");
                if (!string.IsNullOrWhiteSpace(adbOutput))
                {
                    var backups = adbOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(b => b.Trim())
                        .Where(b => !string.IsNullOrEmpty(b))
                        .OrderBy(b => b)
                        .ToList();

                    while (backups.Count > 10)
                    {
                        string oldBackup = backups[0];
                        useTool(ProcessTools.ADB, "shell rm -rf \"" + oldBackup + "\"");
                        backups.RemoveAt(0);
                    }
                }

                // Push from a temporary copy that excludes lovely debug folders
                string tempSaveDir = Path.Combine(Path.GetTempPath(), "balatro_save_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempSaveDir);
                Tools.CopyDirectoryExcluding(Platform.getGameSaveLocation(), tempSaveDir, excludePaths);
                useTool(ProcessTools.ADB, "push \"" + tempSaveDir + "\\.\" " + androidSavePath);
                Tools.tryDelete(tempSaveDir);

                useTool(ProcessTools.ADB, "shell am force-stop com.unofficial.balatro");
                useTool(ProcessTools.ADB, "kill-server");
            }
            else
            {
                if (AskQuestion("Would you like to pull saves from your Android device?"))
                {
                    Log("Warning! If Steam Cloud is enabled, it will overwrite the save you transfer!");
                    while (!AskQuestion("Have you backed up your saves?"))
                        Log("Please back up your saves! I am not responsible if your saves get deleted!");

                    PrepareAndroidPlatformTools();

                    Log("Backing up your files...");
                    string saveLocation = Platform.getGameSaveLocation();
                    string dateSuffix = DateTime.Now.ToString("yyyyMMdd");
                    string localBackupPath = saveLocation + "-backup-" + dateSuffix;
                    System.IO.Directory.CreateDirectory(localBackupPath);
                    Tools.CopyDirectoryExcluding(saveLocation, localBackupPath, excludePaths);

                    // Prune old local backups, keep the most recent 10
                    string saveParent = System.IO.Directory.GetParent(saveLocation)?.FullName ?? ".";
                    var localBackups = System.IO.Directory.GetDirectories(saveParent, "Balatro-backup-*")
                        .OrderBy(d => d)
                        .ToList();
                    while (localBackups.Count > 10)
                    {
                        Tools.tryDelete(localBackups[0]);
                        localBackups.RemoveAt(0);
                    }

                    tryDelete(saveLocation);
                    System.IO.Directory.CreateDirectory(saveLocation);

                    Log("Attempting to pull save files from Android device.");

                    useTool(ProcessTools.ADB, "pull " + androidSavePath + "/. \"" + saveLocation + "\"");
                    Tools.DeleteExcludedPaths(saveLocation, excludePaths);

                    useTool(ProcessTools.ADB, "kill-server");
                }
            }
            #endregion
            #endregion
        }

        Log("Finished!");
        Exit();
    }

    public static void Cleanup()
    {
        if (_cleaup)
        {
            Log("Deleting temporary files...");

            tryDelete("love-11.5-android-embed.apk");
            tryDelete("Balatro-APK-Patch.zip");//TODO: remove when Android build changes
            //tryDelete("AndroidManifest.xml");//TODO: enable when Android build changes
            tryDelete("apktool.jar");
            tryDelete("uber-apk-signer.jar");
            tryDelete("openjdk.zip");
            tryDelete("openjdk.tar.gz");
            tryDelete("openjdk");
            tryDelete("balatro-aligned-debugSigned.apk.idsig");
            tryDelete("balatro-unsigned.apk");
            tryDelete("platform-tools.zip");
            tryDelete("ios.py");
            tryDelete("balatro.zip");
            tryDelete("game.love");
            tryDelete("lovely-aarch64-linux-android.tar.gz");
            tryDelete("lovely-aarch64-linux-android");

            tryDelete("platform-tools");
            tryDelete("jdk-21.0.3+9");
            tryDelete("Balatro-APK-Patch");//TODO: remove when Android build changes
            //tryDelete("icons");//TODO: enable when Android build changes
            tryDelete("Balatro");
            tryDelete("balatro-apk");
            if (!gameProvided)
                tryDelete("Balatro.exe");
        }
    }

    /// <summary>
    /// Prepare Android platform-tools, and prompt user to enable USB debugging
    /// </summary>
    void PrepareAndroidPlatformTools()
    {
        //Check whether they already exist
        if (!directoryExists("platform-tools"))
        {
            Log("Platform tools not found...");

            if (!fileExists("platform-tools.zip"))
                TryDownloadFile("platform-tools", PlatformToolsLink, "platform-tools.zip");

            Log("Extracting platform-tools...");
            extractZip("platform-tools.zip", "platform-tools");
        }

        //Prompt user
        while (!AskQuestion("Is your Android device connected to the host with USB Debugging enabled?"))
            Log("Please enable USB Debugging on your Android device, and connect it to the host.");
    }

    static void PatchAndroidTargetSdkForExternalStorage()
    {
        const string apktoolPath = "balatro-apk/apktool.yml";
        if (!File.Exists(apktoolPath))
        {
            Log("apktool.yml not found; unable to lower targetSdkVersion for legacy external storage.");
            return;
        }

        string apktool = File.ReadAllText(apktoolPath);
        string patched = Regex.Replace(
            apktool,
            @"(?m)^(\s*targetSdkVersion:\s*)'?\d+'?\s*$",
            "$1'29'");

        if (patched == apktool)
        {
            Log("targetSdkVersion not found in apktool.yml; external storage may require manual all-files access.");
            return;
        }

        File.WriteAllText(apktoolPath, patched);
        Log("Set Android targetSdkVersion to 29 for legacy external storage access.");
    }

    void GrantLovelyExternalStorageAccess()
    {
        Log("Granting Lovely external storage access...");
        useTool(ProcessTools.ADB, "shell appops set " + AndroidPackageName + " MANAGE_EXTERNAL_STORAGE allow");
        useTool(ProcessTools.ADB, "shell appops set " + AndroidPackageName + " READ_EXTERNAL_STORAGE allow");
        useTool(ProcessTools.ADB, "shell appops set " + AndroidPackageName + " WRITE_EXTERNAL_STORAGE allow");
    }
}
