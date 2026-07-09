using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Pause
{
    public class PauseView : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitToMenuButton;

        public event Action OnResumeClicked;
        public event Action OnQuitToMenuClicked;

        private void Awake()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());

            if (quitToMenuButton != null)
                quitToMenuButton.onClick.AddListener(() => OnQuitToMenuClicked?.Invoke());
        }
    }
}
