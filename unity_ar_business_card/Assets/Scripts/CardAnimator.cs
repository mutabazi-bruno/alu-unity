using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vuforia;

/// <summary>Plays the card's intro animation when the image target is found.</summary>
public class CardAnimator : MonoBehaviour
{
    /// <summary>Time for one icon to reach its spot.</summary>
    public float iconDuration = 0.6f;

    /// <summary>Delay between icons.</summary>
    public float iconStagger = 0.1f;

    /// <summary>When the text starts, relative to the icons.</summary>
    public float textDelay = 0.35f;

    /// <summary>Time for one line of text to slide in.</summary>
    public float textDuration = 0.5f;

    /// <summary>Delay between lines of text.</summary>
    public float textStagger = 0.12f;

    /// <summary>How far left the text starts.</summary>
    public float textSlideDistance = 60f;

    /// <summary>Height of the icon float.</summary>
    public float floatAmplitude = 6f;

    /// <summary>Speed of the icon float.</summary>
    public float floatSpeed = 2f;

    // icons and where they rest
    private readonly List<RectTransform> icons = new List<RectTransform>();
    private readonly List<Vector2> iconHomes = new List<Vector2>();

    // text lines, where they rest, and their fade
    private readonly List<RectTransform> texts = new List<RectTransform>();
    private readonly List<Vector2> textHomes = new List<Vector2>();
    private readonly List<CanvasGroup> textGroups = new List<CanvasGroup>();

    // icons fly out from the marker
    private Vector2 burstOrigin = new Vector2(-180f, 0f);

    // Vuforia target events
    private DefaultObserverEventHandler observerHandler;

    // running intro
    private Coroutine introRoutine;

    // intro done, icons float
    private bool idle;

    // find icons and text, save their positions
    private void Awake()
    {
        var marker = transform.Find("Marker") as RectTransform;
        if (marker != null)
            burstOrigin = marker.anchoredPosition;

        foreach (Transform child in transform)
        {
            var rt = child as RectTransform;
            if (rt == null) continue;

            if (child.name.StartsWith("Icon_"))
            {
                icons.Add(rt);
                iconHomes.Add(rt.anchoredPosition);
            }
            else if (child.GetComponent<TMP_Text>() != null)
            {
                texts.Add(rt);
                textHomes.Add(rt.anchoredPosition);
                var group = child.GetComponent<CanvasGroup>();
                textGroups.Add(group != null ? group : child.gameObject.AddComponent<CanvasGroup>());
            }
        }

        SetHidden();
    }

    // listen for target found/lost; without a target (editor test) just play
    private void Start()
    {
        observerHandler = GetComponentInParent<DefaultObserverEventHandler>();
        if (observerHandler != null)
        {
            observerHandler.OnTargetFound.AddListener(PlayIntro);
            observerHandler.OnTargetLost.AddListener(ResetCard);
        }
        else
        {
            PlayIntro();
        }
    }

    // stop listening
    private void OnDestroy()
    {
        if (observerHandler == null) return;
        observerHandler.OnTargetFound.RemoveListener(PlayIntro);
        observerHandler.OnTargetLost.RemoveListener(ResetCard);
    }

    // idle float, each icon slightly out of step
    private void Update()
    {
        if (!idle) return;
        for (var i = 0; i < icons.Count; i++)
        {
            var offset = Mathf.Sin(Time.time * floatSpeed + i * 0.8f) * floatAmplitude;
            icons[i].anchoredPosition = iconHomes[i] + Vector2.up * offset;
        }
    }

    /// <summary>Plays the intro from the start.</summary>
    public void PlayIntro()
    {
        if (introRoutine != null)
            StopCoroutine(introRoutine);
        introRoutine = StartCoroutine(Intro());
    }

    /// <summary>Stops the animation and hides the card again.</summary>
    public void ResetCard()
    {
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }
        SetHidden();
    }

    // starting pose: icons tucked in the marker, text shifted and invisible
    private void SetHidden()
    {
        idle = false;
        for (var i = 0; i < icons.Count; i++)
        {
            icons[i].anchoredPosition = burstOrigin;
            icons[i].localScale = Vector3.zero;
            icons[i].localRotation = Quaternion.Euler(0f, 0f, -180f);
        }
        for (var i = 0; i < texts.Count; i++)
        {
            texts[i].anchoredPosition = textHomes[i] + Vector2.left * textSlideDistance;
            textGroups[i].alpha = 0f;
        }
    }

    // icons pop out one by one, then the text slides in
    private IEnumerator Intro()
    {
        SetHidden();

        var iconsEnd = (icons.Count - 1) * iconStagger + iconDuration;
        var textsEnd = textDelay + (texts.Count - 1) * textStagger + textDuration;
        var total = Mathf.Max(iconsEnd, textsEnd);

        for (var elapsed = 0f; elapsed < total; elapsed += Time.deltaTime)
        {
            for (var i = 0; i < icons.Count; i++)
            {
                var t = Mathf.Clamp01((elapsed - i * iconStagger) / iconDuration);
                var pop = EaseOutBack(t);
                icons[i].anchoredPosition = Vector2.LerpUnclamped(burstOrigin, iconHomes[i], pop);
                icons[i].localScale = Vector3.one * pop;
                icons[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(-180f, 0f, EaseOutCubic(t)));
            }
            for (var i = 0; i < texts.Count; i++)
            {
                var t = EaseOutCubic(Mathf.Clamp01((elapsed - textDelay - i * textStagger) / textDuration));
                texts[i].anchoredPosition = textHomes[i] + Vector2.left * (textSlideDistance * (1f - t));
                textGroups[i].alpha = t;
            }
            yield return null;
        }

        for (var i = 0; i < icons.Count; i++)
        {
            icons[i].anchoredPosition = iconHomes[i];
            icons[i].localScale = Vector3.one;
            icons[i].localRotation = Quaternion.identity;
        }
        for (var i = 0; i < texts.Count; i++)
        {
            texts[i].anchoredPosition = textHomes[i];
            textGroups[i].alpha = 1f;
        }

        introRoutine = null;
        idle = true;
    }

    // overshoots a little then settles
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // fast then slow
    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
