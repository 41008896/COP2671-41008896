using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace RhythmGameStarter
{
    [RequireComponent(typeof(CanvasGroup))]
    public class View : MonoBehaviour
    {
        public bool hideOnStart = true;
        public bool resetPosition = true;
        public float transitionTime = 0.5f;
        public bool pauseGameWhenVisible;

        private CanvasGroup canvasGroup;

        [Title("Events"), CollapsedEvent]
        public UnityEvent onShow;
        [CollapsedEvent]
        public UnityEvent onHide;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (resetPosition)
            {
                transform.localPosition = Vector3.zero;
            }
        }

        private void Start()
        {
            if (hideOnStart)
            {
                InstantHide();
            }
        }

        public void Show()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            gameObject.SetActive(true);
            onShow.Invoke();
            fadeCoroutine = StartCoroutine(FadeIn());

            if (pauseGameWhenVisible)
            {
                Time.timeScale = 0;
            }
        }

        public void Hide()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            onHide.Invoke();
            fadeCoroutine = StartCoroutine(FadeOut());

            if (pauseGameWhenVisible)
            {
                Time.timeScale = 1;
            }
        }

        public void InstantHide()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        private IEnumerator FadeIn()
        {
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1, Time.unscaledDeltaTime / transitionTime);
                yield return null;
            }

            fadeCoroutine = null; // Clear the reference when done
        }

        private IEnumerator FadeOut()
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0, Time.unscaledDeltaTime / transitionTime);
                yield return null;
            }

            gameObject.SetActive(false); // Deactivate when fully hidden
            fadeCoroutine = null; // Clear the reference when done
        }
    }
}
