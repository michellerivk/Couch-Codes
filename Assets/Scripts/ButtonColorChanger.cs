using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _lobbyText ;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ColorUtility.TryParseHtmlString("#DB6A2B", out Color newColor))
        {
            _lobbyText.color = newColor;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _lobbyText.transform.localScale = Vector3.one * 1.05f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ColorUtility.TryParseHtmlString("#BD5115", out Color newColor))
        {
            _lobbyText.color = newColor;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _lobbyText.transform.localScale = Vector3.one;
        }
    }
}
        