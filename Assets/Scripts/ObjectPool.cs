using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component, IPoolable
{
    readonly T prefab;
    readonly Transform parent;
    readonly Queue<T> available = new Queue<T>();
    readonly HashSet<T> active = new HashSet<T>();

    public int CountActive => active.Count;
    public int CountInactive => available.Count;

    public ObjectPool(T prefab, Transform parent, 
        int prewarmCount)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < prewarmCount; i++)
        {
            T instance = CreateInstance();
            instance.gameObject.SetActive(false);
            available.Enqueue(instance);
        }
    }
    // when the spawner needs an enemy. 
    public T Get(Vector3 position, Quaternion rotation) {
        
        // Dequeue from available (or instantiate a new one if the queue is empty)
        T instance = available.Count > 0 ? available.Dequeue() : CreateInstance();
        // Set its position and rotation
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);
        // Set its position and rotation

        active.Add(instance);
        instance.OnGetFromPool();
        return instance;
    }

    // returning enemy to pool
    public void Return(T instance)
    {
        if (!active.Remove(instance)) { return; }
        
        // return entity to pool
        instance.OnReturnFromPool();
        // set active false
        instance.gameObject.SetActive(false);

        // enqueue instance to available list
        available.Enqueue(instance);
    }

    public void ReturnAll() {
        foreach (T instance in active)
        {
            // return from pool
            instance.OnReturnFromPool();
            //set game object active to false
            instance.gameObject.SetActive(false);
            // enqueue in available
            available.Enqueue(instance);

        }
        // clear all active entities
        active.Clear();
    }

    T CreateInstance() {
        T instance = Object.Instantiate(prefab, parent);
        return instance;
    }
}
