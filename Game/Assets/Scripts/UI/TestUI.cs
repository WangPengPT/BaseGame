using UnityEngine;

public class TestUI : UIBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        testValue = 123;
        testString = "Hello, World!";

        UI.UITools.Update(gameObject, this);
    }

    public int testValue;
    public string testString;
}
