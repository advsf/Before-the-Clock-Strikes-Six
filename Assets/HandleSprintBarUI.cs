using UnityEngine;
using UnityEngine.UI;

public class HandleSprintBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider sprintSlider;
    [SerializeField] private HandlePlayerMovement pm;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float smoothFactor = 3f;
    [SerializeField] private float timeBeforeFadeOut = 2f;

    private int fadeInHash;
    private int fadeOutHash;

    private bool isVisible;
    private float recoverTimer;
    private float previousStamina;

    private void Start()
    {
        fadeInHash = Animator.StringToHash("FadeIn");
        fadeOutHash = Animator.StringToHash("FadeOut");

        previousStamina = pm.currentStamina;
    }

    private void Update()
    {
        HandleFadeAnimations();

        float target = pm.currentStamina / pm.maxStamina;
        sprintSlider.value = Mathf.MoveTowards(sprintSlider.value, target, smoothFactor * Time.deltaTime);
        previousStamina = pm.currentStamina;
    }

    private void HandleFadeAnimations()
    {
        // the stamina is decreasing
        if (pm.currentStamina < previousStamina)
        {
            recoverTimer = 0f;

            if (!isVisible)
            {
                animator.SetTrigger(fadeInHash);
                isVisible = true;
            }
        }

        // stamina is increasing
        else
        {
            recoverTimer += Time.deltaTime;

            if (isVisible && recoverTimer >= timeBeforeFadeOut)
            {
                animator.SetTrigger(fadeOutHash);
                isVisible = false;
            }
        }
    }
}
