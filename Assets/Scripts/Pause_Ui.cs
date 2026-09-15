using UnityEngine;

public class Pause_Ui : MonoBehaviour
{
    public void OnEnable()
    {
        Time.timeScale = 0f;
        Debug.Log("GAME PAUSED - Time Scale: " + Time.timeScale);
    }


    public void CloseUpgradeScreen()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("GAME RESUMED - Time Scale: " + Time.timeScale);
    }
    public void Upgrade1()
    {
        //Upgrade Code will go here but for now it just closes ui!
        CloseUpgradeScreen();
    }
    public void Upgrade2()
    {
        //Upgrade Code will go here but for now it just closes ui!
        CloseUpgradeScreen();
    }
    public void Upgrade3()
    {
        //Upgrade Code will go here but for now it just closes ui!
        CloseUpgradeScreen();
    }
    public void PauseGame()
    {
        CloseUpgradeScreen();
    }
}
