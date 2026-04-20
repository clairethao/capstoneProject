using UnityEngine;
using UnityEngine.UI;

public class RecordIndicatorUI : MonoBehaviour
{
    public Image indicatorImage;
    public Color recordingColor = Color.red;
    public Color idleColor = Color.grey;

    public void SetRecording(bool isRecording)
    {
        indicatorImage.color = isRecording ? recordingColor : idleColor;
    }
}