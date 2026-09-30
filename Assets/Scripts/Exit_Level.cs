using UnityEngine;
using UnityEngine.SceneManagement;
public class Exit_Level : MonoBehaviour
{
    [SerializeField] int requiredMoney = 10;
    [SerializeField] string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the exit pad!");
        Debug.Log("Tag: " + other.gameObject.tag);
       
        if (!other.transform.root.CompareTag("Player"))
        return;

        if (PlayerStats.Money >= requiredMoney)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("You need" + requiredMoney + " money to exit");
        }
    }
}