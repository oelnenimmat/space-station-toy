using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


public class Clicker : MonoBehaviour
{
    public MapCellType enabledMapCellType;

    public new Camera camera;
    public float rotateSpeed = 1;
    public float scrollSpeed = 1;
    public float moveSpeed = 1;

    public float cameraMinDistance = 1;
    public float cameraMaxDistance = 100;

    // Note(Leo): this is space game :)
    private float cameraAzimuth;
    private float cameraAltitude;

    private float cameraAzimuthDelta;
    private float cameraAltitudeDelta;

        // private bool dragRotateView;
        // private bool dragMoveView;

    public GameObject highlight;

    public float clickTimerDuration = 0.2f;
    private float addClickTimer;
    private float removeClickTimer;

    public SoundEffects soundEffects;
    public Map map;

    public int undoStackSize;
    public UndoRedo undoRedo;

    public Button enableAddButton;
    public Button enableRemoveButton;
    public Button undoButton;
    public Button redoButton;

    public Button enableStructurePlacementButton;
    public Button enableSolarArrayPlacementButton;

    public Button currentEnabledPlacementButton;

    public Color placementEnabledButtonColor;
    public Color placementDisabledButtonColor;
    public float placementDisabledButtonScale;

    public Button saveButton;
    public Button loadButton;
    public Button trashButton;

    public bool autoLoadGameOnStart;
    public bool autoSaveOnAddAndRemove;

    public bool popupVisible => popupGameObject.activeInHierarchy;
    public bool popupClosedTooRecently;
    public GameObject popupGameObject;
    public Button popupDeleteButton;
    public Button popupCancelButton;



    void Awake()
    {
        undoRedo = new UndoRedo(undoStackSize);

        #if UNITY_ANDROID
            inputSource = new TabletInput(this);
        #else
            inputSource = new ComputerInput();
        #endif

        void SetEnabled(Button buttonWithImage)
        {
            Image image = buttonWithImage.GetComponent<Image>();

            image.color = placementEnabledButtonColor;
            buttonWithImage.transform.localScale = Vector3.one;
        }

        void SetDisabled(Button buttonWithImage)
        {
            Image image = buttonWithImage.GetComponent<Image>();

            image.color = placementDisabledButtonColor;
            buttonWithImage.transform.localScale = Vector3.one * placementDisabledButtonScale;
        }

        enableStructurePlacementButton.onClick.AddListener(() => {
            SetDisabled(currentEnabledPlacementButton);

            currentEnabledPlacementButton = enableStructurePlacementButton;
            SetEnabled(currentEnabledPlacementButton);

            enabledMapCellType = MapCellType.Structure;

            soundEffects.Play(soundEffects.ui);
        });

        enableSolarArrayPlacementButton.onClick.AddListener(() => {
            SetDisabled(currentEnabledPlacementButton);

            currentEnabledPlacementButton = enableSolarArrayPlacementButton;
            SetEnabled(currentEnabledPlacementButton);

            enabledMapCellType = MapCellType.Solar;

            soundEffects.Play(soundEffects.ui);
        });

        SetEnabled(enableStructurePlacementButton);
        SetDisabled(enableSolarArrayPlacementButton);

        currentEnabledPlacementButton = enableStructurePlacementButton;

        enabledMapCellType = MapCellType.Structure;

        trashButton.onClick.AddListener(() => {
            popupGameObject.SetActive(true);
            CancelInputOperations();

            soundEffects.Play(soundEffects.ui);
        });

        popupCancelButton.onClick.AddListener(() => {
            popupGameObject.SetActive(false);
            popupClosedTooRecently = true;
            CancelInputOperations();

            soundEffects.Play(soundEffects.ui);
        });

        popupDeleteButton.onClick.AddListener(() => {
            popupGameObject.SetActive(false);
            popupClosedTooRecently = true;
            CancelInputOperations();

            map.ClearAndInitialize();

            soundEffects.Play(soundEffects.ui);
        });
    }
    void Start()
    {
        transform.position = map.startCoord;

        // Note(Leo): this way we enforce order of execution
        map.Begin();

        if (autoLoadGameOnStart && System.IO.File.Exists(GetSaveFilePath()))
        {
            Load();
        }
    }

    [System.Serializable]
    public struct InputValues
    {
        public float    zoom;
        public Vector2  dragDelta;
        public bool     dragRotateView;
        public bool     dragMoveView;
        public float    dragMomentum;

        public bool     cursorAvailable;
        public Vector3  cursorPosition;
        public bool     clickAdd;
        public bool     clickRemove;

        public bool     undo;
        public bool     redo;
    }

    public InputValues DEBUGInputValues;

    public class ComputerInput
    {
        public bool dragRotateView;
        public bool dragMoveView;
        public float dragMomentum;

        private const float dragMomentumTime = 0.2f;

