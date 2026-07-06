using UnityEngine;
using TMPro;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private PlayerModel model;

    public void Initialize(PlayerModel model)
    {
        this.model = model;

        model.OnDepthChanged += UpdateDepthUI;
    }

    private void UpdateDepthUI(float depth)
    {
        text.text = $"{Mathf.RoundToInt(depth)}m";
    }

    
    private void OnDestroy()
    {
        if (model != null)
        {
            model.OnDepthChanged -= UpdateDepthUI;
        }
    }
}