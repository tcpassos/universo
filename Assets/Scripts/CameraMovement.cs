using UnityEngine;
using Mapbox.Unity.Map;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    AbstractMap _map;

    [SerializeField]
    float _zoomSpeed = 50f;

    [SerializeField]
    Camera _referenceCamera;

    Vector3 _origin;
    Vector3 _delta;
    bool _shouldDrag;

    void HandleTouch() {
        switch (Input.touchCount) {
            case 1:
                if (Input.GetTouch(0).phase == TouchPhase.Moved) {
                    HandleDrag();
                }
                break;
            case 2:
                HandlePinchToZoom();
                break;
            default:
                break;
        }
    }

    void HandleDrag() {
        var touch = Input.GetTouch(0);
        var touchPosition = new Vector3(touch.position.x, touch.position.y, _referenceCamera.transform.localPosition.y);
        _delta = _referenceCamera.ScreenToWorldPoint(touchPosition) - _referenceCamera.transform.localPosition;
        _delta.y = 0f;
        if (!_shouldDrag) {
            _shouldDrag = true;
            _origin = _referenceCamera.ScreenToWorldPoint(touchPosition);
        }
        var offset = _origin - _delta;
        offset.y = transform.localPosition.y;
        transform.localPosition = offset;
    }

    void HandlePinchToZoom() {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

        float zoomFactor = 0.05f * (touchDeltaMag - prevTouchDeltaMag);
        ZoomMap(zoomFactor);
    }

    void ZoomMap(float zoomFactor) {
        var y = zoomFactor * _zoomSpeed;
        transform.localPosition += transform.forward * y;
    }

    void Awake() {
        if (_referenceCamera == null) {
            _referenceCamera = GetComponent<Camera>();
            if (_referenceCamera == null) {
                throw new System.Exception("You must have a reference camera assigned!");
            }
        }

        if (_map == null) {
            _map = FindObjectOfType<AbstractMap>();
            if (_map == null) {
                throw new System.Exception("You must have a reference map assigned!");
            }
        }
    }

    void LateUpdate() {
        if (Input.touchSupported && Input.touchCount > 0) {
            Debug.Log("Touch count: " + Input.touchCount);
            HandleTouch();
        }
    }
}
