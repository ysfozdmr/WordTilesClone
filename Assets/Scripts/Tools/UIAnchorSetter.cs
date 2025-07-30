using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class UIAnchorSetter : MonoBehaviour
{
#if UNITY_EDITOR
    
    [MenuItem("Fenrir/UI/Set Anchors to Corners %#a")]
    private static void SetAnchorsToCorners()
    {
        // Seçili tüm GameObject'leri kontrol et
        GameObject[] _selectedObjects = Selection.gameObjects;

        if (_selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        foreach (GameObject _obj in _selectedObjects)
        {
            // GameObject'in RectTransform bileşenini al
            RectTransform _uiElement = _obj.GetComponent<RectTransform>();

            if (_uiElement != null && _uiElement.parent != null)
            {
                RectTransform _targetObject = _uiElement.parent.GetComponent<RectTransform>();

                if (_targetObject != null)
                {
                    // UI elementinin parent'ın köşelerine göre ancorlarını ayarla
                    Vector2 _newAnchorMin = _uiElement.anchorMin;
                    Vector2 _newAnchorMax = _uiElement.anchorMax;

                    Vector2 _parentSize = _targetObject.rect.size;

                    // Min Anchor'ı hesapla
                    _newAnchorMin.x = _uiElement.offsetMin.x / _parentSize.x + _uiElement.anchorMin.x;
                    _newAnchorMin.y = _uiElement.offsetMin.y / _parentSize.y + _uiElement.anchorMin.y;

                    // Max Anchor'ı hesapla
                    _newAnchorMax.x = _uiElement.offsetMax.x / _parentSize.x + _uiElement.anchorMax.x;
                    _newAnchorMax.y = _uiElement.offsetMax.y / _parentSize.y + _uiElement.anchorMax.y;

                    // Yeni ancor değerlerini ayarla
                    _uiElement.anchorMin = _newAnchorMin;
                    _uiElement.anchorMax = _newAnchorMax;

                    // anchoredPosition ve sizeDelta değerlerini koru
                    _uiElement.anchoredPosition = Vector2.zero;
                    _uiElement.sizeDelta = Vector2.zero;

                    Debug.Log($"Anchors set to match corners for: {_obj.name}");
                }
                else
                {
                    Debug.LogWarning($"The parent of {_obj.name} does not have a RectTransform.");
                }
            }
            else
            {
                Debug.LogWarning($"{_obj.name} is not a UI element or has no parent with a RectTransform.");
            }
        }
    }
#endif
}