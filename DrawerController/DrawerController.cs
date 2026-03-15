using UnityEngine;
using System.Collections;
using UnityEditor;

public class DrawerController : MonoBehaviour, ITargetable
{
    public float openDistance = 0.002f;
    public float moveDuration = 0.35f;
    public Vector3 moveDirection = Vector3.forward;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isRotational = false;
    private bool isOpened = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine movementCoroutine = null;

    private Renderer[] renderers;

    void Start()
    {
        closedPosition = transform.localPosition;
        closedRotation = transform.localRotation;

        // Dolabın altındaki tüm rendererları alıyoruz.
        // DİKKAT: Buna dolabın içindeki toplanabilir eşyalar da dahil olabilir.
        renderers = GetComponentsInChildren<Renderer>();

        string objName = gameObject.name.ToLower();

        if (objName.Contains("drawer3left"))
        {
            isRotational = true;
            openRotation = closedRotation * Quaternion.Euler(0, 120, 0);
        }
        else if (objName.Contains("drawer3right"))
        {
            isRotational = true;
            openRotation = closedRotation * Quaternion.Euler(0, -120, 0);
        }
        else if (objName.Contains("drawer4left"))
        {
            isRotational = true;
            openRotation = closedRotation * Quaternion.Euler(0, 0, 100);
        }
        else if (objName.Contains("drawer4right"))
        {
            isRotational = true;
            openRotation = closedRotation * Quaternion.Euler(0, 0, -96);
        }
        else if (objName.Contains("drawer2"))
        {
            isRotational = false;
            openPosition = closedPosition + new Vector3(0f, 0f, 0.36f);
        }
        else if (objName.Contains("drawer4"))
        {
            isRotational = false;
            openPosition = closedPosition + new Vector3(0.005f, 0f, 0.0f);
        }
        else if (objName.Contains("drawer5"))
        {
            isRotational = false;
            openPosition = closedPosition + new Vector3(0f, 0f, 1.7f);
        }
        else if (objName.Contains("kitchendrawer1"))
        {
            isRotational = false;
            openPosition = closedPosition + new Vector3(1f, 0f, 0.0f);
        }
        else if (objName.Contains("drawer6"))
        {
            isRotational = false;
            openPosition = closedPosition + new Vector3(0f, 0f, -0.4f);
        }
        else
        {
            isRotational = false;
            Vector3 localOffset = transform.localRotation * moveDirection.normalized * openDistance;
            openPosition = closedPosition + localOffset;
        }
    }

    public void ToggleDrawer()
    {
        isOpened = !isOpened;
        if (VoiceManager.Instance != null)
        {
            if (isOpened)
            {
                if (gameObject.name == "drawer3left" || gameObject.name == "drawer3right" || gameObject.name == "drawer4left" || gameObject.name == "drawer4right")
                {
                    VoiceManager.Instance.PlayBigDrawerOpen();
                }
                else
                {
                    VoiceManager.Instance.PlayDrawerOpen();
                }

            }
            else
            {
                if (gameObject.name == "drawer3left" || gameObject.name == "drawer3right" || gameObject.name == "drawer4left" || gameObject.name == "drawer4right")
                {
                    VoiceManager.Instance.PlayBigDrawerClose();
                }
                else
                {
                    VoiceManager.Instance.PlayDrawerClose();
                }

            }

        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }


        if (isRotational)
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

    IEnumerator MoveObject(Vector3 targetPosition)
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

    IEnumerator RotateObject(Quaternion targetRotation)
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

    // 🔹 ITargetable arayüzü için eklenen metodlar
    public void ToggleHighlight(bool state)
    {
        if (renderers == null) return;

        foreach (var r in renderers)
        {
            // DÜZELTME BURADA: Eğer renderer yok edildiyse (pil alındıysa) null kontrolü yap
            if (r == null) continue;

            if (r.material.HasProperty("_EmissionColor"))
                r.material.SetColor("_EmissionColor", state ? Color.yellow * 1f : Color.black);
        }
    }

    public void Interact()
    {
        ToggleDrawer();
    }
}