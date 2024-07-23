using System.Collections;
using UnityEngine;

public class DelayLeftClick : MonoBehaviour
{
    private bool canClick = true;
    public float delayTime = 1f;

    private void Update()
    {
        // Sol tıklama algılama
        if (Input.GetMouseButtonDown(0) && canClick)
        {
            StartCoroutine(DelayClick());
        }
    }

    private IEnumerator DelayClick()
    {
        // Sol tıklamayı devre dışı bırak
        canClick = false;

        // Belirtilen süre boyunca bekle
        yield return new WaitForSeconds(delayTime);

        // Sol tıklamayı tekrar etkinleştir
        canClick = true;
    }
}