        public InputValues GetInput()
        {
            InputValues input = new InputValues();

            input.zoom = Input.mouseScrollDelta.y;

            input.dragDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            if (Input.GetMouseButtonDown(0))
            {
                dragRotateView = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                dragRotateView = false;
                if (dragMomentum < 1)
                {
                    input.clickAdd = true;
                }
            }

            input.dragRotateView = dragRotateView;

            if (Input.GetMouseButtonDown(1))
            {
                dragMoveView = true;
            }

            if(Input.GetMouseButtonUp(1))
            {
                dragMoveView = false;
                if (dragMomentum < 1)
                {
                    input.clickRemove = true;
                }
            }

            input.dragMoveView = dragMoveView;

            if(dragRotateView || dragMoveView)
            {
                dragMomentum += Time.deltaTime / dragMomentumTime;
                dragMomentum = Mathf.Clamp01(dragMomentum);
            }
            else
            {                
                dragMomentum = 0;
            }

            input.dragMomentum = Mathf.Pow(dragMomentum, 2);


            input.cursorAvailable = true;
            input.cursorPosition = Input.mousePosition;

            if (Input.GetKeyDown(KeyCode.Z))
            {
                input.undo = true;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                input.redo = true;
            }

            return input;            
        }
    }

    public class TabletInput
    {
        public bool dragRotateView;
        public bool dragMoveView;
        public float dragMomentum;

        private const float dragMomentumTime = 0.2f;

        enum Mode { Add, Remove };
        Mode mode;

        // Todo(Leo): there may be multiple
        public bool undo;
        public bool redo;

        public TabletInput(Clicker c)
        {
            c.enableAddButton.onClick.AddListener(() => {
                mode = Mode.Add;
                c.enableAddButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                c.enableRemoveButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                c.soundEffects.Play(c.soundEffects.ui);
            });


            // Note(Leo): Also set these initial statelelelelfoisgno
            mode = Mode.Add;
            c.enableAddButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            c.enableRemoveButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            c.enableRemoveButton.onClick.AddListener(() => {
                mode = Mode.Remove;
                c.enableAddButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                c.enableRemoveButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                c.soundEffects.Play(c.soundEffects.ui);
            });

            c.undoButton.onClick.AddListener(() => { undo = true; });
            c.redoButton.onClick.AddListener(() => { redo = true; });
        }

        public InputValues GetInput()
        {
            InputValues input = new InputValues();

            input.undo = undo;
            undo = false;

            input.redo = redo;
            redo = false;

            if(Input.touchCount == 0)
            {
                return input;
            }

            if (Input.touchCount == 1)
            {
                dragMoveView = false;
                dragRotateView = true;

                Touch touch = Input.GetTouch(0);

                input.zoom = 0;

                input.dragDelta = touch.deltaPosition;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    dragRotateView = false;
                    if (dragMomentum < 1)
                    {
                        if (mode == Mode.Add)
                        {
                            input.clickAdd = true;
                        }
                        else if (mode == Mode.Remove)
                        {
                            input.clickRemove = true;
                        }
                    }
                }

                input.cursorAvailable = true;
                input.cursorPosition = touch.position;
            }
            
            if (Input.touchCount > 1)
            {
                dragRotateView = false;
                dragMoveView = true;

                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                float dot = Vector2.Dot(touch0.deltaPosition, touch1.deltaPosition);

                if (dot > 0.9f)
                {
                    input.dragDelta = (touch0.deltaPosition + touch1.deltaPosition) / 2;
                }

                if (dot < -0.8f)
                {
                    float previousDistance = Vector2.Distance(touch0.position - touch0.deltaPosition, touch1.position - touch1.deltaPosition);
                    float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                    input.zoom = (currentDistance - previousDistance) * 0.1f;
                }
            }

            // Todo(Leo): 0.1f corresponds to input managers "mouse x" and "mouse y" axes' sensitivity
            input.dragDelta *= 0.1f;

            input.dragRotateView = dragRotateView;
            input.dragMoveView = dragMoveView;

            if(dragRotateView || dragMoveView)
            {
                dragMomentum += Time.deltaTime / dragMomentumTime;
                dragMomentum = Mathf.Clamp01(dragMomentum);
            }
            else
            {                
                dragMomentum = 0;
            }

            input.dragMomentum = Mathf.Pow(dragMomentum, 2);



