using TMPro;
using UnityEngine;

public class Keypad : MonoBehaviour
{
    private string correctCode;
    private string enteredCode = "";
    public GameObject CodePad;

    [SerializeField] private TMP_Text display;
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        correctCode = FindAnyObjectByType<RandomCode>().GetCode();
        display.text = "Enter Code";
        messageText.text = "";
        Debug.Log("Correct code: [" + correctCode + "]");
    }

    public void PressNumber(string number)
    {
        if (enteredCode.Length >= 4)
            return;

        enteredCode += number;
        display.text = enteredCode;
        messageText.text = "";
    }

    public void ClearCode()
    {
        enteredCode = "";
        display.text = "Enter Code";
        messageText.text = "";
    }

    public void EnterCode()
    {
        if (enteredCode == correctCode)
        {
            messageText.text = "Access Granted!";
            CodePad.SetActive(false);

        }
        else
        {
            messageText.text = "Incorrect Code!";
        }

        enteredCode = "";
    }
}
