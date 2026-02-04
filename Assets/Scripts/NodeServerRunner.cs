using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NodeServerRunner : MonoBehaviour
{
    public static NodeServerRunner Instance { get; private set; }

    [SerializeField] private int port = 3000;

    private Process proc;
    private bool startedByUs;

    public int Port => port;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task EnsureServerRunning()
    {
        // If something is already answering /health, do nothing
        if (await HealthCheck()) return;

        StartServerProcess();

        // Wait until it is actually ready before Unity tries to connect
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (await HealthCheck()) return;
            await Task.Delay(200);
        }

        UnityEngine.Debug.LogError("Node server did not become ready in time.");
    }

    private void StartServerProcess()
    {
        string root = GetRootFolder();
        string serverDir = Path.Combine(root, "server");

        // Ship node.exe in server/
        string nodeExe = Path.Combine(serverDir, "node.exe");
        string serverJs = Path.Combine(serverDir, "server.js");

        if (!File.Exists(nodeExe))
        {
            UnityEngine.Debug.LogError($"node.exe not found at: {nodeExe}");
            return;
        }
        if (!File.Exists(serverJs))
        {
            UnityEngine.Debug.LogError($"server.js not found at: {serverJs}");
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = nodeExe,
            Arguments = $"\"{serverJs}\" --port {port}",
            WorkingDirectory = serverDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.Log("[node] " + e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.LogError("[node] " + e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        startedByUs = true;
        UnityEngine.Debug.Log("Started Node server process.");
    }

    private async Task<bool> HealthCheck()
    {
        try
        {
            var req = WebRequest.Create($"http://127.0.0.1:{port}/health");
            req.Timeout = 500;
            using var resp = await req.GetResponseAsync();
            return true;
        }
        catch { return false; }
    }

    private static string GetRootFolder()
    {
        // In Editor: .../Project/Assets -> parent is .../Project
        // In Build:  .../Build/MyGame_Data -> parent is .../Build
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    void OnApplicationQuit() => StopServer();
    void OnDestroy() => StopServer();
    void OnDisable() => StopServer();

    private void StopServer()
    {
        if (!startedByUs) return;

        try
        {
            if (proc != null && !proc.HasExited)
            {
                proc.Kill();
                UnityEngine.Debug.Log("Stopped Node server process.");
            }
        }
        catch { /* ignore */ }
        finally
        {
            proc = null;
            startedByUs = false;
        }
    }
}
