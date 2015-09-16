using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

// todo: compile/run only on windows

namespace Gwen.Platform
{
    /// <summary>
    /// Windows-specific utility functions.
    /// </summary>
    public static class Windows
    {
        private const string fontRegKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

        private static Dictionary<string, string> fontPaths;

        /// <summary>
        /// Gets a font file path from font name.
        /// </summary>
        /// <param name="fontName">Font name.</param>
        /// <returns>Font file path.</returns>
        public static string GetFontPath(string fontName)
        {
            // is this reliable? we rely on lazy jitting to not run win32 code on linux
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return null;

            if (fontPaths == null)
                initFontPaths();

            if (!fontPaths.ContainsKey(fontName))
                return null;

            return fontPaths[fontName];
        }

        private static void initFontPaths()
        {
            // very hacky but better than nothing
            fontPaths = new Dictionary<string, string>();
            string fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            RegistryKey subkey = key.OpenSubKey(fontRegKey);
            foreach (string fontName in subkey.GetValueNames())
            {
                string fontFile = (string)subkey.GetValue(fontName);
                if (!fontName.EndsWith(" (TrueType)"))
                    continue;
                string font = fontName.Replace(" (TrueType)", "");
                fontPaths[font] = Path.Combine(fontsDir, fontFile);
            }
            key.Dispose();
        }
    }
}
