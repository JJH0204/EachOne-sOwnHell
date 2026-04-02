using UnityEngine;

/// <summary>
/// 각자의 지옥 - 쿼터뷰(등각) 카메라 컨트롤러
/// 플레이어를 따라가며 고정된 쿼터뷰 각도를 유지합니다.
/// </summary>
public class QuarterviewCamera : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform target;

    [Header("Offset & Angle")]
    public Vector3 offset = new Vector3(-9f, 13f, -9f);
    public float pitchAngle = 45f;   // 수직 기울기
    public float yawAngle   = 45f;   // 수평 회전 (45 = 등각)

    [Header("Smoothing")]
    public float smoothSpeed = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position  = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.rotation  = Quaternion.Euler(pitchAngle, yawAngle, 0f);
    }
}
