using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using static balatro_mobile_maker.Program;
using static balatro_mobile_maker.Platform;
using System.Diagnostics;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace balatro_mobile_maker;
internal class Tools
{
    public static FastZip fastZip = new FastZip();

    public enum ProcessTools
    {
        ADB,
        Java,
    }

    public static void useTool(ProcessTools tool, string args)
    {
        switch (tool)
        {
            case ProcessTools.ADB:
                Platform.useADB(args);
                break;
            case ProcessTools.Java:
                Platform.useOpenJDK(args);
                break;
        }
    }

    public static bool directoryExists(string path)
    {
        return Directory.Exists(path);
    }
    
    public static void fileMove(string source, string dest)
    {
        fileCopy(source, dest);
        tryDelete(source);
    }

    public static void fileCopy(string source, string dest)
    {
        if (!fileExists(source))
            return;

        if (fileExists(dest))
            tryDelete(dest);

        File.Copy(source, dest);
    }

    public static bool fileExists(string file)
    {
        return File.Exists(file);
    }

    public static void extractZip(string file, string directory)
    {
        fastZip.ExtractZip(file, directory, null);
    }

    public static void compressZip(string directory, string file)
    {
        fastZip.CreateZip(file, directory, true, null);
    }

    public static void tryDelete(string target)
    {
        if (Directory.Exists(target))
        {
            if (_verboseMode)
                Log("Deleting \"" + target + "/\"...");
            Directory.Delete(target, true);
        }

        if (File.Exists(target))
        {
            if (_verboseMode)
                Log("Deleting \"" + target + "\"...");
            File.Delete(target);
        }
    }

    /// <summary>
    /// Attempts to download a file if it does not exist
    /// </summary>
    /// <param name="name">Friendly name for file (for logging)</param>
    /// <param name="link">Download URL</param>
    /// <param name="fileName">File path to save to</param>
    public static void TryDownloadFile(string name, string link, string fileName)
    {
        //If the file does not already exist
        if (!File.Exists(fileName))
        {
            Log("Downloading " + name + "...");
            // TODO: WebClient is Obsolete, and needs to be replaced.
            using (var client = new WebClient())
            {
                client.DownloadFile(link, fileName!);
            }

            //Make sure it exists
            if (File.Exists(fileName))
                Log(name + " downloaded successfully.");
            else
            {
                //If it does not, that's a critical error
                Log("Failed to download " + name + "!");
                Exit();
            }
        }
        else
        {
            //File already exists
            Log(fileName + " already exists.");
        }
    }

    /// <summary>
    /// Wrapper for logging to the console.
    /// </summary>
    /// <param name="text">Text to be logged.</param>
    // This saves me from writing Console.WriteLine a million times
    // ReSharper disable once GrammarMistakeInComment
    // There's probably a better way to make an alias in C#. Oh well
    public static void Log(string text)
    {
        Console.WriteLine(text);
    }

