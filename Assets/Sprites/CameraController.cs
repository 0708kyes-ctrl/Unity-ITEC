using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;

    private Camera cam;
    private SpriteRenderer background;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    void Start()
    {
        cam = GetComponent<Camera>();

        background = GameObject.Find("Background").GetComponent<SpriteRenderer>();

        UpdateBounds();
    }

    void Update()
    {
        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, y, 0);

        transform.position += move * moveSpeed * Time.deltaTime;

        ClampCamera();
    }

    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        cam.orthographicSize -= scroll * zoomSpeed;

        float bgHeight = background.bounds.size.y;
        float bgWidth = background.bounds.size.x;

        float screenRatio = (float)Screen.width / Screen.height;

        float maxZoomY = bgHeight / 2f;
        float maxZoomX = bgWidth / 2f / screenRatio;

        float maxZoom = Mathf.Min(maxZoomX, maxZoomY);

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize,
            minZoom,
            maxZoom
        );

        UpdateBounds();

        ClampCamera();
    }

    void UpdateBounds()
    {
        Bounds bounds = background.bounds;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        minX = bounds.min.x + horzExtent;
        maxX = bounds.max.x - horzExtent;

        minY = bounds.min.y + vertExtent;
        maxY = bounds.max.y - vertExtent;
    }

    void ClampCamera()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}