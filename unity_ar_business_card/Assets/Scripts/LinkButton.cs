using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Opens a link when pressed, with a click sound and a bounce.</summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class LinkButton : MonoBehaviour
{
    /// <summary>Link to open (https:// or mailto:).</summary>
    public string url;

    /// <summary>Color flashed on press.</summary>
    public Color flashColor = new Color(1f, 0.85f, 0.8f, 1f);

    /// <summary>How small the button gets when pressed.</summary>
    [Range(0.5f, 1f)]
    public float pressedScale = 0.85f;

    /// <summary>Length of the press effect before the link opens.</summary>
    public float feedbackDuration = 0.2f;

    // one click sound shared by all buttons
    private static AudioClip clickClip;

    // the button on this object
    private Button button;

    // what flashes: the icon circle or the text
    private Graphic graphic;

    // plays the click
    private AudioSource audioSource;

    // ignore taps while the press effect runs
    private bool busy;

    // hook up the click and make sure the graphic receives taps
    private void Awake()
    {
        button = GetComponent<Button>();
        graphic = button.targetGraphic != null ? button.targetGraphic : GetComponent<Graphic>();
        if (graphic != null)
            graphic.raycastTarget = true;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        if (clickClip == null)
            clickClip = CreateClickClip();

        button.onClick.AddListener(OnPressed);
    }

    // unhook the click
    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnPressed);
    }

    // button click handler
    private void OnPressed()
    {
        if (busy) return;
        StartCoroutine(PressThenOpen());
    }

    // click, bounce and flash, then open the link
    private IEnumerator PressThenOpen()
    {
        busy = true;
        audioSource.PlayOneShot(clickClip);

        var startColor = graphic != null ? graphic.color : Color.white;
        var rest = transform.localScale;

        for (var elapsed = 0f; elapsed < feedbackDuration; elapsed += Time.unscaledDeltaTime)
        {
            var wave = Mathf.Sin(elapsed / feedbackDuration * Mathf.PI);
            transform.localScale = rest * Mathf.Lerp(1f, pressedScale, wave);
            if (graphic != null)
                graphic.color = Color.Lerp(startColor, flashColor, wave);
            yield return null;
        }

        transform.localScale = rest;
        if (graphic != null)
            graphic.color = startColor;

        if (!string.IsNullOrWhiteSpace(url))
            Application.OpenURL(url.Trim());
        else
            Debug.LogWarning($"{name}: no URL set on LinkButton.");

        busy = false;
    }

    // short 1 kHz tick that fades out
    private static AudioClip CreateClickClip()
    {
        const int sampleRate = 44100;
        var samples = new float[(int)(sampleRate * 0.06f)];
        for (var i = 0; i < samples.Length; i++)
        {
            var t = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * 1000f * t) * Mathf.Exp(-t * 60f) * 0.5f;
        }
        var clip = AudioClip.Create("Click", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
