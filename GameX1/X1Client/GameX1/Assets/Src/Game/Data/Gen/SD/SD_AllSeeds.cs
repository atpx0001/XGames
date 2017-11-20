using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SD_AllSeeds: BaseData {
    /// <summary>
    /// 物品种子字典
    /// </summary>
    public Dictionary<int, SD_Item> items;
    /// <summary>
    /// 配方种子字典
    /// </summary>
    public Dictionary<int, SD_Compose> composeList;
}
