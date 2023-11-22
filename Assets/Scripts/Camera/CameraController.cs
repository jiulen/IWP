using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] BoxCollider2D levelBounds;
    [SerializeField] List<Transform> targetsTransforms;

    [SerializeField] float updateSizeSpeed, updatePositionSpeed;

    [SerializeField] float minSize, maxSize;

    [SerializeField] Camera controlledCamera;

    [SerializeField] Vector3 cameraOffset;

    float nextCameraSize;
    Vector2 nextCameraPosition;

    private void Awake()
    {
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
        ratioX = targetBounds.extents.x / levelBounds.bounds.extents.x;
        ratioY = targetBounds.extents.y / levelBounds.bounds.extents.y;

        float extents = 0;
        float lerpPercent = 0;

        if (ratioX > ratioY)
        {
            extents = Mathf.Min(targetBounds.extents.x, levelBounds.bounds.extents.x);
            lerpPercent = Mathf.InverseLerp(0, levelBounds.bounds.extents.x, extents);
        }
        else
        {
            extents = Mathf.Min(targetBounds.extents.y, levelBounds.bounds.extents.y);
            lerpPercent = Mathf.InverseLerp(0, levelBounds.bounds.extents.y, extents);
        }

        nextCameraSize = Mathf.Lerp(minSize, maxSize, lerpPercent);

        nextCameraPosition = new Vector3(averageCenter.x, averageCenter.y) + cameraOffset;
    }
}
