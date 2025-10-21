
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandResizer : MonoBehaviour
{

    /* Need access to:
     * the RectTransform of the hand
     * the HorizontalLayoutGroup of the hand
     * the intended width of each card
     * the default spacing of the HorizontalLayoutGroup
     * the max width of the hand
     * the child count of the hand
     */

    private RectTransform rt;
    private HorizontalLayoutGroup hlg;

    [SerializeField]
    protected Vector2 maxSize = Vector2.zero;

    [SerializeField]
    private int intendedElementWidth = 200;

    [SerializeField]
    private float defaultSpacing = -40;

    const float epsilon = 0.01f;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        hlg = GetComponent<HorizontalLayoutGroup>();
    }

    private float getWidthRequiredForChildren(int n)
    {
        if (n == 0) return 0;
        return n * intendedElementWidth + (n - 1) * defaultSpacing;
    }

    private float getSpacingToClamp(int n)
    {
        return (maxSize.x - n * intendedElementWidth) / (n - 1);
    }

    private void setSpacing(float spacing)
    {
        hlg.spacing = spacing;
    }

    void OnTransformChildrenChanged()
    {
        // If the width of rect is greater than maxSize.x, then reduce spacing to clamp the width to maxSize.x
        // If the width of rect is less than or equal to maxSize.x, then set spacing to defaultSpacing and set the width to fit the children
        int n = transform.childCount;
        float currentWidth = rt.rect.width;
        float spaceRequired = getWidthRequiredForChildren(n);
        if (spaceRequired > maxSize.x + epsilon)
        {
            setSpacing(getSpacingToClamp(n));
        }
        else
        {
            setSpacing(defaultSpacing);
        }

        if (currentWidth > spaceRequired)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, spaceRequired);
        }
    }
}
