using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// HorrorNPCChaser — Logic 3 pha cho NPC kinh dị tích hợp cốt truyện.
///
/// PHA 1: Người chơi vào vùng → đèn tắt → đèn tay bật → NPC đi NavMesh đến
///         trước mặt → hội thoại bắt buộc → NPC chạy về vị trí gốc.
///
/// CHUYỂN TIẾP: Chờ người chơi nhặt đúng vật chứng (evidenceIDToWatch).
///
/// PHA 2: Đèn tắt → NPC teleport ra sau lưng (trong tối) → đèn tay bật →
///         NPC đi NavMesh ra trước mặt → hội thoại lần 2 → NPC chạy về AccuseTarget.
///
/// Đèn hành lang được quản lý tập trung qua StoryLightGuide — chỉ kéo đèn
/// vào StoryLightGuide.hallwayLights một lần, không kéo riêng vào script này.
/// </summary>
public class HorrorNPCChaser : MonoBehaviour
{
    // =============================================
    // THÔNG SỐ CÀI ĐẶT TRONG INSPECTOR
    // =============================================

    [Header("References")]
    // flashlightController: Kéo FlashLightController từ PlayerCamera vào
    public FlashLightController flashlightController;
    // playerCamera: Transform của camera player (NPC đứng trước mặt)
    public Transform playerCamera;
    // playerTransform: Transform của Player GameObject
    public Transform playerTransform;
    // storyLightGuide: Kéo GameObject chứa StoryLightGuide vào đây
    // Đèn hành lang được lấy từ StoryLightGuide.hallwayLights — không cần kéo riêng
    public StoryLightGuide storyLightGuide;

    [Header("Phase 2 Target")]
    // accuseTarget: Transform khu vực buộc tội — NPC chạy về đây sau Pha 2
    // Kéo Transform của AccuseTrigger hoặc Empty GameObject đặt ở khu đó vào đây
    public Transform accuseTarget;
    // evidenceIDToWatch: ID vật chứng cần nhặt để kích hoạt Pha 2
    // Phải khớp với EvidenceData.evidenceID trong scene
    // public string evidenceIDToWatch = "";

    [Header("Trigger Setup")]
    // triggerRadius: Bán kính vùng kích hoạt Pha 1 (mét)
    // Gợi ý: 8–12m (đủ gần để bất ngờ)
    public float triggerRadius = 10f;
    [Tooltip("Layer của Player GameObject")]
    public LayerMask playerLayer;

    [Header("Movement")]
    // chaseSpeed: Tốc độ NPC di chuyển đến người chơi (m/s)
    // Gợi ý: 4–6 (đủ nhanh để bắt kịp, không quá kinh dị)
    public float chaseSpeed = 5f;
    // returnSpeed: Tốc độ NPC chạy về vị trí gốc / AccuseTarget
    public float returnSpeed = 4f;
    // stopAtDistance: Dừng lại khi còn cách camera bao nhiêu mét
    // Gợi ý: 1.2–2m (đủ gần để hội thoại tự nhiên)
    public float stopAtDistance = 1.8f;

    [Header("Phase 2 Teleport")]
    // teleportDistance: Khoảng cách NPC xuất hiện phía sau người chơi (Pha 2)
    // Gợi ý: 3–5m (trong tối — người chơi không thấy)
    [Range(2f, 8f)]
    public float teleportDistance = 1.8f;

    [Header("Animation")]
    public Animator npcAnimator;
    public string runTrigger  = "Run";
    public string idleTrigger = "Idle";
    public string talkTrigger = "IsTalking";

    [Header("Âm Thanh")]
    public AudioClip footstepSound;

    // =============================================
    // BIẾN NỘI BỘ
    // =============================================

    private enum NPCPhase
    {
        Idle,               // Chờ người chơi vào vùng
        Phase1_Moving,      // Đang đi đến trước mặt người chơi (Pha 1)
        Phase1_Talking,     // Đang hội thoại Pha 1
        Phase1_Returning,   // Đang chạy về vị trí gốc
        WaitingForEvidence, // Chờ người chơi nhặt vật chứng
        Phase2_Behind,      // Đã teleport ra sau lưng, đang đi ra trước mặt
        Phase2_Talking,     // Đang hội thoại Pha 2
        Done                // Đã chạy về khu vực buộc tội — kết thúc
    }
    private NPCInteraction npcInteraction;
    private NPCPhase   currentPhase = NPCPhase.Idle;
    private NavMeshAgent navAgent;
    private AudioSource  audioSource;
    private Vector3      originalPosition;
    private Quaternion   originalRotation;

    // =============================================
    // UNITY LIFECYCLE
    // =============================================

