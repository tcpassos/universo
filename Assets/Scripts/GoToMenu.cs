using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMenu : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneSwitcher s1 = new SceneSwitcher();
            s1.LoadSceneByName("Menu");
        }
    }
}
