using UnityEngine;

public class WorldSpaceLabelBillboard : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.enabled)
        {
            Camera[] cameras = FindObjectsByType<Camera>();
            foreach (Camera candidate in cameras)
            {
                if (candidate != null && candidate.enabled)
                {
                    targetCamera = candidate;
                    break;
                }
            }
        }

        if (targetCamera == null) return;

        Vector3 awayFromCamera = transform.position - targetCamera.transform.position;
        awayFromCamera.y = 0f;
        if (awayFromCamera.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(awayFromCamera.normalized, Vector3.up);
        }
    }
}
