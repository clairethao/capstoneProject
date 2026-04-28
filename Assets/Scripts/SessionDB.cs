using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SessionDatabase : MonoBehaviour
{
    private const string SAVE_URL = "http://localhost/drumApp/saveSession.php";
    private const string GET_URL = "http://localhost/drumApp/getSessions.php";

    public IEnumerator SaveCurrentSession()
    {
        WWWForm form = new WWWForm();
        form.AddField("project_name", SessionData.projectName);
        form.AddField("kit_name", SessionData.kitName);
        form.AddField("bars", SessionData.bars);
        form.AddField("bpm", (int)SessionData.bpm);

        using (UnityWebRequest www = UnityWebRequest.Post(SAVE_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Save session failed: " + www.error);
            }
            else
            {
                Debug.Log("Save session response: " + www.downloadHandler.text);
            }
        }
    }

    public IEnumerator LoadSessions(System.Action<string> onResult)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(GET_URL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load sessions failed: " + www.error);
                onResult?.Invoke(null);
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("Load sessions response: " + json);
                onResult?.Invoke(json);
            }
        }
    }

    public IEnumerator LoadSessionById(int id, System.Action<string> onResult)
    {
        string url = "http://localhost/drumApp/getSessionByID.php?id=" + id;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                onResult?.Invoke(null);
            else
                onResult?.Invoke(www.downloadHandler.text);
        }
    }

    public IEnumerator DeleteSession(int id, System.Action<bool> onDone)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", id);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/drumApp/deleteSession.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Delete failed: " + www.error);
                onDone?.Invoke(false);
            }
            else
            {
                onDone?.Invoke(true);
            }
        }
    }
}