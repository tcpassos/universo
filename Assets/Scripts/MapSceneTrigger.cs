using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSceneTrigger : MonoBehaviour
{
    public string mapSceneName = "Map";

    public void OpenMapSceneWithLocation(string locationDescription) {
        // Armazena a descrição do local em PlayerPrefs
        PlayerPrefs.SetString("LocationDescription", locationDescription);
        PlayerPrefs.Save();

        // Carrega a cena do mapa
        SceneManager.LoadScene(mapSceneName);
    }
}
