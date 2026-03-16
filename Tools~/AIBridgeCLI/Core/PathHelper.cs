using System;
using System.IO;

namespace AIBridgeCLI.Core
{
    /// <summary>
    /// Helper class for resolving paths
    /// </summary>
    public static class PathHelper
    {
        private static string _exchangeDir;

        /// <summary>
        /// Get the Exchange directory path (where commands and results are stored)
        /// </summary>
        public static string GetExchangeDirectory()
        {
            if (_exchangeDir != null)
            {
                return _exchangeDir;
            }

            // Method 1: UNITY_PROJECT_ROOT environment variable
            var projectRoot = Environment.GetEnvironmentVariable("UNITY_PROJECT_ROOT");
            if (!string.IsNullOrEmpty(projectRoot))
            {
                _exchangeDir = Path.Combine(projectRoot, "AIBridgeCache");
                return _exchangeDir;
            }

            // Method 2: Search up from current working directory
            projectRoot = FindUnityProjectRoot(Directory.GetCurrentDirectory());
            if (!string.IsNullOrEmpty(projectRoot))
            {
                _exchangeDir = Path.Combine(projectRoot, "AIBridgeCache");
                return _exchangeDir;
            }

            // Method 3: Detect installed path AIBridgeCache/CLI/AIBridgeCLI.exe
            // → parent of exe dir is AIBridgeCache itself
            var exeDir = AppDomain.CurrentDomain.BaseDirectory
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parentDir = Path.GetDirectoryName(exeDir);
            if (Path.GetFileName(exeDir).Equals("CLI", StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(parentDir ?? string.Empty).Equals("AIBridgeCache", StringComparison.OrdinalIgnoreCase))
            {
                _exchangeDir = parentDir;
                return _exchangeDir;
            }

            // Method 4: Legacy fallback Tools~/Exchange
            var toolsDir = Path.GetDirectoryName(exeDir);
            _exchangeDir = Path.Combine(toolsDir, "Exchange");
            return _exchangeDir;
        }

        /// <summary>
        /// Find Unity project root by searching up the directory tree
        /// </summary>
        private static string FindUnityProjectRoot(string startDir)
        {
            var dir = startDir;
            while (!string.IsNullOrEmpty(dir))
            {
                // Check for Unity project markers: Assets folder and ProjectSettings
                if (Directory.Exists(Path.Combine(dir, "Assets")) &&
                    File.Exists(Path.Combine(dir, "ProjectSettings", "ProjectSettings.asset")))
                {
                    return dir;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        /// <summary>
        /// Get the commands directory path
        /// </summary>
        public static string GetCommandsDirectory()
        {
            return Path.Combine(GetExchangeDirectory(), "commands");
        }

        /// <summary>
        /// Get the results directory path
        /// </summary>
        public static string GetResultsDirectory()
        {
            return Path.Combine(GetExchangeDirectory(), "results");
        }

        /// <summary>
        /// Get the screenshots directory path
        /// </summary>
        public static string GetScreenshotsDirectory()
        {
            return Path.Combine(GetExchangeDirectory(), "screenshots");
        }

        /// <summary>
        /// Ensure all required directories exist
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            var commandsDir = GetCommandsDirectory();
            var resultsDir = GetResultsDirectory();

            if (!Directory.Exists(commandsDir))
            {
                Directory.CreateDirectory(commandsDir);
            }

            if (!Directory.Exists(resultsDir))
            {
                Directory.CreateDirectory(resultsDir);
            }
        }

        /// <summary>
        /// Generate a unique command ID
        /// </summary>
        public static string GenerateCommandId()
        {
            return $"cmd_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}".Substring(0, 32);
        }
    }
}
