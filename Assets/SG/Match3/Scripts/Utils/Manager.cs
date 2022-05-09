using UnityEngine;
/// <summary>
/// Implementation of a monosingleton to avoid repeating the same pattern many times for different
/// scripts
/// </summary>
/// <typeparam name="T"></typeparam>
public class Manager<T> : MonoBehaviour where T : Manager<T>
{
    protected static T instance;

    public static T Get() {
        return instance;
    }

    protected virtual void Awake() {
        instance = this as T;
    }
}