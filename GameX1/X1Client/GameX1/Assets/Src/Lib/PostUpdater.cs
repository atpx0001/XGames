using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 供全局使用的更新调用器代理，会在所有其他MonoBehaviour之后执行
/// </summary>
public class PostUpdater: MonoBehaviour {
    public Updater updater;

    void OnDestroy() {
        updater.PostDestroy();
    }

    void Update() {
        updater.PostUpdate();
    }

    void FixedUpdate() {
        updater.PostFixedUpdate();
    }
}

