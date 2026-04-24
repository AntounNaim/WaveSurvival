using UnityEngine;

public interface IPoolable
{
    GameObject gameObject { get; }
    void OnGetFromPool();
    void OnReturnFromPool();
}