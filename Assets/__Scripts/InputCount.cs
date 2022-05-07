using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputCount : MonoBehaviour
{
    public InputField IF;

    private void Start()
    {
        IF = GetComponent<InputField>();
    }

    void Update()
    {
        if (IF.text == "")
            return;
        if (int.Parse(IF.text) > 4)
            IF.text = "4";
        else if (int.Parse(IF.text) < 2)
            IF.text = "2";
    }
}
