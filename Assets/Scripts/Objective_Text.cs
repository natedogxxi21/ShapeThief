using TMPro;
using UnityEngine;

public class Objective_Text : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public int requiredMoney = 500;
    public float FirstMessageTime = 5f;
    public float SecondMessageTime = 5f;

    private void Start()
    {
        objectiveText.text = "Objective: You need to collect $" + requiredMoney + " before you can leave";
            Invoke(nameof(ShowSecondMessage), FirstMessageTime);
    }

    void ShowSecondMessage()
    {
        objectiveText.text = "- Use left joy stick to move,\n " +
            "- Use right side of screen to move camera, \n" +
            "- Tap run button to move faster, \n" +
            "- Get the required amount of money to exit";

        Invoke(nameof(HideObjective), SecondMessageTime);
    }

    void HideObjective()
    {
        objectiveText.gameObject.SetActive(false);
    }
}
