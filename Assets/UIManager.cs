using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void BackButtonClick()
    {
        SceneManager.LoadScene("Menu");
    }
}
