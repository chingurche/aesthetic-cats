using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine;

public abstract class BaseSceneManager : MonoBehaviour
{
    public UnityEvent<string> OnSceneChanged = new UnityEvent<string>();

    public abstract UniTask Initialize();
}