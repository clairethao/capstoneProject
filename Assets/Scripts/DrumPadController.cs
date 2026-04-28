using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DrumPadController : MonoBehaviour
{
    public List<Pad> pads;
    public List<PadHit> recordedHits = new List<PadHit>();
    public BPMController bpmController;
    public UnityEngine.UI.Image loopProgressBar;
    public RecordIndicatorUI recordIndicatorUI;
    public int bars;
    public float loopLength;
    private float loopPosition = 0f;
    public SessionDatabase sessionDatabase;
    public SavedPopupUI savedPopupUI;

    public bool isRecording = false;
    private bool isLooping = false;
 
    [System.Serializable]
    public class PadHit
    {
        public int padId;
        public float time;
        public int orderIndex;

        public PadHit(int id, float t, int index)
        {
            padId = id;
            time = t;
            orderIndex = index;
        }
    }

    void Start()
    {
        if (SessionData.loadedHits != null)
            recordedHits = SessionData.loadedHits;
        else
            recordedHits = new List<PadHit>();
        
        LoadKit(SessionData.kitName);
        
        if (SessionData.bpm <= 0)
            SessionData.bpm = 120;

        bpmController.SetBPM(SessionData.bpm);
        bars = SessionData.bars;
    }

    void Update()
    {
        float bpm = bpmController.bpm;
        loopLength = (60f / bpm) * 4f * bars;

        if (isLooping || isRecording)
        {
            loopPosition += Time.deltaTime;

            if (loopPosition > loopLength)
                loopPosition -= loopLength;
        }

        if (loopProgressBar != null && loopLength > 0f)
        {
            loopProgressBar.fillAmount = loopPosition / loopLength;
        }
    }

    public void StartRecording()
    {
        isRecording = true;
        loopPosition = 0f;
        recordIndicatorUI.SetRecording(true);
    }

    public void ClearRecording()
    {
        recordedHits.Clear();
        loopPosition = 0f;
        isLooping = false;

        if (loopProgressBar != null)
            loopProgressBar.fillAmount = 0f;
    }

    public void StopAll()
    {
        StopAllCoroutines();
        isRecording = false;
        isLooping = false;
        recordIndicatorUI.SetRecording(false);
    }

    public void PlayLoop()
    {
        StopAllCoroutines();
        loopPosition = 0f;
        isLooping = true;
        StartCoroutine(LoopCoroutine());
    }

    private IEnumerator LoopCoroutine()
    {
        while (true)
        {
            recordedHits.Sort((a, b) =>
            {
                int timeCompare = a.time.CompareTo(b.time);
                if (timeCompare != 0)
                    return timeCompare;

                return a.orderIndex.CompareTo(b.orderIndex);
            });

            var hitsSnapshot = new List<PadHit>(recordedHits);

            float loopStart = Time.time;

            foreach (PadHit hit in hitsSnapshot)
            {
                Debug.Log($"[Playback] pad {hit.padId}, time={hit.time}, orderIndex={hit.orderIndex}");
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

    private string GetUniqueExportPath(string projName)
    {
        string folder = Application.persistentDataPath;
        string extension = ".wav";

        if (string.IsNullOrWhiteSpace(projName))
            projName = "exportedBeat";

        string path = Path.Combine(folder, projName + extension);

        int counter = 1;
        while (File.Exists(path))
        {
            path = Path.Combine(folder, projName + counter + extension);
            counter++;
        }

        return path;
    }

    public void OnExportButtonPressed()
    {
        BeatExporter.PadHit[] hitsArray = recordedHits
            .Select(h => new BeatExporter.PadHit { padId = h.padId, time = h.time })
            .ToArray();

        string kitFolderPath = Application.dataPath + "/Resources/DrumSamples/" + SessionData.kitName;

        string[] fileNames = new string[]
        {
        "pad0.wav", "pad1.wav", "pad2.wav", "pad3.wav",
        "pad4.wav", "pad5.wav", "pad6.wav", "pad7.wav",
        "pad8.wav", "pad9.wav", "pad10.wav", "pad11.wav"
        };

        string outputPath = GetUniqueExportPath(SessionData.projectName);

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
                int index = recordedHits.Count;

                recordedHits.Add(new PadHit(padId, t, index));

                Debug.Log($"[Recorded] pad {padId} at time {t}, orderIndex={index}");
            }
        }
    }

    private void LoadSelectedKit()
    {
        string kitName = SessionData.kitName;
        LoadKit(kitName);
    }

    private void LoadKit(string kitName)
    {
        for (int i = 0; i < pads.Count; i++)
        {
            AudioClip clip = Resources.Load<AudioClip>($"DrumSamples/{kitName}/pad{i}");

            if (clip != null)
                pads[i].SetClip(clip);
            else
                Debug.LogWarning($"Missing sample: DrumSamples/{kitName}/pad{i}.wav");
        }
    }
    public void OnSaveButtonPressed()
    {
        StartCoroutine(SaveEverything());
    }

    private IEnumerator SaveEverything()
    {
        Debug.Log("Saving project:");
        Debug.Log("projectName = " + SessionData.projectName);
        Debug.Log("kitName = " + SessionData.kitName);
        Debug.Log("bars = " + SessionData.bars);
        Debug.Log("bpm = " + SessionData.bpm);

        SessionData.bpm = (int)bpmController.bpm;

        yield return StartCoroutine(sessionDatabase.SaveCurrentSession());

        int sessionId = SessionData.sessionId;

        foreach (var hit in recordedHits)
        {
            yield return StartCoroutine(sessionDatabase.SaveHit(sessionId, hit));
        }

        yield return StartCoroutine(savedPopupUI.ShowPopup());
    }

}