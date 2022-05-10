using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pile : MonoBehaviour
{
    //public DropZone actualDropZone;
    public DropZone dropZone;

    public void SetColor(Color c) { image.color = c; }
    public RectTransform GetRect() { return rect; }

    private RectTransform rect;
    private Image image;
    private DropZone actualDropZone;

    private void Awake()
    {
        image = GetComponent<Image>(); 
        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Constants.CARD_WIDTH, Constants.CARD_HEIGHT);
    }
}
