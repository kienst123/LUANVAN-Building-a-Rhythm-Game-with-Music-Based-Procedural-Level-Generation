using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Bridge để gọi Python ML script từ Unity
/// Xử lý subprocess, progress tracking, timeout, và error handling
/// </summary>
public class PythonMLBridge
{
    // Events để update UI
    public event Action<string, string> OnProgress;  // (stage, message)
    public event Action<string> OnComplete;          // (outputPath)
    public event Action<string> OnError;             // (errorMessage)
    
    // Configuration
    private const string PYTHON_COMMAND = "python3";
    private const float DEFAULT_TIMEOUT = 120f; // 2 phút
    private const int BUFFER_SIZE = 4096;
    
    // State
    private Process currentProcess;
    private bool isCancelled;
    private StringBuilder outputBuffer;
    private StringBuilder errorBuffer;
    
    /// <summary>
    /// Generate beatmap từ audio file sử dụng ML model
    /// </summary>
    /// <param name="audioPath">Đường dẫn file MP3/OGG/WAV</param>
    /// <param name="difficulty">Độ khó (0.0-1.0)</param>
    /// <param name="outputPath">Đường dẫn output .osu file</param>
    /// <param name="timeout">Timeout trong giây (default: 120s)</param>
    /// <returns>IEnumerator để chạy trong coroutine</returns>
    public IEnumerator GenerateBeatmap(
        string audioPath, 
        float difficulty, 
        string outputPath,
        float timeout = DEFAULT_TIMEOUT)
    {
        // Reset state
        isCancelled = false;
        outputBuffer = new StringBuilder();
        errorBuffer = new StringBuilder();
        
        // Validate inputs
        if (!File.Exists(audioPath))
        {
            string error = $"Audio file not found: {audioPath}";
            Debug.LogError($"[PythonMLBridge] {error}");
            OnError?.Invoke(error);
            yield break;
        }
        
        if (difficulty < 0f || difficulty > 1f)
        {
            string error = $"Difficulty must be between 0.0 and 1.0, got {difficulty}";
            Debug.LogError($"[PythonMLBridge] {error}");
            OnError?.Invoke(error);
            yield break;
        }
        
        // Get Python script path
        string scriptPath = GetPythonScriptPath();
        if (!File.Exists(scriptPath))
        {
            string error = $"Python script not found: {scriptPath}";
            Debug.LogError($"[PythonMLBridge] {error}");
            OnError?.Invoke(error);
            yield break;
        }
        
        // Create output directory
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        
        Debug.Log($"[PythonMLBridge] Starting ML generation...");
        Debug.Log($"  Audio: {audioPath}");
        Debug.Log($"  Difficulty: {difficulty:F2}");
        Debug.Log($"  Output: {outputPath}");
        
        // Setup process
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = PYTHON_COMMAND,
            Arguments = $"\"{scriptPath}\" \"{audioPath}\" {difficulty:F2} \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(scriptPath)
        };
        
        // Start process
        try
        {
            currentProcess = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            
            // Setup output handlers
            currentProcess.OutputDataReceived += OnOutputDataReceived;
            currentProcess.ErrorDataReceived += OnErrorDataReceived;
            
            currentProcess.Start();
            currentProcess.BeginOutputReadLine();
            currentProcess.BeginErrorReadLine();
            
            Debug.Log($"[PythonMLBridge] Process started (PID: {currentProcess.Id})");
        }
        catch (Exception e)
        {
            string error = $"Failed to start Python process: {e.Message}";
            Debug.LogError($"[PythonMLBridge] {error}");
            OnError?.Invoke(error);
            yield break;
        }
        
