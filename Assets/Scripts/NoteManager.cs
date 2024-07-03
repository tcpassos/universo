using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class NoteManager : MonoBehaviour
{
    public GameObject note3DPrefab;
    public GameObject noteWidgetPrefab;
    public GameObject locationInputPanelPrefab;
    public ARTrackedImageManager trackedImageManager;

    private Transform _boardTransform;
    private GameObject _currentNote;
    private GameObject _activeNoteWidget;
    private GameObject _locationInputPanel;
    private DatabaseManager _databaseManager;
    private bool _notesLoaded = false;
    private int _currentBoardId;

    void Start() {
        if (!ValidatePrefabs()) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) {
            Debug.LogError("Canvas não encontrado na cena.");
            return;
        }

        _activeNoteWidget = Instantiate(noteWidgetPrefab, canvas.transform);
        _locationInputPanel = Instantiate(locationInputPanelPrefab, canvas.transform);
        InitializeWidgets();

        _databaseManager = new DatabaseManager("universo");
        if (trackedImageManager.referenceLibrary != null) {
            for (int i = 0; i < trackedImageManager.referenceLibrary.count; i++) {
                var referenceImage = trackedImageManager.referenceLibrary[i];
                var board = new BoardEntity { Id = i + 1, Name = referenceImage.name };
                _databaseManager.VirtualBoards.Save(board);
            }
        } else {
            Debug.LogError("Biblioteca de referência não está atribuída.");
        }

        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDestroy() {
        if (trackedImageManager != null) {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    void Update() {
        if (_activeNoteWidget.activeSelf || _locationInputPanel.activeSelf) return;

        if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true) {
            ProcessTouch(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private bool ValidatePrefabs() {
        if (note3DPrefab == null || noteWidgetPrefab == null || locationInputPanelPrefab == null || trackedImageManager == null) {
            Debug.LogError("Um ou mais prefabs ou ARTrackedImageManager não estão atribuídos.");
            return false;
        }
        return true;
    }

    private void InitializeWidgets() {
        if (_activeNoteWidget == null || _locationInputPanel == null) {
            Debug.LogError("Falha ao instanciar os widgets.");
            return;
        }
        _activeNoteWidget.SetActive(false);
        _locationInputPanel.SetActive(false);
    }

    private void ProcessTouch(Vector2 touchPosition) {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.collider.CompareTag("Note")) {
                OpenEditWidget(hit.collider.gameObject);
            }
            else if (hit.transform == _boardTransform) {
                CreateNewNoteAtPosition(hit.point);
            }
        }
    }

    private void CreateNewNoteAtPosition(Vector3 position) {
        _currentNote = Instantiate(note3DPrefab, position, _boardTransform.rotation, _boardTransform);
        _currentNote.tag = "Note";

        var noteEntity = new NoteEntity {
            Content = "",
            X = _currentNote.transform.localPosition.x,
            Y = _currentNote.transform.localPosition.y,
            Z = _currentNote.transform.localPosition.z,
            VirtualBoardId = _currentBoardId
        };
        _databaseManager.Notes.Save(noteEntity);

        var noteComponent = _currentNote.AddComponent<NoteComponent>();
        noteComponent.noteEntity = noteEntity;

        OpenEditWidget(_currentNote);
    }

    private void OpenEditWidget(GameObject note) {
        _currentNote = note;
        _activeNoteWidget.SetActive(true);
        var tmpInputField = _activeNoteWidget.GetComponentInChildren<TMP_InputField>();
        var tmpText = _currentNote.GetComponentInChildren<TextMeshPro>();
        tmpInputField.text = tmpText != null ? tmpText.text : string.Empty;

        SetUpButton("SaveButton", SaveNoteContent);
        SetUpButton("CancelButton", CancelEdit);
        SetUpButton("DeleteButton", DeleteNote);
        SetUpButton("NewLocationButton", OpenLocationInputPanel);
        SetUpButton("LocationButton", OpenMapWithRoute);
    }

    private void SetUpButton(string buttonName, UnityEngine.Events.UnityAction action) {
        var button = _activeNoteWidget.transform.Find(buttonName).GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void SaveNoteContent() {
        var tmpInputField = _activeNoteWidget.GetComponentInChildren<TMP_InputField>();
        var tmpText = _currentNote.GetComponentInChildren<TextMeshPro>();

        if (tmpText != null) tmpText.text = tmpInputField.text;

        var noteComponent = _currentNote.GetComponent<NoteComponent>();
        if (noteComponent != null) {
            noteComponent.noteEntity.Content = tmpInputField.text;
            var locationInputField = _locationInputPanel.GetComponentInChildren<TMP_InputField>();
            noteComponent.noteEntity.Location = !string.IsNullOrEmpty(locationInputField.text) ? locationInputField.text : null;
            UpdateLocationButtonState(noteComponent.noteEntity.Location != null);
            _databaseManager.Notes.Update(noteComponent.noteEntity);
        }
        _activeNoteWidget.SetActive(false);
    }

    private void CancelEdit() {
        _activeNoteWidget.SetActive(false);
    }

    private void DeleteNote() {
        if (_currentNote != null) {
            DeleteNoteFromDatabase(_currentNote);
            Destroy(_currentNote);
            _activeNoteWidget.SetActive(false);
        }
    }

    private void OpenLocationInputPanel() {
        _activeNoteWidget.SetActive(false);
        _locationInputPanel.SetActive(true);

        var locationInputField = _locationInputPanel.GetComponentInChildren<TMP_InputField>();
        var locationSaveButton = _locationInputPanel.transform.Find("LocationSaveButton").GetComponent<Button>();
        locationSaveButton.onClick.RemoveAllListeners();
        locationSaveButton.onClick.AddListener(() => {
            if (!string.IsNullOrEmpty(locationInputField.text)) {
                locationInputField.text += ", Unisinos, São Leopoldo - RS";
                UpdateLocationButtonState(true);
            } else {
                UpdateLocationButtonState(false);
            }

            _locationInputPanel.SetActive(false);
            _activeNoteWidget.SetActive(true);
        });

        var locationCancelButton = _locationInputPanel.transform.Find("LocationCancelButton").GetComponent<Button>();
        locationCancelButton.onClick.RemoveAllListeners();
        locationCancelButton.onClick.AddListener(() => {
            _locationInputPanel.SetActive(false);
            _activeNoteWidget.SetActive(true);
        });
    }

    private void OpenMapWithRoute() {
        var locationInputField = _locationInputPanel.GetComponentInChildren<TMP_InputField>();
        if (locationInputField != null && !string.IsNullOrEmpty(locationInputField.text)) {
            PlayerPrefs.SetString("LocationDescription", locationInputField.text);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Map");
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var trackedImage in eventArgs.added) {
            UpdateBoardTransform(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated) {
            UpdateBoardTransform(trackedImage);
            LoadNotes();
        }
    }

    private void UpdateBoardTransform(ARTrackedImage trackedImage) {
        if (trackedImage.trackingState == TrackingState.Tracking) {
            _boardTransform = trackedImage.transform;

            for (int i = 0; i < trackedImageManager.referenceLibrary.count; i++) {
                if (trackedImageManager.referenceLibrary[i].guid == trackedImage.referenceImage.guid) {
                    _currentBoardId = i + 1; // Atualiza o ID do quadro
                    Debug.Log("GUID do quadro: " + trackedImage.referenceImage.guid);
                    Debug.Log("Quadro atualizado: " + trackedImage.referenceImage.name);
                    Debug.Log("ID do quadro: " + _currentBoardId);
                    break;
                }
            }
        }
    }

    private void DeleteNoteFromDatabase(GameObject note) {
        var noteComponent = note.GetComponent<NoteComponent>();
        if (noteComponent != null) {
            _databaseManager.Notes.Delete(noteComponent.noteEntity.Id);
        }
    }

    private void LoadNotes() {
        if (_notesLoaded || _boardTransform == null) return;

        var notes = _databaseManager.Notes.GetNotesByBoardId(_currentBoardId);
        foreach (var noteEntity in notes) {
            Vector3 worldNotePosition = _boardTransform.TransformPoint(new Vector3((float)noteEntity.X, (float)noteEntity.Y, (float)noteEntity.Z));
            var note = Instantiate(note3DPrefab, worldNotePosition, _boardTransform.rotation, _boardTransform);
            note.GetComponentInChildren<TextMeshPro>().text = noteEntity.Content;
            note.tag = "Note";

            var noteComponent = note.AddComponent<NoteComponent>();
            noteComponent.noteEntity = noteEntity;

            if (!string.IsNullOrEmpty(noteEntity.Location)) {
                var locationInputField = _locationInputPanel.GetComponentInChildren<TMP_InputField>();
                locationInputField.text = noteEntity.Location;
                UpdateLocationButtonState(true);
            }
        }
        _notesLoaded = true;
    }

    private void UpdateLocationButtonState(bool isEnabled) {
        var routeButton = _activeNoteWidget.transform.Find("LocationButton").GetComponent<Button>();
        routeButton.interactable = isEnabled;
    }
}
