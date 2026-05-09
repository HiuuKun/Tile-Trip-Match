using System.Collections.Generic;
using UnityEngine;

public class AspectPanelSwitcher : MonoBehaviour
{
    [Header("Portrait Objects")]
    [SerializeField] private List<GameObject> portraitObjects = new();

    [Header("Landscape Objects")]
    [SerializeField] private List<GameObject> landscapeObjects = new();

    private bool? lastIsPortrait;

    private void Awake()
    {
        ApplyLayout();
    }

    private void Update()
    {
        bool isPortrait = Screen.height >= Screen.width;

        if (lastIsPortrait.HasValue && lastIsPortrait.Value == isPortrait)
            return;

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        bool isPortrait = Screen.height >= Screen.width;
        lastIsPortrait = isPortrait;

        SetObjectsActive(portraitObjects, isPortrait);
        SetObjectsActive(landscapeObjects, !isPortrait);
    }

    private void SetObjectsActive(List<GameObject> objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            if (obj == null)
                continue;

            obj.SetActive(active);
        }
    }
}