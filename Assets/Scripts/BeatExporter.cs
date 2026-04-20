using System.Runtime.InteropServices;
using UnityEngine;

public static class BeatExporter
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PadHit
    {
        public int padId;
        public float time;
    }

    [DllImport("BeatRenderer")]
    private static extern void RenderBeat(
        [In] PadHit[] hits,
        int numHits,
        string outputPath,
        string kitFolder,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeConst = 12)]
    string[] fileNames,
        float bpm,
        int bars
    );

    public static void ExportBeat(
    PadHit[] hits,
    string outputPath,
    string kitFolderPath,
    string[] fileNames,
    float bpm,
    int bars
)
    {
        if (fileNames == null || fileNames.Length != 12)
        {
            Debug.LogError("fileNames must be an array of exactly 12 items.");
            return;
        }

        // Call into the native C++ plugin to write the WAV file
        RenderBeat(
            hits,
            hits.Length,
            outputPath,
            kitFolderPath,
            fileNames,
            bpm,
            bars
        );

        Debug.Log("Exported beat to: " + outputPath);

        // ⭐ Open the folder automatically
        Application.OpenURL("file://" + Application.persistentDataPath);
    }
}
