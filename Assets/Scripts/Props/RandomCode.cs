using UnityEngine;

public class RandomCode : MonoBehaviour
{
    private int rawCode;
    private string formattedCode;

    private void Start()
    {
        rawCode = Random.Range(0, 10000);
        formattedCode = rawCode.ToString("D4");
        Debug.Log("4 Digit Code " + formattedCode);
    }
}
