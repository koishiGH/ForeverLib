using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextSetter : MonoBehaviour
{
    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        text.text = "Unmodded Script Text :shocked:\n\nPress Shift or Space 4 Modded Text\n(if the mod loaded properly)";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
