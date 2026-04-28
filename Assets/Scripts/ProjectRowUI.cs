using UnityEngine;
using UnityEngine.UI;

public class ProjectRowUI : MonoBehaviour
{
    public Text projectNameText;
    public Text detailsText;
    public Button deleteButton;

    private int sessionId;

    public void Setup(int id, string projectName, string kitName, int bars, int bpm)
    {
        sessionId = id;
        projectNameText.text = projectName;
        detailsText.text = $"{kitName} • {bars} bars • {bpm} BPM";
    }

    public void OnClick()
    {
        ContinueSceneManager.Instance.LoadProject(sessionId);
    }

    public void OnDeletePressed()
    {
        ContinueSceneManager.Instance.DeleteProject(sessionId, this);
    }
}