using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetupSceneController : MonoBehaviour
{
    public InputField projectNameInput;
    public Dropdown kitDropdown;
    public Dropdown barDropdown;

    public void Create()
    {
        SessionData.projectName = projectNameInput.text;

        SessionData.kitName = kitDropdown.options[kitDropdown.value].text;

        SessionData.bars = int.Parse(barDropdown.options[barDropdown.value].text);


        SceneManager.LoadScene("DrumPadScene");
    }
}