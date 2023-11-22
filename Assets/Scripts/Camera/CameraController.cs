using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] BoxCollider2D levelBounds;
    [SerializeField] List<Transform> targetsTransforms;

    [SerializeField] float updateSizeSpeed, updatePositionSpeed;

    [SerializeField] float minSize, maxSize;

    Camera controlledCamera;

    float nextCameraSize;
    Vector2 nextCameraPosition;

    [SerializeField] float screenRatio;
    [SerializeField] float paddingPercentAll;
    [SerializeField] float paddingPercentY;

    private void Awake()
    {
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
        Vector2 totalPositions = Vector2.zero;
        Bounds targetBounds = new();

        foreach (Transform targetTransform in targetsTransforms)
        {
            Vector2 targetPosition = targetTransform.position;

            if (!levelBounds.bounds.Contains(targetPosition))
            {
                float targetX, targetY;

                targetX = Mathf.Clamp(targetPosition.x, levelBounds.bounds.min.x, levelBounds.bounds.max.x);
                targetY = Mathf.Clamp(targetPosition.y, levelBounds.bounds.min.y, levelBounds.bounds.max.y);

                targetPosition = new Vector3(targetX, targetY);
            }

            totalPositions += targetPosition;
            targetBounds.Encapsulate(targetPosition);
        }

        averageCenter = totalPositions / targetsTransforms.Count;

        float ratioX, ratioY;
        ratioX = targetBounds.extents.x * screenRatio;
        ratioY = targetBounds.extents.y;

        if (ratioX > ratioY)
        {
            nextCameraSize = targetBounds.extents.x * screenRatio;
            nextCameraSize *= 1 + paddingPercentAll;
            nextCameraSize = Mathf.Clamp(nextCameraSize, minSize, maxSize);
        }
        else
        {
            nextCameraSize = targetBounds.extents.y;
            nextCameraSize *= 1 + paddingPercentAll;
            nextCameraSize = Mathf.Clamp(nextCameraSize, minSize, maxSize);
        }

        float minYSize = targetBounds.extents.y * (1 + paddingPercentY);
        nextCameraSize = Mathf.Max(nextCameraSize, minYSize);

        nextCameraPosition = new Vector3(averageCenter.x, averageCenter.y);
    }
}
