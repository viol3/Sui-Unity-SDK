using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

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

        public static async Task<CommandResult> ExecuteCommandAsync(string command, string args = "", IDictionary<string, string> environmentVariables = null)
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
                result.Output = await process.StandardOutput.ReadToEndAsync();
                result.Error = await process.StandardError.ReadToEndAsync();

                // Compatible replacement for WaitForExitAsync
                while (!process.HasExited)
                {
                    await Task.Delay(100);
                }

                result.ExitCode = process.ExitCode;
            }
            catch (Exception ex)
            {
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

    [SetUpFixture]
    public class SuiNodeTests
    {
        private Process suiNodeProcess;

        private async Task CheckSuiInstallation()
        {
            try
            {
                var result = await CliCommandExecutor.ExecuteCommandAsync("sui", "--version");
                if (result.ExitCode != 0)
                {
                    throw new Exception($"Sui CLI check failed: {result.Error}");
                }
                UnityEngine.Debug.Log($"Detected Sui version: {result.Output.Trim()}");
            }
            catch (Exception ex) when (ex.ToString().Contains("not found") || ex.ToString().Contains("command not found"))
            {
                throw new Exception(
                    "Sui CLI is not installed or not in PATH. " +
                    "Please install Sui first (https://docs.sui.io/build/install)"
                );
            }
        }

        [OneTimeSetUp]
        protected async Task SetUp()
        {
            UnityEngine.Debug.Log("Starting Sui node setup...");

            // Check if Sui is installed first
            await CheckSuiInstallation();

            var envVars = new Dictionary<string, string>
            {
                { "RUST_LOG", "off,sui_node=info" }
            };

            // Create ProcessStartInfo for the long-running Sui process
            var startInfo = new ProcessStartInfo
            {
                FileName = CliCommandExecutor.GetCommandFileName("sui"),
                Arguments = CliCommandExecutor.GetCommandArguments("sui", "start --with-faucet --force-regenesis --with-indexer"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = UnityEngine.Application.dataPath + "/.." // Set to project root
            };

            foreach (var variable in envVars)
            {
                startInfo.EnvironmentVariables[variable.Key] = variable.Value;
            }

            // Start the process
            try
            {
                UnityEngine.Debug.Log("Attempting to start Sui node...");
                suiNodeProcess = new Process { StartInfo = startInfo };
                suiNodeProcess.Start();

                // Handle output asynchronously using events
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

                suiNodeProcess.BeginOutputReadLine();
                suiNodeProcess.BeginErrorReadLine();

                // Wait for the node to be ready using Unity's coroutine-like approach
                var startTime = DateTime.Now;
                while (!suiNodeProcess.HasExited && (DateTime.Now - startTime).TotalSeconds < 30) // 30 second timeout
                {
                    await Task.Delay(100);
                    // Allow Unity to process other tasks
                    await Task.Yield();
                }

                if (suiNodeProcess.HasExited)
                {
                    throw new Exception($"Sui node process exited prematurely with code: {suiNodeProcess.ExitCode}");
                }

                UnityEngine.Debug.Log("Sui node started successfully");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start Sui node: {ex.Message}");
                throw;
            }
        }

        [OneTimeTearDown]
        protected async Task TearDown()
        {
            if (suiNodeProcess != null && !suiNodeProcess.HasExited)
            {
                try
                {
                    // First try graceful shutdown
                    var killResult = await CliCommandExecutor.ExecuteCommandAsync("sui", "node kill");
                    await Task.Delay(2000); // Give it some time to shut down gracefully

                    // If process is still running, force kill it
                    if (!suiNodeProcess.HasExited)
                    {
                        TestContext.WriteLine("Sui node didn't shut down gracefully, forcing termination...");
                        suiNodeProcess.Kill(); // Simplified Kill call for compatibility
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Error during Sui node cleanup: {ex.Message}");
                    // Ensure process is killed even if graceful shutdown fails
                    try
                    {
                        suiNodeProcess.Kill();
                    }
                    catch (Exception killEx)
                    {
                        TestContext.WriteLine($"Failed to force kill Sui node: {killEx.Message}");
                    }
                }
                finally
                {
                    suiNodeProcess.Dispose();
                    suiNodeProcess = null;
                }
            }
        }

        [Test]
        protected void ExampleSuiTest()
        {
            // Test code here
            Assert.Pass();
        }
    }

    // Example of how to use the SuiNodeTests class
    [TestFixture]
    public class DummyTests : SuiNodeTests
    {
        [Test]
        public void TestSuiNodeInteraction()
        {
            // Sctual test code here
            Assert.Pass();
        }
    }
}