using UnityEngine;

public class MovingCamera : MonoBehaviour
{
    private float edgeSize = 70f;
    private float moveAmount = 200f;
    Vector3 touchStart;
    private void Start()
    {
        Vector3 cameraStartPosition;
        cameraStartPosition.x = -Settings.instance.officeWidth / 2f - 2;
        cameraStartPosition.y = PlanManager.instance.floors[0].RoomHeight / 2f;
        cameraStartPosition.z = transform.position.z;
        transform.position = cameraStartPosition;
    }

    void Update()
    {
#if UNITY_ANDROID
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            if (!PlanManager.IsPointerOverUIObject())
            {
                PlanManager.instance.followTarget = null;
                Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Camera.main.transform.position.x + direction.x > PlanManager.instance.worldBounds.left && Camera.main.transform.position.x + direction.x < PlanManager.instance.worldBounds.right
                    && Camera.main.transform.position.y + direction.y > PlanManager.instance.worldBounds.down && Camera.main.transform.position.y + direction.y < PlanManager.instance.worldBounds.up)
                {
                    Camera.main.transform.position += direction;
                }
            }
        }
#endif
#if !UNITY_ANDROID
        if (Input.GetMouseButton(1))
        {
            EdgeScrolling();
        }
#endif

        else
        {
            if (PlanManager.instance != null && PlanManager.instance.followTarget != null)
            {
                FollowAgent();
            }
        }

    }

    public void MoveCamera(Vector3 cameraFollowPosition)
    {
        Vector3 cameraMoveDir = (cameraFollowPosition - transform.position).normalized;
        float distance = Vector3.Distance(cameraFollowPosition, transform.position);
        float cameraMoveSpeed = 2.5f;

        if (distance > 0)
        {
            Vector3 newCameraPosition = transform.position + cameraMoveDir * distance * cameraMoveSpeed * Time.deltaTime / Time.timeScale
                / Time.timeScale;
            float distanceAfterMoving = Vector3.Distance(newCameraPosition, cameraFollowPosition);

            if (distanceAfterMoving > distance)
            {
                newCameraPosition = cameraFollowPosition;
            }

            transform.position = newCameraPosition;
        }
    }

    public void EdgeScrolling()
    {
        Vector3 cameraFollowPosition;
        cameraFollowPosition = transform.position;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        if (mousePos.x > Screen.width - edgeSize && worldPosition.x <= PlanManager.instance.worldBounds.right)
        {
            cameraFollowPosition.x += moveAmount * Time.deltaTime;
        }
        if (mousePos.x < edgeSize && worldPosition.x >= PlanManager.instance.worldBounds.left)
        {
            cameraFollowPosition.x -= moveAmount * Time.deltaTime;
        }
        if (mousePos.y > Screen.height - edgeSize && worldPosition.y <= PlanManager.instance.worldBounds.up)
        {
            cameraFollowPosition.y += moveAmount * Time.deltaTime;
        }
        if (mousePos.y < edgeSize && worldPosition.y >= PlanManager.instance.worldBounds.down)
        {
            cameraFollowPosition.y -= moveAmount * Time.deltaTime;
        }
        PlanManager.instance.followTarget = null;
        MoveCamera(cameraFollowPosition);
    }

    public void FollowAgent()
    {
        Vector3 cameraFollowPosition;
        cameraFollowPosition.x = PlanManager.instance.followTarget.transform.position.x;
        cameraFollowPosition.y = PlanManager.instance.followTarget.transform.position.y;
        cameraFollowPosition.z = transform.position.z;
        MoveCamera(cameraFollowPosition);
    }
}