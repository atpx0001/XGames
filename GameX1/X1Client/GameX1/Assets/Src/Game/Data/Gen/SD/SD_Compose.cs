using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SD_Compose: BaseData {
    /// <summary>
    /// 配方id
    /// </summary>
    public int id;
    /// <summary>
    /// 合成品ID
    /// </summary>
    public int targetItemId;
    /// <summary>
    /// 材料ID列表
    /// </summary>
    public List<int> itemIdList;
    /// <summary>
    /// 材料数量列表
    /// </summary>
    public List<int> itemNumList;
}
