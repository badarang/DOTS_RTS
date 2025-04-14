using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    
    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectionAreaStart += OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += OnSelectionAreaEnd;
        selectionBox.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (selectionBox.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }
    
    private void OnSelectionAreaStart(object sender, System.EventArgs e)
    {
        selectionBox.gameObject.SetActive(true);
        selectionBox.sizeDelta = Vector2.zero;
    }
    
    private void OnSelectionAreaEnd(object sender, System.EventArgs e)
    {
        selectionBox.gameObject.SetActive(false);
    }
    
    private void UpdateVisual()
    {
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();
        selectionBox.anchoredPosition = selectionAreaRect.position;
        selectionBox.sizeDelta = selectionAreaRect.size;
    }
}
