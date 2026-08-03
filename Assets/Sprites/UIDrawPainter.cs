using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDrawPainter : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler
{
    [Header("그림판")]
    public RawImage drawImage;

    [Header("텍스처 크기")]
    public int width = 1024;
    public int height = 1024;

    [Header("붓")]
    public int brushSize = 10;

    public Color brushColor = Color.black;

    public Color backgroundColor = Color.white;

    private Texture2D texture;

    private bool isEraser = false;

    //----------------------------------------------------

    void Start()
    {
        texture = new Texture2D(width, height);

        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;

        texture.SetPixels(pixels);
        texture.Apply();

        drawImage.texture = texture;
    }

    //----------------------------------------------------

    public void OnPointerDown(PointerEventData eventData)
    {
        Draw(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Draw(eventData);
    }

    //----------------------------------------------------

    void Draw(PointerEventData eventData)
    {
        RectTransform rect = drawImage.rectTransform;

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        Rect r = rect.rect;

        float px = Mathf.InverseLerp(r.xMin, r.xMax, localPoint.x);
        float py = Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y);

        int x = Mathf.RoundToInt(px * width);
        int y = Mathf.RoundToInt(py * height);

        PaintCircle(x, y);

        texture.Apply();
    }

    //----------------------------------------------------

    void PaintCircle(int cx, int cy)
    {
        Color color = isEraser ? backgroundColor : brushColor;

        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int y = -brushSize; y <= brushSize; y++)
            {
                if (x * x + y * y > brushSize * brushSize)
                    continue;

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || px >= width)
                    continue;

                if (py < 0 || py >= height)
                    continue;

                texture.SetPixel(px, py, color);
            }
        }
    }

    //----------------------------------------------------

    public void SetColor(Color color)
    {
        brushColor = color;
        isEraser = false;
    }

    public void SetBrush()
    {
        isEraser = false;
    }

    public void SetEraser()
    {
        isEraser = true;
    }
}