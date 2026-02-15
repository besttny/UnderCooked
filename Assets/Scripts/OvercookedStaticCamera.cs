using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class OvercookedStaticCamera : MonoBehaviour
{
    [Header("Map (ลาก LevelBuilder ที่มี GridMapBuilder มาใส่ได้)")]
    public GridMapBuilder map;

    [Header("Angle (เหมือน Overcooked)")]
    [Range(20f, 80f)] public float pitch = 55f;     // ก้มลง (X)
    [Range(-180f, 180f)] public float yaw = 0f;    // หมุนเฉียง (Y)
    public float targetHeight = 0.5f;               // ยกจุดที่กล้องเล็งขึ้นนิดนึง

    [Header("Framing")]
    public float padding = 1.5f;
    public bool useOrthographic = false;            // แนะนำ false (Perspective จะเหมือน Overcooked กว่า)
    public float fieldOfView = 40f;                 // ใช้ตอน Perspective
    public float minDistance = 6f;

    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        Reframe();
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        Reframe();
    }

    void OnValidate()
    {
        cam = GetComponent<Camera>();
        Reframe();
    }

    void LateUpdate()
    {
        // ให้เห็นผลใน Editor ทันทีตอนปรับค่า
        if (!Application.isPlaying) Reframe();
    }

    [ContextMenu("Reframe Now")]
    public void Reframe()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (map == null) map = FindObjectOfType<GridMapBuilder>();
        if (map == null) return;

        float w = map.width * map.tileSize;
        float h = map.height * map.tileSize;

        // center ของกริด (จาก 0..width-1)
        Vector3 centerLocal = new Vector3(
            (w - map.tileSize) * 0.5f,
            map.floorY + targetHeight,
            (h - map.tileSize) * 0.5f
        );

        Vector3 center = map.transform.position + centerLocal;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = rot;

        // radius ครอบทั้งแมพแบบวงกลม (กันมุมหลุด)
        float halfW = (w * 0.5f);
        float halfH = (h * 0.5f);
        float radius = Mathf.Sqrt(halfW * halfW + halfH * halfH) + padding;

        if (useOrthographic)
        {
            cam.orthographic = true;

            // ให้พอดีทั้งแนวตั้ง/แนวนอน (คิด aspect)
            float sizeByHeight = radius;
            float sizeByWidth = radius / Mathf.Max(0.01f, cam.aspect);
            cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

            float dist = Mathf.Max(minDistance, 10f);
            transform.position = center - (rot * Vector3.forward) * dist;
        }
        else
        {
            cam.orthographic = false;
            cam.fieldOfView = fieldOfView;

            // คำนวณระยะให้พอดีเฟรมจาก FOV
            float vFov = cam.fieldOfView * Mathf.Deg2Rad;
            float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * cam.aspect);

            float distV = radius / Mathf.Tan(vFov * 0.5f);
            float distH = radius / Mathf.Tan(hFov * 0.5f);
            float dist = Mathf.Max(minDistance, Mathf.Max(distV, distH));

            transform.position = center - (rot * Vector3.forward) * dist;
        }
    }
}
