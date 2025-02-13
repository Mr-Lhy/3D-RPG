using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;

    public Transform barPoint;

    public bool alwaysVisable;

    public float visibleTime;
    private float timeLeft;

    Image healthSlider;
    Transform UIBar;
    Transform cam;

    CharaterStats currentStats;

    private void Awake()
    {
        currentStats = GetComponent<CharaterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        cam = Camera.main.transform;

        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                UIBar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisable);

            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if(UIBar == null) { return; }

        if(currentHealth <= 0)
        {
            Destroy(UIBar.gameObject);
        }

        UIBar.gameObject.SetActive(true);
        timeLeft = visibleTime;
        
        float sliderPercent = (float)currentHealth / maxHealth;

        healthSlider.fillAmount = sliderPercent;
    }

    private void LateUpdate()
    {
        if(UIBar != null)
        {
            UIBar.position = barPoint.position;
            UIBar.forward = -cam.forward;

            if(timeLeft <= 0 && !alwaysVisable)
            {
                UIBar.gameObject.SetActive(false);
            }
            else
            {
                timeLeft -=Time.deltaTime;
            }
        }
    }
}
