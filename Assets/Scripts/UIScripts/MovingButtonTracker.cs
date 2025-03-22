using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MovingButtonTracker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button movingButton;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    private Image buttonImage;

    private Vector3 initialPosition;

    void Start()
    {
        buttonImage = movingButton.GetComponent<Image>();
        initialPosition = movingButton.transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsPointerOverButton())
        {
            Debug.Log("Button Clicked!");
            movingButton.onClick.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = normalColor;
    }

    bool IsPointerOverButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == movingButton.gameObject)
                return true;
        }
        return false;
    }
}
