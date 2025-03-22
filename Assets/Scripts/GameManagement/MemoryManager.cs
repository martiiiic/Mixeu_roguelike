using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{
    private static List<IDisposable> tempAllocations = new List<IDisposable>();

    public static void RegisterTempAllocation(IDisposable tempObject)
    {
        if (tempObject != null)
        {
            Debug.Log("Registering Temp Allocation");
            tempAllocations.Add(tempObject);
        }
    }

    public static void DisposeTempAllocations()
    {
        foreach (var allocation in tempAllocations)
        {
            Debug.Log("Disposing Temp Allocation");
            allocation.Dispose();
        }
        tempAllocations.Clear();
    }

    // Added the generic type parameter to the method
    public static void TrackNativeArray<T>(ref NativeArray<T> array) where T : unmanaged
    {
        RegisterTempAllocation(new DisposableNativeArray<T>(ref array));
    }

    // Fixed the class definition with proper generic type parameter
    private class DisposableNativeArray<T> : IDisposable where T : unmanaged
    {
        private NativeArray<T> _array;

        public DisposableNativeArray(ref NativeArray<T> array)
        {
            _array = array;
        }

        public void Dispose()
        {
            if (_array.IsCreated)
            {
                _array.Dispose();
            }
        }
    }

    // Fixed the NativeListPool class with proper generic type parameter
    public class NativeListPool<T> where T : unmanaged
    {
        private static Stack<NativeList<T>> pool = new Stack<NativeList<T>>();

        public static NativeList<T> Get()
        {
            if (pool.Count > 0)
                return pool.Pop();
            return new NativeList<T>(Allocator.Temp);
        }

        public static void Release(NativeList<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}