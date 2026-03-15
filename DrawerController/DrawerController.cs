using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// Çekmecenin hareket tipini belirleyen Enum
public enum DrawerMovementType
{
    Slide,  // İleri/Geri kayma
    Rotate  // Kapak gibi dönme
}

public class DrawerController : MonoBehaviour, ITargetable
{
    [Header("Movement Settings")]
    public DrawerMovementType movementType = DrawerMovementType.Slide;
    public float moveDuration = 0.35f;
    
    [Tooltip("Slide seçiliyse, çekmecenin ne kadar ve hangi yöne açılacağı (Local)")]
    public Vector3 slideOffset = new Vector3(0f, 0f, 0.4f);
    
    [Tooltip("Rotate seçiliyse, kapağın hangi açıyla döneceği")]
    public Vector3 rotationOffset = new Vector3(0f, 120f, 0f);

    [Header("Highlight Settings")]
    [Tooltip("Sadece parlamasını istediğiniz Mesh'leri buraya sürükleyin. İçindeki toplanabilir eşyaları eklemeyin.")]
    public Renderer[] drawerRenderers;
    [ColorUsage(true, true)] // HDRP Emission renkleri için
    public Color highlightEmissionColor = Color.yellow;

    [Header("Audio & Events")]
    public UnityEvent onDrawerOpened;
    public UnityEvent onDrawerClosed;

    private bool isOpened = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine movementCoroutine = null;

    void Start()
    {
        // Başlangıç pozisyonlarını kaydet ve hedefleri hesapla
        closedPosition = transform.localPosition;
        openPosition = closedPosition + slideOffset;

        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(rotationOffset);

        // Eğer Inspector'dan renderer atanmamışsa, sadece bu objedekini al (Çocukları alma!)
        if (drawerRenderers == null || drawerRenderers.Length == 0)
        {
            Renderer myRenderer = GetComponent<Renderer>();
            if (myRenderer != null)
            {
                drawerRenderers = new Renderer[] { myRenderer };
            }
        }
    }

    public void Interact()
    {
        ToggleDrawer();
    }

    public void ToggleDrawer()
    {
        isOpened = !isOpened;

        // Kendi VoiceManager'ın yerine Event kullandık. 
        // İster büyük, ister küçük çekmece sesi olsun; Inspector'dan bağla.
        if (isOpened)
            onDrawerOpened?.Invoke();
        else
            onDrawerClosed?.Invoke();

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        if (movementType == DrawerMovementType.Rotate)
        {
            Quaternion target = isOpened ? openRotation : closedRotation;
            movementCoroutine = StartCoroutine(RotateObject(target));
        }
        else
        {
            Vector3 target = isOpened ? openPosition : closedPosition;
            movementCoroutine = StartCoroutine(MoveObject(target));
        }
    }

    private IEnumerator MoveObject(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.localPosition;
        float timeElapsed = 0f;

        while (timeElapsed < moveDuration)
        {
            float t = timeElapsed / moveDuration;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPosition;
        movementCoroutine = null;
    }

    private IEnumerator RotateObject(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.localRotation;
        float timeElapsed = 0f;

        while (timeElapsed < moveDuration)
        {
            float t = timeElapsed / moveDuration;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = targetRotation;
        movementCoroutine = null;
    }

    // --- ITargetable Interface Uygulaması ---
    public void ToggleHighlight(bool state)
    {
        if (drawerRenderers == null) return;

        foreach (var r in drawerRenderers)
        {
            if (r == null) continue;

            if (r.material.HasProperty("_EmissionColor"))
            {
                r.material.SetColor("_EmissionColor", state ? highlightEmissionColor : Color.black);
            }
        }
    }
}
