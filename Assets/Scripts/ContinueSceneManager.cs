using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class ContinueSceneManager : MonoBehaviour
{
    public static ContinueSceneManager Instance;
    public SessionDatabase sessionDatabase;
    public Transform contentParent;
    public GameObject projectRowPrefab;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(sessionDatabase.LoadSessions(OnSessionsLoaded));
    }

    private void OnSessionsLoaded(string json)
    {
        var root = JObject.Parse(json);
        var sessions = root["sessions"] as JArray;

        foreach (var s in sessions)
        {
            int id = (int)s["id"];
            string name = (string)s["project_name"];
            string kit = (string)s["kit_name"];
            int bars = (int)s["bars"];
            int bpm = (int)s["bpm"];

            GameObject row = Instantiate(projectRowPrefab, contentParent);
            row.GetComponent<ProjectRowUI>().Setup(id, name, kit, bars, bpm);
        }
    }

    public void LoadProject(int sessionId)
    {
        StartCoroutine(sessionDatabase.LoadSessionById(sessionId, (json) =>
        {
            var root = JObject.Parse(json);
            var s = root["session"];

            SessionData.projectName = (string)s["project_name"];
            SessionData.kitName = (string)s["kit_name"];
            SessionData.bars = (int)s["bars"];
            SessionData.bpm = (int)s["bpm"];

            UnityEngine.SceneManagement.SceneManager.LoadScene("DrumPadScene");
        }));
    }

    public void DeleteProject(int sessionId, ProjectRowUI row)
    {
        StartCoroutine(sessionDatabase.DeleteSession(sessionId, (success) =>
        {
            if (success)
            {
                Destroy(row.gameObject);
            }
            else
            {
                Debug.LogError("Failed to delete project");
            }
        }));
    }
}