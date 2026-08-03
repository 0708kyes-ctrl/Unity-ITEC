using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    [Header("RGB 입력")]
    public TMP_InputField rInput;
    public TMP_InputField gInput;
    public TMP_InputField bInput;

    [Header("현재 색")]
    public Image preview;

    [Header("그림판")]
    public UIDrawPainter painter;

    [Header("붓 크기")]
    public Slider brushSizeSlider;

    public void ApplyColor()
    {
        int r = ReadValue(rInput);
        int g = ReadValue(gInput);
        int b = ReadValue(bInput);

        Color color = new Color32((byte)r, (byte)g, (byte)b, 255);

        preview.color = color;
        painter.SetColor(color);
    }

    private int ReadValue(TMP_InputField input)
    {
        if (input == null)
            return 0;

        int value;

        if (!int.TryParse(input.text, out value))
            value = 0;

        return Mathf.Clamp(value, 0, 255);
    }

    public void Brush()
    {
        painter.SetBrush();
    }

    public void Eraser()
    {
        painter.SetEraser();
    }

    public void SetBrushSize(float value)
    {
        painter.brushSize = Mathf.RoundToInt(value);
    }
}