    private void Start()
    {
        // Cache component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("[HorrorNPC] Thiếu AudioSource trên NPC!");

        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
            Debug.LogError("[HorrorNPC] Thiếu NavMeshAgent! Hãy thêm component NavMeshAgent và Bake NavMesh.");

        // if (npcInteraction == null)
        npcInteraction = GetComponent<NPCInteraction>();
        // if (npcInteraction == null)
        //     Debug.LogWarning("[HorrorNPC] Không tìm thấy NPCInteraction trên NPC này!");

        // Lưu vị trí gốc
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Cấu hình NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed           = chaseSpeed;
            navAgent.angularSpeed    = 360f;
            navAgent.acceleration    = 12f;
            navAgent.stoppingDistance = 0f;
        }
         if (accuseTarget != null)
        {
            UnityEngine.AI.NavMeshHit hit;
            // Tìm mặt sàn NavMesh gần nhất trong bán kính 5 mét theo chiều dọc
            if (UnityEngine.AI.NavMesh.SamplePosition(accuseTarget.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Ép tọa độ Y của đích đến bằng đúng tọa độ Y chuẩn của mặt sàn NavMesh
                accuseTarget.position = hit.position;
            }
        }
    }

    private void Update()
    {
        switch (currentPhase)
        {
            case NPCPhase.Idle:
                // Kiểm tra người chơi có vào vùng trigger không
                Collider[] hits = Physics.OverlapSphere(
                    transform.position, triggerRadius, playerLayer);
                if (hits.Length > 0)
                    StartCoroutine(Phase1_Sequence());
                break;

            case NPCPhase.WaitingForEvidence:
                // Chờ người chơi nhặt đúng vật chứng → kích hoạt Pha 2
                if (EvidenceManager.Instance.HasEvidence(npcInteraction.requiredEvidenceID))
                {
                    StartCoroutine(Phase2_Sequence());
                }
                break;

            // Các pha khác được coroutine tự quản lý — Update không can thiệp
        }
    }

    // =============================================
    // PHA 1: VÀO VÙNG → HỘI THOẠI → VỀ VỊ TRÍ GỐC
    // =============================================

    private IEnumerator Phase1_Sequence()
    {
        currentPhase = NPCPhase.Phase1_Moving;

        // BƯỚC 1: Tắt đèn hành lang đột ngột
        if (storyLightGuide != null)
            storyLightGuide.SetCurrentStageLights(false);

        // BƯỚC 3: NPC đi NavMesh đến trước mặt người chơi (người chơi thấy được)
        // Nếu người chơi bỏ đi, NPC đi theo đến khi đến đủ gần
        yield return StartCoroutine(MoveToFrontOfPlayer(1));

        // BƯỚC 2: Bật đèn tay (kể cả đang tắt)
        if (flashlightController != null)
            flashlightController.ToggleFlashlight(true);
        
        // BƯỚC 4: Chờ người chơi thao tác bấm E (từ PlayerInteractor)
        currentPhase = NPCPhase.Phase1_Talking;
        
        // Chờ đến khi khung hội thoại bật lên (Người chơi bấm E)
        //biến chờ điều kiện để thực thi
        yield return new WaitUntil(() => IsDialogueActive());
        
        // Chờ đến khi hội thoại kết thúc
        yield return new WaitUntil(() => !IsDialogueActive());

        // BƯỚC 5: Hội thoại xong → NPC chạy về vị trí gốc
        currentPhase = NPCPhase.Phase1_Returning;
        storyLightGuide.SetCurrentStageLights(true);
        yield return StartCoroutine(ReturnToOrigin(originalPosition));
        // BƯỚC 6: Về đến nơi → chuyển sang chờ người chơi nhặt vật chứng
        currentPhase = NPCPhase.WaitingForEvidence;
    }

    // =============================================
    // PHA 2: SAU KHI NHẶT VẬT CHỨNG → HỘI THOẠI → CHẠY VỀ KHU BUỘC TỘI
    // =============================================

    private IEnumerator Phase2_Sequence()
    {
        currentPhase = NPCPhase.Phase2_Behind;

        // BƯỚC 2: Trong bóng tối — teleport NPC ra SAU LƯNG người chơi
        // Đây là lần duy nhất được phép teleport vì đèn đang tắt → người chơi không thấy
        if (playerCamera != null)
        {
            Vector3 behindPos = playerCamera.position
                                - playerCamera.forward * teleportDistance;
            behindPos.y = originalPosition.y;

            if (navAgent != null)
            {
                // Thay vì Warp mù quáng, tìm điểm NavMesh an toàn gần behindPos nhất (bán kính 3m)
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(behindPos, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    navAgent.Warp(hit.position);
                }
                else
                {
                    // Nếu sau lưng hoàn toàn là vực/không có đường, thì Warp về vị trí gốc cho an toàn
                    navAgent.Warp(originalPosition);
                }
            }
            else
            {
                transform.position = behindPos;
            }

            FaceCamera();
        }
        // BƯỚC 3: Bật đèn tay sau 0.2 giây (người chơi chưa kịp thấy NPC đang ở đâu)
        yield return new WaitForSeconds(0.2f);
        if (flashlightController != null)
            flashlightController.ToggleFlashlight(true);

        // BƯỚC 4: NPC đi NavMesh ra trước mặt người chơi (người chơi thấy được)
        yield return StartCoroutine(MoveToFrontOfPlayer(1));

        // BƯỚC 5: Chờ người chơi thao tác bấm E (từ PlayerInteractor)
        currentPhase = NPCPhase.Phase2_Talking;

        // Chờ đến khi khung hội thoại bật lên (Người chơi bấm E)
        yield return new WaitUntil(() => IsDialogueActive());

        // Chờ hội thoại kết thúc
        yield return new WaitUntil(() => !IsDialogueActive());

        // BƯỚC 6: Hội thoại xong → NPC chạy về khu vực buộc tội
        // Không chờ — để NPC tự chạy, gameplay tiếp tục bình thường
        yield return StartCoroutine(ReturnToOrigin(accuseTarget.position));
        currentPhase = NPCPhase.Done;
    }

