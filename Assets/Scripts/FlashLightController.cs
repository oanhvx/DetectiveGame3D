using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightController : MonoBehaviour
{
    [Header("Đèn pin")]
    public Light flashLight;
    public KeyCode toggleKey = KeyCode.F;
    public bool startOn = false;
    [Header("Thông số đèn pin")]
    public float intensity = 3f;
    public float range = 10f;
    public float spotAngle = 35f;
    public Color lightColor = new Color(0.91f, 0.94f, 1f);
    [Header("Hiệu ứng nhấp nháy")]
    public bool enableFlicker = false;
    public float flickerSpeed = 0.1f;
    public float flickerIntensityMin = 1.5f;
    public float flickerIntensityMax = 3.5f;
    [Header("Âm thanh")]
    public AudioClip clickSound;
    private AudioSource audioSource;
    [HideInInspector]
    public bool isFlashlightOn = false;
    private float flickerTimer = 0f;
    private void Start()
    {
        if(flashLight != null)
        {
            flashLight.range = range;
            flashLight.intensity = startOn ? intensity : 0f;
            flashLight.color = lightColor;
            flashLight.spotAngle = spotAngle;
            flashLight.enabled = startOn;
        }
        isFlashlightOn = startOn;
    }
    private void Update()
    {
        HangleToggleInput();
        if (enableFlicker && isFlashlightOn)
        {
            HangleFlicker();
        }
    }
    private void HangleToggleInput()
    {
        if (DialogueManager.instance.isDialogueActive)
        {
            return;
        }
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight(!isFlashlightOn);
        }
    }
    public void ToggleFlashlight(bool on)
    {
        isFlashlightOn = on;

        if (flashLight != null)
        {
            flashLight.enabled = on;
            flashLight.intensity = on ? intensity : 0f;
        }
        if(clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, 0.8f);
        }
    }
    private void HangleFlicker()
    {
        flickerTimer -= Time.deltaTime;
        if (flickerTimer <= 0)
        {
            if (flashLight != null)
            {
                flashLight.intensity = Random.Range(flickerIntensityMin, flickerIntensityMax);
                flickerTimer = Random.Range(flickerSpeed * 0.5f, flickerSpeed * 1.5f);
            }
        }
    }

}
