using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInput : MonoBehaviour
{
    public static string name;

    public void Click()
    {
        Application.runInBackground = true;

        TMP_InputField input = GameObject.FindWithTag("MenuInput").GetComponent<TMP_InputField>();

        name = input.text;

        SceneManager.LoadScene("Game");
    }
}
