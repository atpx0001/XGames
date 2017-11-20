using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GD_Role: BaseData {
    public int playerId;
    public int exp;
    public int lv;
    public Dictionary<int, GD_Item> items;
    public HashSet<int> composeList;
}
