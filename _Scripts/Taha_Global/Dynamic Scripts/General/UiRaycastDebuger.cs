using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A powerful UI debugging tool that reports raycast hits.
/// it almost covers any possible problem and report it to you.
/// </summary>
public class UiRayCastDebugger : MonoBehaviour
{
    [Tooltip("Show detailed information for each hit")]
    public bool showCompleteInfo = true;

    [Tooltip("Only show warnings when problems are detected")]
    public bool onlyShowWarnings = false;

    private EventSystem eventSystem;
    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Awake()
    {
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("❌ No EventSystem found in scene! UI raycasting won't work.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformRaycast();
            ProcessResults();
        }
    }

    private void PerformRaycast()
    {
        pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };
        raycastResults.Clear();

        if (eventSystem != null)
        {
            eventSystem.RaycastAll(pointerData, raycastResults);
        }
    }

    private void ProcessResults()
    {
        if (eventSystem == null)
        {
            Debug.Log("⚠️ [UI Raycast] No EventSystem found in scene");
            return;
        }

        StringBuilder report = new StringBuilder();
        report.AppendLine("=== UI RAYCAST REPORT ===");

        if (raycastResults.Count == 0)
        {
            report.AppendLine("⚠️ [UI Raycast] No UI elements hit by raycast");

            CanvasGroup[] allCanvasGroups = FindObjectsOfType<CanvasGroup>(true);
            bool foundBlockingGroup = false;

            foreach (var group in allCanvasGroups)
            {
                if (!group.blocksRaycasts && group.gameObject.activeInHierarchy)
                {
                    report.AppendLine($"⚠️ Potential blocker: CanvasGroup '{group.gameObject.name}' has blocksRaycasts = false");
                    foundBlockingGroup = true;
                }
            }

            if (!foundBlockingGroup)
            {
                report.AppendLine("✅ No CanvasGroups with blocksRaycasts = false found in scene");
            }

            Debug.Log(report.ToString());
            return;
        }

        report.AppendLine($"Hits: {raycastResults.Count}");
        report.AppendLine("-------------------------");

        foreach (var result in raycastResults)
        {
            GameObject hitObject = result.gameObject;
            StringBuilder objectReport = new StringBuilder();
            bool hasProblems = false;

            objectReport.AppendLine($"Object: {hitObject.name}");
            objectReport.AppendLine($"Layer: {LayerMask.LayerToName(hitObject.layer)}");
            objectReport.AppendLine($"Active: {hitObject.activeInHierarchy}");

            hasProblems |= AppendRaycastTargetInfo(hitObject, objectReport);
            hasProblems |= AppendButtonInfo(hitObject, objectReport);
            hasProblems |= AppendGraphicInfo(hitObject, objectReport);
            hasProblems |= AppendCanvasGroupInfo(hitObject, objectReport);
            hasProblems |= AppendCanvasInfo(hitObject, objectReport);

            // NEW: Detect backface clicks in world-space canvases
            Canvas canvas = hitObject.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            {
                Vector3 normal = hitObject.transform.forward;
                float dot = Vector3.Dot(normal, Camera.main.transform.forward);
                if (dot > 0.0f)
                {
                    objectReport.AppendLine("❌ PROBLEM: Clicked on the BACK side of a World-Space UI element");
                    hasProblems = true;
                }
            }

            if (hasProblems || showCompleteInfo || !onlyShowWarnings)
            {
                report.Append(objectReport.ToString());
                report.AppendLine("-------------------------");
            }
        }

        Debug.Log(report.ToString());
    }

    private bool AppendRaycastTargetInfo(GameObject obj, StringBuilder report)
    {
        var graphics = obj.GetComponents<Graphic>();
        bool hasRaycastTarget = false;

        foreach (var graphic in graphics)
        {
            if (graphic.raycastTarget)
            {
                hasRaycastTarget = true;
                break;
            }
        }

        report.AppendLine($"Raycast Target: {hasRaycastTarget}");

        if (!hasRaycastTarget)
        {
            report.AppendLine("❌ PROBLEM: No Graphic with raycastTarget enabled");
            return true;
        }

        return false;
    }

    private bool AppendButtonInfo(GameObject obj, StringBuilder report)
    {
        Button button = obj.GetComponent<Button>();
        if (button == null) return false;

        report.AppendLine($"Button: interactable={button.interactable}");
        report.AppendLine($"Click Handlers: {button.onClick.GetPersistentEventCount()}");

        bool hasProblems = false;

        if (!button.interactable)
        {
            report.AppendLine("❌ PROBLEM: Button is not interactable");
            hasProblems = true;
        }

        if (button.onClick.GetPersistentEventCount() == 0)
        {
            report.AppendLine("⚠️ PROBLEM: Button has no click handlers");
            hasProblems = true;
        }

        return hasProblems;
    }

    private bool AppendGraphicInfo(GameObject obj, StringBuilder report)
    {
        Image image = obj.GetComponent<Image>();
        bool hasProblems = false;

        if (image != null)
        {
            report.AppendLine($"Image: sprite={(image.sprite != null)}, alpha={image.color.a}");

            if (image.sprite == null && image.color.a <= 0)
            {
                report.AppendLine("❌ PROBLEM: Image has no sprite and is fully transparent");
                hasProblems = true;
            }
        }

        return hasProblems;
    }

    private bool AppendCanvasGroupInfo(GameObject obj, StringBuilder report)
    {
        CanvasGroup[] groups = obj.GetComponentsInParent<CanvasGroup>();
        bool hasProblems = false;

        if (groups.Length > 0)
        {
            foreach (var group in groups)
            {
                report.AppendLine($"CanvasGroup ({group.gameObject.name}): " +
                                $"interactable={group.interactable}, " +
                                $"blocksRaycasts={group.blocksRaycasts}");

                if (!group.interactable)
                {
                    report.AppendLine($"⚠️ PROBLEM: CanvasGroup {group.gameObject.name} is not interactable");
                    hasProblems = true;
                }

                if (!group.blocksRaycasts)
                {
                    report.AppendLine($"⚠️ PROBLEM: CanvasGroup {group.gameObject.name} does not block raycasts");
                    hasProblems = true;
                }
            }
        }

        return hasProblems;
    }

    private bool AppendCanvasInfo(GameObject obj, StringBuilder report)
    {
        Canvas canvas = obj.GetComponentInParent<Canvas>();
        if (canvas == null) return false;

        report.AppendLine($"Canvas ({canvas.gameObject.name}): " +
                         $"enabled={canvas.enabled}, " +
                         $"renderMode={canvas.renderMode}");

        if (!canvas.enabled)
        {
            report.AppendLine($"❌ PROBLEM: Canvas {canvas.gameObject.name} is disabled");
            return true;
        }

        return false;
    }
}