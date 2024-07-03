using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using System.Collections;
using UnityEngine.Android;

public class SetupMap : MonoBehaviour
{
    public AbstractMap map;

    private DirectionsHandler _directionsHandler;

    void Start() {
        CheckLocationPermission();
    }

    void CheckLocationPermission() {
        // Verifica se a permissão de localização foi concedida
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            Permission.RequestUserPermission(Permission.FineLocation);
        } else {
            InitializeMap();
        }
    }

    void OnApplicationFocus(bool hasFocus) {
        // Verifica novamente a permissão quando o aplicativo ganhar foco
        if (hasFocus && Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            InitializeMap();
        }
    }

    void InitializeMap() {
        if (!Input.location.isEnabledByUser) {
            Debug.LogError("Serviço de localização não está ativado pelo usuário.");
            return;
        }
        Input.location.Start();
        StartCoroutine(GetLocation());
    }

    IEnumerator GetLocation() {
        // Espera até que o serviço de localização seja inicializado
        while (Input.location.status == LocationServiceStatus.Initializing && Input.location.status != LocationServiceStatus.Failed) {
            Debug.Log("Inicializando serviço de localização...");
            yield return new WaitForSeconds(1);
        }

        // Verifica o status do serviço de localização
        if (Input.location.status == LocationServiceStatus.Running) {
            Debug.Log("Serviço de localização iniciado com sucesso.");
            CreateRouteIfNecessary();

            // Atualiza o mapa com a localização atual
            while (true) {
                Vector2d userLocation = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);
                map.UpdateMap(userLocation, map.Zoom);
                yield return new WaitForSeconds(1); // Atualiza a cada segundo
            }
        } else {
            Debug.LogError("Falha ao iniciar o serviço de localização: " + Input.location.status);
        }
    }

    void CreateRouteIfNecessary() {
        _directionsHandler = FindObjectOfType<DirectionsHandler>();

        if (_directionsHandler == null) {
            Debug.LogError("DirectionsHandler não encontrado na cena.");
            return;
        }

        // Verifica se há uma descrição de local armazenada em PlayerPrefs
        if (PlayerPrefs.HasKey("LocationDescription")) {
            Debug.Log("Criando rota para o local armazenado em PlayerPrefs.");

            string locationDescription = PlayerPrefs.GetString("LocationDescription");
            // Limpa os dados de PlayerPrefs
            PlayerPrefs.DeleteKey("LocationDescription");
            // Chama a função SetDestination no DirectionsHandler
            _directionsHandler.SetDestination(locationDescription);
        }
    }
}
