using UnityEngine;

public class Manager<T> : MonoBehaviour where T : Manager<T>
{ 
    protected static T instance;

    public static T Get()
    {
        return instance;
    }

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        instance = this as T;    
    }
}