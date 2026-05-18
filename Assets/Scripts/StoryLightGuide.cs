using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LightingStage
{
    // stageName: Tên giai đoạn — hiện trong Inspector để dễ phân biệt
    // Ví dụ: "Tìm Dao", "Tìm Nhật Ký", "Tìm Dấu Vân Tay"
    public string stageName = "Stage";
    // evidenceID: ID của vật phẩm trong EvidenceData.evidenceID
    // Khi player nhặt evidence có ID này → stage này kết thúc → stage kế bật
    // Để trống ("") nếu đây là stage cuối không cần unlock điều kiện
    public string evidenceID = "";
    // guideLights: Các đèn sẽ BẬT khi stage này đang active
    // → Đặt đèn dọc hành lang, hướng vào phòng chứa evidence
    // → Có thể là Point Light, Spot Light, hoặc Area Light (tùy loại cần chiếu)
    public Light[] guideLights;
    // ambientLights: Đèn nền yếu hơn, luôn bật trong giai đoạn này
    // → Dùng để chiếu nhẹ lên tường hoặc trần, tạo bầu không khí
    // → Intensity thấp hơn guideLights (0.1 - 0.5)
    public Light[] ambientLights;
    // guideIntensity: Độ sáng của guideLights khi bật
    // Gợi ý: 0.8 - 1.5 (ánh đèn yếu, run rẩy)
    public float guideIntensity = 1f;
    // guideColor: Màu đèn hành lang
    // → Vàng đỏ (#FF6A2A) = đèn cổ điển, ấm, Gothic
    // → Tím xanh (#8A3FFF) = ánh sáng ma quái, siêu nhiên
    // → Trắng xanh lạnh (#A0C8FF) = ánh trăng lọt qua cửa sổ
    public Color guideColor = new Color(1f, 0.42f, 0.17f); // cam vàng
    // fadeDuration: Thời gian đèn fade in/out (giây)
    // → Đèn bật dần lên thay vì bật đột ngột → tự nhiên hơn, ít giật hơn
    [Range(0.5f, 5f)]
    public float fadeDuration = 1.5f;
    // flickerOnActivate: Đèn nhấp nháy 1-2 lần khi mới bật
    // → Tạo cảm giác đèn cũ, hỏng hóc — rất phù hợp game kinh dị
    public bool flickerOnActivate = true;
}
public class StoryLightGuide : MonoBehaviour
{
    [Header("Danh Sách Giai Đoạn")]
    // stages: Mỗi phần tử là 1 giai đoạn cốt truyện
    // Thứ tự trong mảng = thứ tự thực hiện (index 0 đầu tiên)
    public LightingStage[] stages;
    [Header("Cài Đặt Kiểm Tra")]
    // checkInterval: Kiểm tra EvidenceManager bao lâu một lần (giây)
    // → Không kiểm tra mỗi frame để tiết kiệm hiệu năng
    // → Gợi ý: 0.5 - 1.0 giây (người chơi không cảm nhận được delay này)
    public float checkInterval = 0.5f;
    [Header("Đèn Khẩn Cấp (Emergency)")]
    // emergencyLight: Đèn dự phòng bật khi TẤT CẢ evidence đã thu thập
    // → Dùng để chiếu sáng khu vực cáo buộc (AccuseTrigger)
    public Light[] endgameLights;
    public Color EndGameColor = new Color(1f, 0.42f, 0.17f);
    public float endgameIntensity = 2f;
 
