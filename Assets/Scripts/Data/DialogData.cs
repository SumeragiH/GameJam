using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogData", menuName = "Dialog Data", order = 1)]
public class DialogData : ScriptableObject
{
    public List<string> dialogDictionary;
}
