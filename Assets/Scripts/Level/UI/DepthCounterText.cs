using UnityEngine;
using TMPro;

public class DepthCounterText : MonoBehaviour
{
    [SerializeField] public DepthCounter depthCounter;
    [SerializeField] private TextMeshProUGUI text;

    private void SetDepthText(float depth)
    {
        
        text.text = $"{Mathf.RoundToInt(depth)}m";
    }

    
    private void OnEnable()
    {
        depthCounter.OnDepthChanged.AddListener(SetDepthText);
    }

    private void OnDisable()
    {
        depthCounter.OnDepthChanged.RemoveListener(SetDepthText);
    }
}