        // Wait for completion with timeout
        float elapsed = 0f;
        while (!currentProcess.HasExited && elapsed < timeout && !isCancelled)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            
            // Progress update (could be improved with actual progress from Python)
            float progress = Mathf.Clamp01(elapsed / timeout);
            Debug.Log($"[PythonMLBridge] Progress: {progress * 100:F0}% ({elapsed:F1}s)");
        }
        
        // Handle timeout
        if (elapsed >= timeout && !currentProcess.HasExited)
        {
            Debug.LogWarning($"[PythonMLBridge] Generation timeout after {timeout}s");
            KillProcess();
            OnError?.Invoke($"Generation timeout after {timeout} seconds");
            yield break;
        }
        
        // Handle cancellation
        if (isCancelled)
        {
            Debug.Log($"[PythonMLBridge] Generation cancelled by user");
            KillProcess();
            OnError?.Invoke("Generation cancelled");
            yield break;
        }
        
        // Wait a bit for buffers to flush
        yield return new WaitForSeconds(0.5f);
        
        // Check exit code
        int exitCode = currentProcess.ExitCode;
        Debug.Log($"[PythonMLBridge] Process exited with code: {exitCode}");
        
        if (exitCode == 0)
        {
            // Success - verify output file
            if (File.Exists(outputPath))
            {
                long fileSize = new FileInfo(outputPath).Length;
                Debug.Log($"[PythonMLBridge] ✅ Success! Generated beatmap ({fileSize} bytes)");
                OnComplete?.Invoke(outputPath);
            }
            else
            {
                string error = "Generation reported success but output file not found";
                Debug.LogError($"[PythonMLBridge] {error}");
                OnError?.Invoke(error);
            }
        }
        else
        {
            // Error
            string errorMsg = errorBuffer.ToString();
            if (string.IsNullOrEmpty(errorMsg))
            {
                errorMsg = $"Python script failed with exit code {exitCode}";
            }
            
            Debug.LogError($"[PythonMLBridge] ❌ Error: {errorMsg}");
            OnError?.Invoke(errorMsg);
        }
        
        // Cleanup
        CleanupProcess();
    }
    
    /// <summary>
    /// Cancel đang generate
    /// </summary>
    public void Cancel()
    {
        Debug.Log($"[PythonMLBridge] Cancelling generation...");
        isCancelled = true;
        KillProcess();
    }
    
    /// <summary>
    /// Handle stdout từ Python
    /// </summary>
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data)) return;
        
        outputBuffer.AppendLine(e.Data);
        
        // Parse progress messages (format: "PROGRESS:{json}")
        if (e.Data.StartsWith("PROGRESS:"))
        {
            try
            {
                string json = e.Data.Substring("PROGRESS:".Length);
                // Simple parsing (có thể dùng JsonUtility nếu cần)
                Debug.Log($"[PythonML] {e.Data}");
                
                // Extract stage and message
                if (json.Contains("stage") && json.Contains("message"))
                {
                    // Basic extraction (not robust, but works for simple cases)
                    int stageStart = json.IndexOf("\"stage\":\"") + 9;
                    int stageEnd = json.IndexOf("\"", stageStart);
                    string stage = json.Substring(stageStart, stageEnd - stageStart);
                    
                    int msgStart = json.IndexOf("\"message\":\"") + 11;
                    int msgEnd = json.IndexOf("\"", msgStart);
                    string message = json.Substring(msgStart, msgEnd - msgStart);
                    
                    OnProgress?.Invoke(stage, message);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PythonMLBridge] Failed to parse progress: {ex.Message}");
            }
        }
        // Parse success message
        else if (e.Data.StartsWith("SUCCESS:"))
        {
            string path = e.Data.Substring("SUCCESS:".Length);
            Debug.Log($"[PythonML] Success: {path}");
        }
        else
        {
            // Regular output
            Debug.Log($"[PythonML] {e.Data}");
        }
    }
    
    /// <summary>
    /// Handle stderr từ Python
    /// </summary>
    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data)) return;
        
        errorBuffer.AppendLine(e.Data);
        Debug.LogWarning($"[PythonML] {e.Data}");
    }
    
    /// <summary>
    /// Get đường dẫn tới Python script
    /// </summary>
    private string GetPythonScriptPath()
    {
        // Try multiple locations
        string[] possiblePaths = new string[]
        {
            // StreamingAssets (recommended for builds)
            Path.Combine(Application.streamingAssetsPath, "MLScripts", "generate_beatmap.py"),
            
            // Project directory (for Editor)
            Path.Combine(Application.dataPath, "..", "MLScripts", "generate_beatmap.py"),
            
            // Next to executable (for builds)
            Path.Combine(Application.dataPath, "MLScripts", "generate_beatmap.py"),
        };
        
        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                Debug.Log($"[PythonMLBridge] Found script at: {path}");
                return path;
            }
        }
        
        // Default to StreamingAssets
        string defaultPath = Path.Combine(Application.streamingAssetsPath, "MLScripts", "generate_beatmap.py");
        Debug.LogWarning($"[PythonMLBridge] Script not found, using default path: {defaultPath}");
        return defaultPath;
    }
    
    /// <summary>
    /// Kill process nếu đang chạy
    /// </summary>
    private void KillProcess()
    {
        if (currentProcess != null && !currentProcess.HasExited)
        {
            try
            {
                currentProcess.Kill();
                Debug.Log($"[PythonMLBridge] Process killed");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PythonMLBridge] Failed to kill process: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Cleanup process resources
    /// </summary>
    private void CleanupProcess()
    {
        if (currentProcess != null)
        {
            currentProcess.OutputDataReceived -= OnOutputDataReceived;
            currentProcess.ErrorDataReceived -= OnErrorDataReceived;
            currentProcess.Dispose();
            currentProcess = null;
        }
    }
}

/// <summary>
/// Progress data từ Python script
/// </summary>
[Serializable]
public class MLGenerationProgress
{
    public string stage;
    public string message;
    public double timestamp;
}