    /// <summary>
    /// Exits the application after the user presses any key
    /// </summary>
    public static void Exit()
    {
        View.Cleanup();
        Log("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }

    /// <summary>
    /// Prints output (or errors) if verbose mode is enabled
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ProcessOutputHandler(object sender, DataReceivedEventArgs e)
    {
        string data = e.Data;
        if (_verboseMode && data != null && data != "")
            try //I got System.InvalidOperationException here once. Seems to happen if ADB exits fatally
            {
                Log("[" + ((System.Diagnostics.Process)sender).ProcessName + "]: " + data);
            }
            catch
            {
                Log("Error Occurred!");
            }
        //I'd like to use another color for this text specifically, but I'm not sure if it's possible.
    }

    /// <summary>
    /// Starts process using the platform's shell
    /// Currently this is restricted to Windows.
    /// </summary>
    /// <param name="args">Command to pass to the shell</param>
    /// <returns>Process, post finishing.</returns>
    public static Process RunCommand(string command, string args)
    {
        //Create a new cmd process
        Process commandLineProcess = new Process();
        commandLineProcess.StartInfo.FileName = command;
        commandLineProcess.StartInfo.CreateNoWindow = true;
        commandLineProcess.StartInfo.UseShellExecute = false;

        //Output and error handling
        commandLineProcess.StartInfo.RedirectStandardOutput = true;
        commandLineProcess.StartInfo.RedirectStandardError = true;
        commandLineProcess.OutputDataReceived += ProcessOutputHandler;
        commandLineProcess.ErrorDataReceived += ProcessOutputHandler;

        //Apply args
        commandLineProcess.StartInfo.Arguments = args;

        //Start the process
        commandLineProcess.Start();
        commandLineProcess.BeginOutputReadLine();

        //This could be changed to allow for multi-threading, but that's handled with System.Threading anyway
        commandLineProcess.WaitForExit();

        //On exit
        commandLineProcess.Exited += (_, _) =>
        {
            //Check for errors
            if (commandLineProcess.ExitCode != 0)
            {
                //Error occurred
                Log("An unexpected error occurred!");
                if (!_verboseMode)
                    Log("Try running in verbose mode to determine the cause of the error.");
            }
            else
                Log("\n");
        };

        //Return the process
        return commandLineProcess;
    }

    /// <summary>
    /// Runs a command and returns its standard output as a string.
    /// </summary>
    public static string RunCommandWithOutput(string command, string args)
    {
        Process p = new Process();
        p.StartInfo.FileName = command;
        p.StartInfo.Arguments = args;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;

        StringBuilder output = new StringBuilder();
        p.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                output.AppendLine(e.Data);
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();

        return output.ToString();
    }

    /// <summary>
    /// Runs an ADB command and returns its standard output.
    /// </summary>
    public static string RunADBWithOutput(string args)
    {
        if (isWindows)
            return RunCommandWithOutput("platform-tools\\platform-tools\\adb.exe", args);

        // TODO: OSX and Linux implementations
        return "";
    }

    /// <summary>
    /// Copies a directory recursively to a destination, excluding specific relative sub-paths.
    /// </summary>
    public static void CopyDirectoryExcluding(string sourceDir, string destDir, string[] excludePaths, string currentRelativePath = "")
    {
        Directory.CreateDirectory(destDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string fileRelativePath = string.IsNullOrEmpty(currentRelativePath)
                ? fileName
                : currentRelativePath + "/" + fileName;

            bool excluded = false;
            foreach (string exclude in excludePaths)
            {
                if (fileRelativePath.Equals(exclude, StringComparison.OrdinalIgnoreCase))
                {
                    excluded = true;
                    break;
                }
            }

            if (!excluded)
            {
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string dirRelativePath = string.IsNullOrEmpty(currentRelativePath)
                ? dirName
                : currentRelativePath + "/" + dirName;

            bool excluded = false;
            foreach (string exclude in excludePaths)
            {
                if (dirRelativePath.Equals(exclude, StringComparison.OrdinalIgnoreCase))
                {
                    excluded = true;
                    break;
                }
            }

            if (!excluded)
            {
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectoryExcluding(subDir, destSubDir, excludePaths, dirRelativePath);
            }
        }
    }

    /// <summary>
    /// Deletes specific relative sub-paths within a base directory if they exist.
    /// </summary>
    public static void DeleteExcludedPaths(string baseDir, string[] excludePaths)
    {
        foreach (string excludePath in excludePaths)
        {
            string fullPath = Path.Combine(baseDir, excludePath.Replace('/', Path.DirectorySeparatorChar));
            tryDelete(fullPath);
        }
    }

    /// <summary>
    /// Prompt user for a 'Y' or a 'N' (not case-sensitive)
    /// </summary>
    /// <param name="question">Prompt for the user</param>
    /// <returns>Status of prompt - true for 'Y', false for 'N'</returns>
    public static bool AskQuestion(string question)
    {
        string input = null;
        do
        {
            if (input != null)
                Log("Enter either 'Y' or 'N'!");
            Log(question + " (y/n):");
            input = Console.ReadLine()?.ToLower();
        } while (input != "y" && input != "n");

        return input == "y";
    }

    public static void ModifyZip()
    {
        string existingZipFile = "balatro-base.zip";
        string newFilePath = "game.love";
        string arcname = "Payload/Balatro.app/game.love";

        using (ZipArchive archive = System.IO.Compression.ZipFile.Open(existingZipFile, ZipArchiveMode.Update))
        {
            archive.CreateEntryFromFile(newFilePath, arcname);
        }
    }
}
