using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DrumPadController : MonoBehaviour
{
    public List<Pad> pads;
    public BPMController bpmController;
    public bool isRecording = false;
    public RecordIndicatorUI recordIndicatorUI;
    public float recordStartTime;
    public List<PadHit> recordedHits = new List<PadHit>();
    public int bars = 2;
    public float loopLength;
    private float loopPosition = 0f;


    [System.Serializable]
    public class PadHit
    {
        public int padId;
        public float time;

        public PadHit(int id, float t)
        {
            padId = id;
            time = t;
        }
    }

    void Update()
    {
        float bpm = bpmController.bpm;
        loopLength = (60f / bpm) * 4f * bars;

        loopPosition += Time.deltaTime;

        if (loopPosition > loopLength)
            loopPosition -= loopLength;
    }

    public void StartRecording()
    {
        recordedHits.Clear();
        loopPosition = 0f;
        isRecording = true;
        recordIndicatorUI.SetRecording(true);
    }

    public void StopAll()
    {
        StopAllCoroutines();
        isRecording = false;
        recordIndicatorUI.SetRecording(false);
    }

    public void PlayLoop()
    {
        StopAllCoroutines();
        StartCoroutine(LoopCoroutine());
    }

    private IEnumerator LoopCoroutine()
    {
        while (true)
        {
            recordedHits.Sort((a, b) => a.time.CompareTo(b.time));
            var hitsSnapshot = new List<PadHit>(recordedHits);

            float loopStart = Time.time;

            foreach (PadHit hit in hitsSnapshot)
            {
                float targetTime = loopStart + hit.time;
                float waitTime = targetTime - Time.time;

                if (waitTime > 0)
                    yield return new WaitForSeconds(waitTime);

                TriggerPad(hit.padId);
            }

            float loopEnd = loopStart + loopLength;
            float remaining = loopEnd - Time.time;

            if (remaining > 0)
                yield return new WaitForSeconds(remaining);
        }
    }

    public void PlayRecording()
    {
        StopAllCoroutines();
        StartCoroutine(PlaybackCoroutine());
    }

    private System.Collections.IEnumerator PlaybackCoroutine()
    {
        if (recordedHits.Count == 0)
            yield break;

        float bpm = bpmController.bpm;
        float speedMultiplier = 120f / bpm;

        float startTime = Time.time;

        foreach (PadHit hit in recordedHits)
        {
            float adjustedTime = hit.time * speedMultiplier;
            float waitTime = (startTime + adjustedTime) - Time.time;

            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            TriggerPad(hit.padId);
        }
    }
    public void OnExportButtonPressed()
    {
        // Convert recorded hits to array
        BeatExporter.PadHit[] hitsArray = recordedHits
            .Select(h => new BeatExporter.PadHit { padId = h.padId, time = h.time })
            .ToArray();

        foreach (var h in hitsArray)
            Debug.Log($"Hit pad {h.padId} at time {h.time}");

        // Correct kit folder path
        string kitFolderPath = Application.dataPath + "/Audio/DrumSamples/hiphopSamples";

        // Auto-load filenames from the folder
        string[] fileNames = Directory.GetFiles(kitFolderPath, "*.wav")
                                      .Select(Path.GetFileName)
                                      .Take(12)
                                      .ToArray();

        // Output path
        string outputPath = Application.persistentDataPath + "/exportedBeat.wav";

        // Export
        BeatExporter.ExportBeat(
            hitsArray,
            outputPath,
            kitFolderPath,
            fileNames,
            bpmController.bpm,
            bars
        );

        Debug.Log("Export button pressed — exporting beat...");
    }

    public void TriggerPad(int padId)
    {
        if (padId >= 0 && padId < pads.Count)
        {
            pads[padId].PlaySound();

            if (isRecording)
            {
                float t = loopPosition;
                recordedHits.Add(new PadHit(padId, t));
            }
        }
    }
}