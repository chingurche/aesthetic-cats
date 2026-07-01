using UnityEngine;

[ExecuteAlways]
public class PlantInteractor : MonoBehaviour
{
    private static readonly int UnitPosition = Shader.PropertyToID("_UnitPosition");

    void Update()
    {
        Shader.SetGlobalVector(UnitPosition, transform.position);
    }
}
