using System;
using System.Diagnostics;
using System.IO;

public static class SettingsManager
{
    private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");

    public static void SaveFolderPath(string folderPath)
    {
        File.WriteAllText(SettingsFilePath, folderPath);
    }

    public static string? LoadFolderPath()
    {
        if (!File.Exists(SettingsFilePath))
            return null;

        string path = File.ReadAllText(SettingsFilePath).Trim();

        return Directory.Exists(path) ? path : null;
    }
    public static void ClearFolderPath()
    {
        if (File.Exists(SettingsFilePath))
        {
            File.Delete(SettingsFilePath); // Delete the settings file to clear the folder path
        }
    }

    public static string? GetMostRecentSpoilerLogFullPath(string folder)
    {
        // Get all .txt files in the directory and filter them to include only those that have "spoiler" in their filename (case-insensitive)
        var file = Directory.GetFiles(folder, "*.txt")
                             .Where(file => Path.GetFileName(file).IndexOf("spoiler", StringComparison.OrdinalIgnoreCase) >= 0)
                             .OrderByDescending(File.GetLastWriteTime)
                             .FirstOrDefault();

        return file;
    }

}