    // =============================================
    // COROUTINE DÙNG CHUNG
    // =============================================

    /// <summary>
    /// NPC liên tục di chuyển đến trước mặt người chơi bằng NavMesh.
    /// Nếu người chơi bỏ đi, NPC đi theo — cho đến khi đến đủ gần.
    /// Dùng chung cho cả Pha 1 và Pha 2.
    /// </summary>
    private IEnumerator MoveToFrontOfPlayer(int negative)
    {
        PlayFootstep();
        if (npcAnimator != null) npcAnimator.SetTrigger(runTrigger);

        while (true)
        {
            if (playerCamera == null) yield break;

            // Mục tiêu: đứng phía TRƯỚC mặt người chơi ở khoảng stopAtDistance
            Vector3 targetPos = playerCamera.position
                                + negative * playerCamera.forward * stopAtDistance;
            targetPos.y = originalPosition.y; // giữ độ cao NPC

            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist < 0.4f)
            {
                // Đã đến — dừng NavMesh
                if (navAgent != null) navAgent.ResetPath();
                break;
            }

            if (navAgent != null && navAgent.isOnNavMesh)
            {
                //navAgent.Warp(targetPos);
                navAgent.speed = chaseSpeed;
                navAgent.SetDestination(targetPos);
            }
            else if (navAgent != null && !navAgent.isOnNavMesh)
            {
                // Nếu NPC lỡ bị văng khỏi NavMesh, ép nó tìm lại vị trí NavMesh gần nhất
                //hàm ép NPC lên NavMesh gần nhất thông với mọi sàn allAreas
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    navAgent.Warp(hit.position);
                }
            }

            yield return null;
        }

    // Dừng âm thanh, chuyển animation về Idle, quay mặt vào camera
        StopFootstep();
        if (npcAnimator != null) npcAnimator.SetTrigger(idleTrigger);
        FaceCamera();
    }

    /// <summary>
    /// NPC chạy về vị trí gốc bằng NavMesh. Chờ đến khi về đến nơi.
    /// </summary>
    private IEnumerator ReturnToOrigin(Vector3 TargetPosition)
    {
        if (navAgent == null) yield break;

        PlayFootstep();
        if (npcAnimator != null) npcAnimator.SetTrigger(runTrigger);

        navAgent.speed = returnSpeed;
        navAgent.SetDestination(TargetPosition);

        // Chờ đến khi về đến nơi (timeout 15 giây đề phòng NavMesh bị kẹt)
        float timeout = 25f;
        while (true)
        {
            if(timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            if((!navAgent.pathPending && navAgent.remainingDistance <= 0.5f) || timeout <= 0)
            {
                break;
            }
        }

        StopFootstep();
        navAgent.ResetPath();
        transform.rotation = originalRotation;
        if (npcAnimator != null) npcAnimator.SetTrigger(idleTrigger);
    }

    // =============================================
    // HELPER METHODS
    // =============================================

    private void FaceCamera()
    {
        if (playerCamera == null) return;
        Vector3 dir = playerCamera.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void PlayFootstep()
    {
        if (audioSource == null || footstepSound == null) return;
        audioSource.clip = footstepSound;
        audioSource.loop = true;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    private void StopFootstep()
    {
        if (audioSource != null) audioSource.Stop();
    }

    /// <summary>
    /// Kiểm tra hội thoại có đang chạy không.
    /// Dùng DialogueManager.Instance.IsDialogueActive nếu có.
    /// </summary>
    private bool IsDialogueActive()
    {
        if (DialogueManager.instance != null)
            return DialogueManager.instance.isDialogueActive;
        return false;
    }

    // =============================================
    // GIZMOS — Vẽ vùng trigger trong Scene View
    // =============================================

    private void OnDrawGizmosSelected()
    {
        // Vùng trigger Pha 1
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // Vị trí gốc
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Application.isPlaying ? originalPosition : transform.position,
                            Vector3.one * 0.5f);
    }
}