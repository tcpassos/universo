using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using System.Collections;
using UnityEngine.Android;

public class SpawnActor : MonoBehaviour
{
    [SerializeField]
    AbstractMap _map;

    [SerializeField]
    float _spawnScale = 100f;

    [SerializeField]
    GameObject _markerPrefab;

    private GameObject _spawnedObject;
    private bool _locationInitialized = false;
    
    void InitializeMap() {
        if (!Input.location.isEnabledByUser) {
            Debug.LogError("Serviço de localização não está ativado pelo usuário.");
            return;
        }
        Input.location.Start();
        StartCoroutine(GetLocation());
    }
    void Start()
    {
        // Verifica se a permissão de localização foi concedida
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            Permission.RequestUserPermission(Permission.FineLocation);
        } else {
            InitializeMap();
        }

    }

    IEnumerator GetLocation() {

        while (Input.location.status == LocationServiceStatus.Initializing && Input.location.status != LocationServiceStatus.Failed) {
            Debug.Log("Inicializando serviço de localização...");
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status == LocationServiceStatus.Running) {
            // Location service is ready to use
            _locationInitialized = true;
            var location = Input.location.lastData;
            Vector2d currentLocation = new Vector2d(location.latitude, location.longitude);
            _spawnedObject = Instantiate(_markerPrefab);
            _spawnedObject.transform.localPosition = _map.GeoToWorldPosition(currentLocation, true);
            _spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            
        } else {
            Debug.LogError("Falha ao iniciar o serviço de localização: " + Input.location.status);
        }
    }

    private void Update()
        {
            if (_locationInitialized)
            {
                var location = Input.location.lastData;
                Vector2d currentLocation = new Vector2d(location.latitude, location.longitude);
                _spawnedObject.transform.localPosition = _map.GeoToWorldPosition(currentLocation, true);
                _spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            }
        }

}