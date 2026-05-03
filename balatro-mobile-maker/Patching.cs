using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static balatro_mobile_maker.Tools;

namespace balatro_mobile_maker;

internal class Patching
{
    /// <summary>
    /// Apply a "patch" given a file path, a string from a line to be replaced, and the text with which to replace it.
    /// </summary>
    /// <param name="file">File to patch</param>
    /// <param name="lineContains">Line to be replaced</param>
    /// <param name="replaceWith">New contents for the line</param>
    /// <returns></returns>
    // We wish to keep the return type, incase we want to make use of this later.
    // ReSharper disable once UnusedMethodReturnValue.Local
    static bool ApplyPatch(string file, string lineContains, string replaceWith)
    {
        //Read the file
        Log("Loading " + file + " file...");
        string[] loadedFile = File.ReadAllLines("Balatro/" + file);

        //Search for the line to replace
        bool found = false;
        for (int i = 0; i < loadedFile.Length; i++)
            // This has to be made culture-invariant, or, in some regions this could result in unexpected behaviour
            // Consider also using .Contains here - is there a reason we make use of IndexOf?
            if (loadedFile[i].IndexOf(lineContains, StringComparison.Ordinal) != -1)
            {
                //Replace the line
                loadedFile[i] = replaceWith;
                found = true;
                break;
            }

        if (found)
        {
            //If it is found, write the file.
            Log("Successfully applied patch...");
            File.WriteAllLines("Balatro/" + file, loadedFile);
        }
        else
            Log("Unable to find patch location...");

        return found;
    }

    /// <summary>
    /// Prompts the user to select which patches they want, then applies them.
    /// </summary>
    // This is hideous, but it works.
    public static void Begin()
    {
        Tools.Log("External storage is handled by the custom Android liblove; skipping Lua save-path patch.");
    }
}
