using UnityEngine;

public class MiniMapUIIcons : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform mapRect;       
    public RectTransform playerIcon;     
    public RectTransform bossIcon;       

    [Header("World References")]
    public Transform player;
    public Camera miniMapCam;

    [Header("Boss (optional)")]
    public bool autoFindBossByTag = true;
    public string bossTag = "Boss";
    public Transform boss;

    [Header("Options")]
    public bool playerIconFixedCenter = true;
    public bool showBossIconOnlyWhenFound = true;

    void LateUpdate()
    {
        if (mapRect == null || miniMapCam == null || player == null)
            return;

       
        if (playerIconFixedCenter && playerIcon != null)
            playerIcon.anchoredPosition = Vector2.zero;

      
        if (boss == null && autoFindBossByTag)
        {
            GameObject b = GameObject.FindGameObjectWithTag(bossTag);
            if (b != null) boss = b.transform;
        }

      
        if (boss == null)
        {
            if (bossIcon != null && showBossIconOnlyWhenFound)
                bossIcon.gameObject.SetActive(false);
            return;
        }

        if (bossIcon != null && !bossIcon.gameObject.activeSelf)
            bossIcon.gameObject.SetActive(true);

        
        Vector3 vp = miniMapCam.WorldToViewportPoint(boss.position);

        
        if (vp.z < 0f)
        {
            if (bossIcon != null) bossIcon.gameObject.SetActive(false);
            return;
        }

        
        float x = Mathf.Clamp01(vp.x);
        float y = Mathf.Clamp01(vp.y);

       
        float px = (x - mapRect.pivot.x) * mapRect.rect.width;
        float py = (y - mapRect.pivot.y) * mapRect.rect.height;

        if (bossIcon != null)
            bossIcon.anchoredPosition = new Vector2(px, py);
        
    }
}
