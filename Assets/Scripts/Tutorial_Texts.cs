using TMPro;
using UnityEngine;

public class Tutorial_Text : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    [TextArea(2, 5)]
    public string message;
    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        tutorialText.gameObject.SetActive(true);
        tutorialText.text = message;
            
    }
    private void OnTriggerExit(Collider other)
    {
        tutorialText.gameObject.SetActive(false);
    }

}
