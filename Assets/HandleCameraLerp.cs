using System.Collections;
using UnityEngine;

public class HandleCameraLerp : MonoBehaviour
{
    [SerializeField] private Transform camHolder;

    private float originalY;

    public IEnumerator MoveCameraDownWhileCrouching(float cameraMoveSpeed)
    {
        originalY = camHolder.localPosition.y;
        float targetY = originalY - 0.5f;

        while (Mathf.Abs(camHolder.localPosition.y - targetY) > 0.01f)
        {
            Vector3 nextPos = camHolder.localPosition;
            nextPos.y = Mathf.MoveTowards(nextPos.y, targetY, cameraMoveSpeed * Time.deltaTime);
            camHolder.localPosition = nextPos;

            yield return null;
        }
    }

    public IEnumerator MoveCameraUpWhileCrouching(float cameraMoveSpeed)
    {
        while (Mathf.Abs(camHolder.localPosition.y - originalY) > 0.01f)
        {
            Vector3 nextPos = camHolder.localPosition;
            nextPos.y = Mathf.MoveTowards(nextPos.y, originalY, cameraMoveSpeed * Time.deltaTime);
            camHolder.localPosition = nextPos;

            yield return null;
        }
    }
}
