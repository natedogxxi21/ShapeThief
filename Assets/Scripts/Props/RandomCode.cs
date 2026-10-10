using UnityEngine;

public class RandomCode : MonoBehaviour
{
    private int rawCode;
    private string formattedCode;

    private void Awake()
    {
        rawCode = Random.Range(0, 10000);
        formattedCode = rawCode.ToString("D4");
        Debug.Log("4 Digit Code " + formattedCode);
    }

    public string GetCode()
    {
        return formattedCode;
    }
}
