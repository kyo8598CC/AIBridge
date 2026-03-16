using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 供 AIBridge MCP 调用的专用编译接口。
    /// AIBridge 通过反射调用，程序集：Unity.Editor
    ///   Type:   ET.MCPBridgeCommand
    ///   Method: CompileHotfix()  →  returns JSON string
    /// </summary>
    public static class MCPBridgeCommand
    {
        /// <summary>
        /// 编译热更 DLL，等同于 F6 快捷键（AssemblyTool.DoCompile）。
        /// 返回 JSON 字符串，供 AIBridge 解析：
        /// {
        ///   "success": true/false,
        ///   "duration": 秒,
        ///   "errorCount": N,
        ///   "warningCount": N,
        ///   "errors": ["..."],        // 仅编译失败时存在
        ///   "exception": "..."        // 仅抛异常时存在
        /// }
        /// </summary>
        public static string CompileHotfix()
        {
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.Now;
            var errors = new List<string>();
            var warnings = new List<string>();
            string exceptionMsg = null;
            bool success = false;

            // 捕获编译期间的 Unity 日志
            Application.LogCallback logCallback = (condition, _, type) =>
            {
                switch (type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        errors.Add(condition);
                        break;
                    case LogType.Warning:
                        warnings.Add(condition);
                        break;
                }
            };
            Application.logMessageReceived += logCallback;

            try
            {
                AssemblyTool.DoCompile();
                // DoCompile 是同步阻塞调用，返回后通过 DLL 文件时间戳判断是否成功
                success = CheckDllsUpdated(startTime);
            }
            catch (Exception ex)
            {
                exceptionMsg = ex.Message;
                success = false;
            }
            finally
            {
                Application.logMessageReceived -= logCallback;
                stopwatch.Stop();
            }

            return BuildResultJson(success, stopwatch.Elapsed.TotalSeconds, errors, warnings, exceptionMsg);
        }

        /// <summary>
        /// 通过检查输出 DLL 的最后写入时间来判断编译是否成功
        /// </summary>
        private static bool CheckDllsUpdated(DateTime startTime)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var dllPath = Path.Combine(projectRoot, Define.CodeDir, "Unity.Hotfix.dll.bytes");
            if (!File.Exists(dllPath)) return false;
            // 允许 2 秒误差（时钟精度）
            return File.GetLastWriteTime(dllPath) >= startTime.AddSeconds(-2);
        }

        private static string BuildResultJson(bool success, double duration, List<string> errors, List<string> warnings, string exception)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            sb.Append($"\"success\":{(success ? "true" : "false")},");
            sb.Append($"\"duration\":{duration:F2},");
            sb.Append($"\"errorCount\":{errors.Count},");
            sb.Append($"\"warningCount\":{warnings.Count}");

            if (exception != null)
                sb.Append($",\"exception\":\"{EscapeJson(exception)}\"");

            if (errors.Count > 0)
            {
                sb.Append(",\"errors\":[");
                for (int i = 0; i < errors.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append($"\"{EscapeJson(errors[i])}\"");
                }
                sb.Append(']');
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }
    }
}
