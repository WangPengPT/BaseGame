
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Transform uiroot;

    private void Awake()
    {
        Instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in uiroot)
        {
            Destroy(child.gameObject);
        }

        Load<TestUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public T Load<T>() where T : UIBase
    {
        var name = typeof(T).Name;

        var obj = Resources.Load<GameObject>("UI/" + name);
        
        var gameobject = Instantiate(obj, uiroot);
        gameobject.name = name;

        return gameobject.GetComponent<T>();
    }           


}
