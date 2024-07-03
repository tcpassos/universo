using Mapbox.Directions;
using Mapbox.Geocoding;
using Mapbox.Unity;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DirectionsHandler : MonoBehaviour
{
    public AbstractMap map;
    public GameObject routePrefab;
    public GameObject destinationMarkerPrefab;
    public bool createRouteWithTouch = false;

    private Directions _directions;
    private Geocoder _geocoder;
    private GameObject _route;
    private GameObject _destinationMarker;
    private List<Vector2d> _routeGeometry;

    void Start() {
        _directions = MapboxAccess.Instance.Directions;
        _geocoder = MapboxAccess.Instance.Geocoder;
        InvokeRepeating(nameof(UpdateRoute), 0f, 1f); // Chamada do método UpdateRoute a cada 1 segundo
    }

    void Update() {
        if (createRouteWithTouch && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                Vector3 clickPosition = hit.point;
                Vector2d destinationLocation = map.WorldToGeoPosition(clickPosition);
                UpdateDestinationMarker(destinationLocation);
                RequestDirections(destinationLocation);
            }
        }
    }

    public void SetDestination(string locationDescription) {
        ForwardGeocodeResource forwardGeocodeResource = new ForwardGeocodeResource(locationDescription);

        _geocoder.Geocode(forwardGeocodeResource, (ForwardGeocodeResponse res) => {
            if (res == null || res.Features == null || res.Features.Count == 0) {
                Debug.LogError("Nenhum local encontrado para a descrição fornecida.");
                return;
            }

            Vector2d destinationLocation = res.Features[0].Center;
            UpdateDestinationMarker(destinationLocation);
            RequestDirections(destinationLocation);
        });
    }

    private void UpdateDestinationMarker(Vector2d destinationLocation) {
        if (_destinationMarker != null)
            Destroy(_destinationMarker);

        Vector3 destinationWorldPosition = map.GeoToWorldPosition(destinationLocation, true);
        if (destinationMarkerPrefab != null) {
            _destinationMarker = Instantiate(destinationMarkerPrefab, destinationWorldPosition, Quaternion.identity);
            _destinationMarker.transform.Rotate(Vector3.right, -90);
            _destinationMarker.transform.SetParent(map.transform, false);
        } else {
            Debug.LogError("Marcador de destino não atribuído.");
        }
    }

    private void RequestDirections(Vector2d destinationLocation) {
        Vector2d userLocation = map.CenterLatitudeLongitude;

        var waypoints = new List<Vector2d> {
            userLocation,
            destinationLocation
        };

        var directionResource = new DirectionResource(waypoints.ToArray(), RoutingProfile.Walking) {
            Steps = true
        };
        _directions.Query(directionResource, HandleDirectionsResponse);
    }

    void HandleDirectionsResponse(DirectionsResponse response) {
        if (response == null || response.Routes == null || response.Routes.Count < 1) {
            Debug.LogError("Nenhum caminho encontrado.");
            return;
        }

        var route = response.Routes[0];
        _routeGeometry = route.Geometry;

        if (_route != null) {
            Destroy(_route);
        }

        if (routePrefab != null) {
            _route = Instantiate(routePrefab);
            var lineRenderer = _route.GetComponent<LineRenderer>();
            lineRenderer.transform.SetParent(map.transform, false);
            lineRenderer.positionCount = _routeGeometry.Count;

            for (int i = 0; i < _routeGeometry.Count; i++) {
                Vector3 localPosition = map.GeoToWorldPosition(_routeGeometry[i], true);
                lineRenderer.SetPosition(i, localPosition);
            }
        } else {
            Debug.LogError("Prefab de rota não atribuído.");
        }
    }

    private void UpdateRoute() {
        if (_routeGeometry == null || _routeGeometry.Count < 2) {
            return;
        }

        Vector2d userLocation = map.CenterLatitudeLongitude;

        // Calcula a projeção da localização do usuário no segmento de linha entre os dois primeiros pontos da rota
        Vector2d point1 = _routeGeometry[0];
        Vector2d point2 = _routeGeometry[1];

        Vector2d projectedPoint = GetClosestPointOnSegment(point1, point2, userLocation);
        double distanceToProjectedPoint = Vector2d.Distance(userLocation, projectedPoint);

        double threshold = 3 / 111320.0; // 3 metros em graus

        // Atualiza o primeiro ponto da rota para o ponto projetado se a distância for menor que um limiar
        if (distanceToProjectedPoint < threshold) {
            _routeGeometry[0] = projectedPoint;
            if (Vector2d.Distance(_routeGeometry[0], _routeGeometry[1]) < threshold) {
                Debug.Log("Removendo ponto da rota: " + _routeGeometry[0].x + ", " + _routeGeometry[0].y);
                _routeGeometry.RemoveAt(0);
            }
        }

        // Se restar apenas um ponto, remove a rota
        if (_routeGeometry.Count == 1) {
            Destroy(_route);
            _route = null;
            _routeGeometry = null;
        } else {
            var lineRenderer = _route.GetComponent<LineRenderer>();
            lineRenderer.positionCount = _routeGeometry.Count;

            for (int i = 0; i < _routeGeometry.Count; i++) {
                Vector3 localPosition = map.GeoToWorldPosition(_routeGeometry[i], true);
                lineRenderer.SetPosition(i, localPosition);
            }
        }
    }

    private Vector2d GetClosestPointOnSegment(Vector2d A, Vector2d B, Vector2d P) {
        Vector2d AP = P - A;       // Vector A -> P
        Vector2d AB = B - A;       // Vector A -> B
        double ab2 = AB.x * AB.x + AB.y * AB.y;
        double ap_ab = AP.x * AB.x + AP.y * AB.y;
        double t = ap_ab / ab2;
        if (t < 0.0) t = 0.0;
        else if (t > 1.0) t = 1.0;
        return new Vector2d(A.x + AB.x * t, A.y + AB.y * t);
    }
}
