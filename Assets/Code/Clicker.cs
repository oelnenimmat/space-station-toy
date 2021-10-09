using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Clicker : MonoBehaviour
{
    public new Camera camera;
    public float rotateSpeed = 1;
    public float scrollSpeed = 1;
    public float moveSpeed = 1;

    public float cameraMinDistance = 1;
    public float cameraMaxDistance = 100;

    // Look up astronomical terms
    private float cameraOrbitAngle;
    private float cameraTumbleAngle;

    private bool dragRotateView;
    private bool dragMoveView;

    public GameObject highlight;

    public float clickTimerDuration = 0.2f;
    private float addClickTimer;
    private float removeClickTimer;

    public SoundEffects soundEffects;
    public Map map;

    void Start()
    {
        transform.position = map.startCoord;
    }

    void Update()
    {
        {
            float cameraDistance = -camera.transform.localPosition.z;

            float distanceInput = Input.mouseScrollDelta.y;

            cameraDistance += distanceInput * scrollSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, cameraMinDistance, cameraMaxDistance);

            camera.transform.localPosition = new Vector3(0, 0, -cameraDistance);
        }

        bool selected = false;
        Vector3Int selectedCoords = Vector3Int.zero;
        Vector3 selectedNormal = Vector3.zero;
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                highlight.SetActive(true);
                
                Vector3Int coords = hit.collider.GetComponent<MapCube>().coords;
                Vector3 position = coords + hit.normal * 0.5f;

                highlight.transform.position = position;
                highlight.transform.forward = hit.normal;

                selected = true;
                selectedCoords = coords;
                selectedNormal = hit.normal;

            }
            else
            {
                highlight.SetActive(false);
            }

        }

        addClickTimer -= Time.deltaTime;
        removeClickTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            if (selected)
            {
                addClickTimer = clickTimerDuration;
            }

            dragRotateView = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selected && addClickTimer > 0)
            {
                Vector3Int direction = Vector3Int.RoundToInt(selectedNormal);

                Assert.AreEqual(direction.magnitude, 1);

                if (map.Add(selectedCoords + direction))
                {
                    soundEffects.Play(SoundEffect.Add);
                }
                else
                {
                    soundEffects.Play(SoundEffect.Error);
                }
            }
            dragRotateView = false;
        }

        if (dragRotateView)
        {
            // Todo(Leo): we should have an tiny bit of clearance before starting to move this

            float xInput = Input.GetAxis("Mouse X");
            float yInput = Input.GetAxis("Mouse Y");

            cameraOrbitAngle += xInput * rotateSpeed;
            cameraTumbleAngle += -yInput * rotateSpeed;

            Quaternion a = Quaternion.AngleAxis(cameraOrbitAngle, Vector3.up);
            Quaternion b = Quaternion.AngleAxis(cameraTumbleAngle, Vector3.right);

            transform.rotation = a * b;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (selected)
            {
                removeClickTimer = clickTimerDuration;
            }
            
            // Todo(Leo): there should be a little momentum build up or something, about the time of clickTimerDuration
            dragMoveView = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (selected && removeClickTimer > 0)
            {
                if (map.Destroy(selectedCoords))
                {
                    soundEffects.Play(SoundEffect.Remove);
                }
                else
                {
                    soundEffects.Play(SoundEffect.Error);
                }
            }

            dragMoveView = false;
        }


        if (dragMoveView)
        {
            float xInput = Input.GetAxis("Mouse X");
            float yInput = Input.GetAxis("Mouse Y");

            Vector3 FlatOnXZPlane(Vector3 v)
            {
                v.y = 0;
                v = v.normalized;
                return v;
            }

            transform.Translate(-xInput * moveSpeed * FlatOnXZPlane(transform.right), Space.World);
            transform.Translate(-yInput * moveSpeed * FlatOnXZPlane(transform.forward), Space.World);
        }
    }
}
