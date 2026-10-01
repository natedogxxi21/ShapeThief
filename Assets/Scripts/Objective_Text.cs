using TMPro;
using UnityEngine;

public class Objective_Text : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public int requiredMoney = 500;
    public float displayTime = 5f;

   void Start()
    {
        objectiveText.text = "Objective: You need to collect $" + requiredMoney + " before you can leave" ;
            Invoke(nameof(HideObjective),displayTime);
    }

    void HideObjective()
    {
        objectiveText.gameObject.SetActive(false);
    }
}
