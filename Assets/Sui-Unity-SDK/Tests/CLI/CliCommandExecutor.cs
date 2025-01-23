using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Sui.Tests.CLI
{
    public class CliCommandExecutor
    {
        public class CommandResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string Error { get; set; }
        }

        public static CommandResult ExecuteCommand(string command, string args = "", IDictionary<string, string> environmentVariables = null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = GetCommandFileName(command),
                Arguments = GetCommandArguments(command, args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (environmentVariables != null)
            {
                foreach (var variable in environmentVariables)
                {
                    startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                }
            }

            using var process = new Process { StartInfo = startInfo };
            var result = new CommandResult();

            try
            {
                process.Start();
                result.Output = process.StandardOutput.ReadToEnd();
                result.Error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                result.ExitCode = process.ExitCode;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("PROCESS ERROR: " + ex.ToString());
                result.Error = ex.ToString();
                result.ExitCode = -1;
            }

            return result;
        }

        public static string GetCommandFileName(string command)
        {
            if (IsWindows())
            {
                return "cmd.exe";
            }
            return "/bin/bash";
        }

        public static string GetCommandArguments(string command, string args)
        {
            if (IsWindows())
            {
                return $"/c {command} {args}";
            }
            return $"-c \"{command} {args}\"";
        }

        private static bool IsWindows()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
    }

    public class SuiNodeTests
    {
        private Process suiNodeProcess;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            UnityEngine.Debug.Log("Starting Sui node setup...");

            // Check if Sui is installed
            var versionResult = CliCommandExecutor.ExecuteCommand("sui", "--version");
            UnityEngine.Debug.Log($"VERSION RESULT: {versionResult.Output}");
            if (versionResult.ExitCode != 0)
            {
                throw new Exception(
                    "Sui CLI is not installed or not in PATH. " +
                    "Please install Sui first (https://docs.sui.io/build/install)"
                );
            }

            UnityEngine.Debug.Log($"Detected Sui version: {versionResult.Output.Trim()}");

            var envVars = new Dictionary<string, string>
            {
                { "RUST_LOG", "off,sui_node=info" }
            };

            var startInfo = new ProcessStartInfo
            {
                FileName = CliCommandExecutor.GetCommandFileName("sui"),
                Arguments = CliCommandExecutor.GetCommandArguments("sui", "start --with-faucet --force-regenesis --with-indexer"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Application.dataPath + "/.."
            };

            foreach (var variable in envVars)
            {
                startInfo.EnvironmentVariables[variable.Key] = variable.Value;
            }

            UnityEngine.Debug.Log("Attempting to start Sui node...");
            suiNodeProcess = new Process { StartInfo = startInfo };
            suiNodeProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    UnityEngine.Debug.Log($"Sui Node Output: {e.Data}");
            };
            suiNodeProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    UnityEngine.Debug.LogError($"Sui Node Error: {e.Data}");
            };

            try
            {
                suiNodeProcess.Start();
                suiNodeProcess.BeginOutputReadLine();
                suiNodeProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start Sui node: {ex.Message}");
                throw;
            }

            // Wait for the process outside try-catch
            yield return new WaitForSeconds(5);

            if (suiNodeProcess.HasExited)
            {
                throw new Exception($"Sui node process exited prematurely with code: {suiNodeProcess.ExitCode}");
            }

            UnityEngine.Debug.Log("Sui node started successfully");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (suiNodeProcess != null && !suiNodeProcess.HasExited)
            {
                // Try graceful shutdown first
                var killResult = CliCommandExecutor.ExecuteCommand("sui", "node kill");

                yield return new WaitForSeconds(2);

                try
                {
                    // If process is still running, force kill it
                    if (!suiNodeProcess.HasExited)
                    {
                        UnityEngine.Debug.Log("Sui node didn't shut down gracefully, forcing termination...");
                        suiNodeProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error during Sui node cleanup: {ex.Message}");
                    try
                    {
                        suiNodeProcess.Kill();
                    }
                    catch (Exception killEx)
                    {
                        UnityEngine.Debug.LogError($"Failed to force kill Sui node: {killEx.Message}");
                    }
                }
                finally
                {
                    suiNodeProcess.Dispose();
                    suiNodeProcess = null;
                }
            }
        }
    }

    public class YourActualTests : SuiNodeTests
    {
        [UnityTest]
        public IEnumerator TestSuiNodeInteraction()
        {
            // Your test code here
            yield return null;
            Assert.Pass();
        }
    }
}