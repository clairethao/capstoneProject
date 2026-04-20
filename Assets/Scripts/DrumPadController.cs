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
        isRecording = true;
        recordIndicatorUI.SetRecording(true);
    }

    public void ClearRecording()
    {
        recordedHits.Clear();
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

    private string GetUniqueExportPath()
    {
        string folder = Application.persistentDataPath;
        string baseName = "exportedBeat";
        string extension = ".wav";

        string path = System.IO.Path.Combine(folder, baseName + extension);

        int counter = 1;
        while (System.IO.File.Exists(path))
        {
            path = System.IO.Path.Combine(folder, baseName + counter + extension);
            counter++;
        }

        return path;
    }

    public void OnExportButtonPressed()
    {
        // Convert recorded hits to array
        BeatExporter.PadHit[] hitsArray = recordedHits
            .Select(h => new BeatExporter.PadHit { padId = h.padId, time = h.time })
            .ToArray();

        // Choose your kit folder (this is the only thing you change when switching kits)
        string kitFolderPath = Application.dataPath + "/Audio/DrumSamples/hiphopSamples";

        string[] fileNames = new string[]
        {
        "pad0.wav", "pad1.wav", "pad2.wav", "pad3.wav",
        "pad4.wav", "pad5.wav", "pad6.wav", "pad7.wav",
        "pad8.wav", "pad9.wav", "pad10.wav", "pad11.wav"
        };

        string outputPath = GetUniqueExportPath();

        BeatExporter.ExportBeat(
            hitsArray,
            outputPath,
            kitFolderPath,
            fileNames,
            bpmController.bpm,
            bars
        );
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