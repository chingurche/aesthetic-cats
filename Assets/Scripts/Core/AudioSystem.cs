using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AudioSystem
{
    public void Initialize()
    {
        
    }

    public async UniTask PlaySound(string address, Vector3 position, float volume = 1f)
    {
        var clip = await Addressables.LoadAssetAsync<AudioClip>(address).ToUniTask();
        
        GameObject go = new GameObject($"TempAudio_{address}");
        go.transform.position = position;
        
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.maxDistance = 20f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.Play();
        
        Object.Destroy(go, clip.length + 0.1f);
        
        Debug.Log($"🔊 Sound: {address} at {position}");
    }

    public async UniTask PlayUISound(string address)
    {
        var clip = await Addressables.LoadAssetAsync<AudioClip>(address).ToUniTask();
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }
}