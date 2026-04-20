using UnityEngine;
using UnityEngine.UI;

public class BPMController : MonoBehaviour
{
    public Slider bpmSlider;
    public Text bpmText;
    public float bpm;

    void Start()
    {
        bpmSlider.minValue = 10;
        bpmSlider.maxValue = 235;
        bpmSlider.wholeNumbers = true;

        bpmSlider.value = 120;
    }

    void Update()
    {
        bpm = bpmSlider.value;
        bpmText.text = bpm.ToString();
    }
}