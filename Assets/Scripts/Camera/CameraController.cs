using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance = null;

    [SerializeField] BoxCollider2D levelBounds;
    [SerializeField] List<Transform> targetsTransforms;

    [SerializeField] float updateSizeSpeed, updatePositionSpeed;

    [SerializeField] float minSize;

    Camera controlledCamera;

    float nextCameraSize;
    Vector2 nextCameraPosition;

    [SerializeField] float screenRatio;
    [SerializeField] float paddingPercentAll;
    [SerializeField] float paddingPercentY;

    private void Awake()
    {
        Instance = this;

        controlledCamera = GetComponent<Camera>();

        CalculateCameraLocations();
        MoveCameraInstant();
    }

    private void LateUpdate()
    {
        CalculateCameraLocations();
        MoveCamera();
    }

    void MoveCamera()
    {
        Vector2 currentPosition = transform.position;

        if (currentPosition != nextCameraPosition)
        {
            Vector3 targetPosition;
            targetPosition.x = Mathf.MoveTowards(currentPosition.x, nextCameraPosition.x, updatePositionSpeed * Time.deltaTime);
            targetPosition.y = Mathf.MoveTowards(currentPosition.y, nextCameraPosition.y, updatePositionSpeed * Time.deltaTime);
            targetPosition.z = transform.position.z;

            transform.position = targetPosition;
        }

        float currentSize = controlledCamera.orthographicSize;

        if (currentSize != nextCameraSize)
        {
            controlledCamera.orthographicSize = Mathf.MoveTowards(currentSize, nextCameraSize, updateSizeSpeed * Time.deltaTime);
        }
    }

    void MoveCameraInstant()
    {
        Vector2 currentPosition = transform.position;

        if (currentPosition != nextCameraPosition)
        {
            Vector3 targetPosition;
            targetPosition.x = nextCameraPosition.x;
            targetPosition.y = nextCameraPosition.y;
            targetPosition.z = transform.position.z;

            transform.position = targetPosition;
        }

        float currentSize = controlledCamera.orthographicSize;

        if (currentSize != nextCameraSize)
        {
            controlledCamera.orthographicSize = nextCameraSize;
        }
    }

    void CalculateCameraLocations()
    {
        Vector2 averageCenter;
        Bounds targetBounds = new();

        foreach (Transform targetTransform in targetsTransforms)
        {
            Vector2 targetPosition = targetTransform.position;

            //Check if target is spell that got disabled
            if (targetPosition == (Vector2)ShooterGameManager.Instance.spellsSpawnPoint.position)
            {
                continue;
            }

            float targetX, targetY;

            targetX = Mathf.Clamp(targetPosition.x, levelBounds.bounds.min.x, levelBounds.bounds.max.x);
            targetY = Mathf.Clamp(targetPosition.y, levelBounds.bounds.min.y, levelBounds.bounds.max.y);

            targetPosition = new Vector3(targetX, targetY);

            targetBounds.Encapsulate(targetPosition);
        }

        averageCenter = targetBounds.center;

        float ratioX, ratioY;
        ratioX = targetBounds.extents.x * screenRatio;
        ratioY = targetBounds.extents.y;

        if (ratioX > ratioY)
        {
            nextCameraSize = targetBounds.extents.x * screenRatio;
        }
        else
        {
            nextCameraSize = targetBounds.extents.y;
        }

        nextCameraSize = Mathf.Max(nextCameraSize, minSize);
        float minYSize = targetBounds.extents.y * (1 + paddingPercentY);
        nextCameraSize = Mathf.Max(nextCameraSize, minYSize);
        nextCameraSize *= 1 + paddingPercentAll;

        float finalCamPosX = averageCenter.x, finalCamPosY = averageCenter.y;

        //Bounds cameraBounds = new();
        //cameraBounds.min = new Vector3(levelBounds.bounds.min.x + nextCameraSize / 2 / screenRatio,
        //                               levelBounds.bounds.min.y + nextCameraSize / 2);
        //cameraBounds.max = new Vector3(levelBounds.bounds.max.x - nextCameraSize / 2 / screenRatio,
        //                               levelBounds.bounds.max.y - nextCameraSize / 2);

        //finalCamPosX = Mathf.Clamp(averageCenter.x, cameraBounds.min.x, cameraBounds.max.x);
        //finalCamPosY = Mathf.Clamp(averageCenter.y, cameraBounds.min.y, cameraBounds.max.y);

        nextCameraPosition = new Vector3(finalCamPosX, finalCamPosY);
    }

    public void TrackTransform(Transform trackedTransform)
    {
        if (!targetsTransforms.Contains(trackedTransform))
        {
            targetsTransforms.Add(trackedTransform);
        }
    }
}