            return input;            
        }
    }

    #if UNITY_ANDROID
        public TabletInput inputSource;
    #else
        public ComputerInput inputSource;
    #endif

    void Update()
    {
        if (popupVisible)
        {
            return;
        }

        if (popupClosedTooRecently)
        {
            popupClosedTooRecently = false;
            return;
        }

        InputValues input = inputSource.GetInput();
        DEBUGInputValues = input;
        {
            float cameraDistance            = -camera.transform.localPosition.z;
            float distanceInput             = input.zoom;
            cameraDistance                  += distanceInput * scrollSpeed;
            cameraDistance                  = Mathf.Clamp(cameraDistance, cameraMinDistance, cameraMaxDistance);
            camera.transform.localPosition  = new Vector3(0, 0, -cameraDistance);
        }

        bool selected = false;
        Vector3Int selectedCoords = Vector3Int.zero;
        Vector3 selectedNormal = Vector3.zero;
        MapCellType selectedMapCellType = MapCellType.Structure;

        if (input.cursorAvailable)
        {
            Ray ray = camera.ScreenPointToRay(input.cursorPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                highlight.SetActive(true);
                
                Vector3Int coords = hit.collider.GetComponent<MapCell>().coords;
                Vector3 position = coords + hit.normal * 0.5f;

                highlight.transform.position = position;
                highlight.transform.forward = hit.normal;

                selected = true;
                selectedCoords = coords;
                selectedNormal = hit.normal;
                selectedMapCellType = hit.collider.GetComponent<MapCell>().type;

            }
            else
            {
                highlight.SetActive(false);
            }
        }
        else
        {
            highlight.SetActive(false);
        }

        addClickTimer -= Time.deltaTime;
        removeClickTimer -= Time.deltaTime;

        if (selected && input.clickAdd)
        {
            Vector3Int direction = Vector3Int.RoundToInt(selectedNormal);

            Assert.AreEqual(direction.magnitude, 1);

            if (map.Add(selectedCoords + direction, enabledMapCellType))
            {
                soundEffects.Play(soundEffects.add);
                undoRedo.Add(selectedCoords + direction, enabledMapCellType, UndoRedoOperation.Add);
            }
            else
            {
                soundEffects.Play(soundEffects.error);
            }

            if (autoSaveOnAddAndRemove)
            {
                Save();
            }
        }

        if (input.dragRotateView)
        {
            float step      = rotateSpeed * input.dragMomentum;
            cameraAzimuth   += input.dragDelta.x * step;
            cameraAltitude  += -input.dragDelta.y * step;

            Quaternion a = Quaternion.AngleAxis(cameraAzimuth, Vector3.up);
            Quaternion b = Quaternion.AngleAxis(cameraAltitude, Vector3.right);

            transform.rotation = a * b;
        }

        if (selected && input.clickRemove)
        { 
            if (map.Destroy(selectedCoords))
            {
                soundEffects.Play(soundEffects.remove);
                undoRedo.Add(selectedCoords, selectedMapCellType, UndoRedoOperation.Remove);
            }
            else
            {
                soundEffects.Play(soundEffects.error);
            }

            if (autoSaveOnAddAndRemove)
            {
                Save();
            }
        }

        if (input.dragMoveView)
        {
            Vector3 FlatOnXZPlane(Vector3 v)
            {
                v.y = 0;
                v = v.normalized;
                return v;
            }

            float step = moveSpeed * input.dragMomentum;

            transform.Translate(-input.dragDelta.x * step * FlatOnXZPlane(transform.right), Space.World);
            transform.Translate(-input.dragDelta.y * step * FlatOnXZPlane(transform.forward), Space.World);
        }

        // Todo(Leo): we should check with the map if undo or redo operation is actually okay. it should,
        // but im not super positive about it yet
        if (input.undo)
        {
            if (undoRedo.Undo(out UndoRedoItem item))
            {
                switch (item.operation)
                {
                    case UndoRedoOperation.Add:
                        if (map.Destroy(item.cell))
                        {
                            soundEffects.Play(soundEffects.remove);
                        }
                        break;

                    case UndoRedoOperation.Remove:
                        if (map.Add(item.cell, item.type))
                        {
                            soundEffects.Play(soundEffects.add);
                        }
                        break;
                }
            }
            else
            {
                soundEffects.Play(soundEffects.error);
            }
        }

        if (input.redo)
        {
            if (undoRedo.Redo(out UndoRedoItem item))
            {
                switch(item.operation)
                {
                    case UndoRedoOperation.Add:
                        if (map.Add(item.cell, item.type))
                        {
                            soundEffects.Play(soundEffects.add);
                        }
                        break;

                    case UndoRedoOperation.Remove:
                        if (map.Destroy(item.cell))
                        {
                            soundEffects.Play(soundEffects.remove);
                        }
                        break;
                }
            }
            else
            {
                soundEffects.Play(soundEffects.error);
            }
        }
    }

    private string GetSaveFilePath()
    {
        string path = $"{Application.persistentDataPath}/save-file.json";

        return path;
    }

    private void Save()
    {
        var data = map.GetSavedata();
        var json = JsonUtility.ToJson(data, prettyPrint: true);
        System.IO.File.WriteAllText(GetSaveFilePath(), json);

        Debug.Log($"Saved to {GetSaveFilePath()}");
    }

    private void Load()
    {
        var json = System.IO.File.ReadAllText(GetSaveFilePath());
        var data = JsonUtility.FromJson<Map.MapSaveData>(json);
        map.ReadSaveData(data);

        Debug.Log($"Loaded from {GetSaveFilePath()}");
    }

    private void CancelInputOperations()
    {
        inputSource.dragMoveView = false;
        inputSource.dragRotateView = false;
    }
}
