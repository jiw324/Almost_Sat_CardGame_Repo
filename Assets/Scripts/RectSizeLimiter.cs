using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RectSizeLimiter : UIBehaviour, ILayoutSelfController
{

    private RectTransform rt;
    private HorizontalLayoutGroup hlg;

    [SerializeField]
    protected Vector2 m_maxSize = Vector2.zero;

    [SerializeField]
    protected Vector2 m_minSize = Vector2.zero;

    [SerializeField]
    private int intendedElementWidth = 200;

    [SerializeField]
    private float defaultSpacing = -40;

    public Vector2 maxSize
    {
        get { return m_maxSize; }
        set
        {
            if (m_maxSize != value)
            {
                m_maxSize = value;
                SetDirty();
            }
        }
    }

    public Vector2 minSize
    {
        get { return m_minSize; }
        set
        {
            if (m_minSize != value)
            {
                m_minSize = value;
                SetDirty();
            }
        }
    }

    private DrivenRectTransformTracker m_Tracker;

    protected override void OnEnable()
    {
        base.OnEnable();
        rt = GetComponent<RectTransform>();
        hlg = GetComponent<HorizontalLayoutGroup>();
        SetDirty();
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(rt);
        base.OnDisable();
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rt);
    }

    public void SetLayoutHorizontal()
    {
        if (m_maxSize.x > 0f && rt.rect.width > m_maxSize.x)
        {
            int n = transform.childCount;
            float required = (maxSize.x - n * intendedElementWidth) / (n - 1);
            hlg.spacing = Mathf.Min(defaultSpacing, required);
        }
        else if (rt.rect.width < m_maxSize.x)
        {
            hlg.spacing = defaultSpacing;
        }
    }

    public void SetLayoutVertical()
    {
        if (m_maxSize.y > 0f && rt.rect.height > m_maxSize.y)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize.y);
            m_Tracker.Add(this, rt, DrivenTransformProperties.SizeDeltaY);
        }

    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty();
    }
#endif

}

