using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;

    private Camera cam;

    // 모든 Background를 합친 영역
    private Bounds mapBounds;
    private bool mapFound = false;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    void Start()
    {
        cam = GetComponent<Camera>();
        StartCoroutine(FindBackgrounds());
    }

    IEnumerator FindBackgrounds()
    {
        while (!mapFound)
        {
            GameObject[] objects = FindObjectsByType<GameObject>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            bool first = true;

            foreach (GameObject obj in objects)
            {
                if (obj.name.Contains("Background"))
                {
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

                    if (sr != null)
                    {
                        if (first)
                        {
                            mapBounds = sr.bounds;
                            first = false;
                        }
                        else
                        {
                            mapBounds.Encapsulate(sr.bounds);
                        }

                        Debug.Log("Background 발견 : " + obj.name);
                    }
                }
            }

            if (!first)
            {
                mapFound = true;
                UpdateBounds();
                ClampCamera();
                Debug.Log("Background 전체 영역 계산 완료");
                yield break;
            }

            yield return null;
        }
    }

    void Update()
    {
        if (!mapFound)
            return;

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

        float bgHeight = mapBounds.size.y;
        float bgWidth = mapBounds.size.x;

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
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        minX = mapBounds.min.x + horzExtent;
        maxX = mapBounds.max.x - horzExtent;

        minY = mapBounds.min.y + vertExtent;
        maxY = mapBounds.max.y - vertExtent;
    }

    void ClampCamera()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}