    // =============================================
    // BIẾN NỘI BỘ
    // =============================================
    private int currentStageIndex = 0;
    private bool allStagesDone = false;
    private void Start()
    {
        // Tắt toàn bộ đèn của mọi stage trước
        foreach (var stage in stages)
        {
            SetStageLights(stage, 0f);
        }
        // Tắt đèn endgame
        if (endgameLights != null)
            foreach (var l in endgameLights) { if (l) l.enabled = false; }
        // Bật stage đầu tiên (stage 0)
        if (stages.Length > 0)
            StartCoroutine(ActivateStage(stages[0]));
        // Bắt đầu vòng kiểm tra tiến độ
        StartCoroutine(CheckProgressLoop());
    }
    private IEnumerator CheckProgressLoop()
    {
        while (!allStagesDone)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckAndAdvanceStage();
        }
    }
    private void CheckAndAdvanceStage()
    {
        if (currentStageIndex >= stages.Length) return;
        LightingStage current = stages[currentStageIndex];
        // Nếu stage này không có evidenceID → không tự chuyển
        if (string.IsNullOrEmpty(current.evidenceID)) return;
        // Kiểm tra xem player đã nhặt evidence của stage này chưa
        if (EvidenceManager.Instance != null &&
            EvidenceManager.Instance.HasEvidence(current.evidenceID))
        {
            AdvanceToNextStage();
        }
    }
    private void AdvanceToNextStage()
    {
        // Tắt đèn stage hiện tại
        StartCoroutine(FadeOutStage(stages[currentStageIndex]));
        currentStageIndex++;
        if (currentStageIndex < stages.Length)
        {
            // Bật đèn stage tiếp theo
            StartCoroutine(ActivateStage(stages[currentStageIndex]));
        }
        else
        {
            // Đã qua hết tất cả stage → bật đèn endgame
            allStagesDone = true;
            StartCoroutine(ActivateEndgame());
        }
    }
    // Coroutine: Fade IN đèn của 1 stage (kèm hiệu ứng nhấp nháy khi bật)
    private IEnumerator ActivateStage(LightingStage stage)
    {
        // Bật đèn trước
        foreach (var l in stage.guideLights)
        {
            if (l == null) continue;
            l.enabled = true;
            l.color = stage.guideColor;
            l.intensity = 0f;
        }
        foreach (var l in stage.ambientLights)
        {
            if (l == null) continue;
            l.enabled = true;
            l.color = stage.guideColor;
            l.intensity = 0f;
        }
        // Nhấp nháy một lần khi mới bật (nếu bật tùy chọn)
        if (stage.flickerOnActivate)
            yield return StartCoroutine(FlickerOnce(stage.guideLights, stage.guideIntensity));
        // Fade in từ 0 → intensity mục tiêu
        float elapsed = 0f;
        while (elapsed < stage.fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stage.fadeDuration;
            foreach (var l in stage.guideLights)
                if (l) l.intensity = Mathf.Lerp(0f, stage.guideIntensity, t);
            foreach (var l in stage.ambientLights)
                if (l) l.intensity = Mathf.Lerp(0f, stage.guideIntensity * 0.3f, t);
            yield return null;
        }
        // Đảm bảo đúng giá trị cuối
        SetStageLights(stage, stage.guideIntensity);
    }
    // Coroutine: Fade OUT đèn của 1 stage
    private IEnumerator FadeOutStage(LightingStage stage)
    {
        float startIntensity = stage.guideIntensity;
        float elapsed = 0f;
        float outDuration = stage.fadeDuration * 0.6f; // tắt nhanh hơn bật
        while (elapsed < outDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / outDuration;
            foreach (var l in stage.guideLights)
                if (l) l.intensity = Mathf.Lerp(startIntensity, 0f, t);
            foreach (var l in stage.ambientLights)
                if (l) l.intensity = Mathf.Lerp(startIntensity * 0.3f, 0f, t);
            yield return null;
        }
        SetStageLights(stage, 0f);
    }
    // Hiệu ứng nhấp nháy 1 lần khi đèn mới kích hoạt
    private IEnumerator FlickerOnce(Light[] lights, float targetIntensity)
    {
        for (int i = 0; i < 3; i++)
        {
            float val = (i % 2 == 0) ? targetIntensity : 0f;
            foreach (var l in lights)
                if (l) l.intensity = val;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
        }
    }
    private void SetStageLights(LightingStage stage, float intensity)
    {
        foreach (var l in stage.guideLights)
        {
            if (l == null) continue;
            l.intensity = intensity;
            l.enabled = intensity > 0f;
        }
        foreach (var l in stage.ambientLights)
        {
            if (l == null) continue;
            l.intensity = intensity * 0.3f;
            l.enabled = intensity > 0f;
        }
    }
    private IEnumerator ActivateEndgame()
    {
        if (endgameLights == null) yield break;
        foreach (var l in endgameLights)
        {
            if (l == null) continue;
            l.enabled = true;
            l.color = EndGameColor;
            l.intensity = 0f;
        }
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            foreach (var l in endgameLights)
                if (l) l.intensity = Mathf.Lerp(0f, endgameIntensity, elapsed / 2f);
            yield return null;
        }
    }
    /// <summary>
    /// Gọi từ bên ngoài (ví dụ debug) để nhảy thẳng đến stage theo index.
    /// Hữu ích khi test nhanh mà không muốn nhặt từng evidence.
    /// </summary>
    public void DebugJumpToStage(int index)
    {
        if (index >= 0 && index < stages.Length)
        {
            StartCoroutine(FadeOutStage(stages[currentStageIndex]));
            currentStageIndex = index;
            StartCoroutine(ActivateStage(stages[currentStageIndex]));
        }
    }
    /// <summary>
    /// Tắt/bật các đèn thuộc Stage (giai đoạn) đang active hiện tại.
    /// Dùng cho Jumpscare để chỉ tắt những đèn nào đang dẫn đường.
    /// </summary>
    public void SetCurrentStageLights(bool on)
    {
        if (currentStageIndex >= 0 && currentStageIndex < stages.Length)
        {
            LightingStage current = stages[currentStageIndex];
            foreach (var l in current.guideLights) if (l != null) l.enabled = on;
            foreach (var l in current.ambientLights) if (l != null) l.enabled = on;
        }
    }
}
