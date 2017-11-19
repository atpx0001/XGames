using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 供全局使用的更新调用器代理，会在所有其他MonoBehaviour前执行
/// </summary>
public class PreUpdater: MonoBehaviour {
    public Updater updater;

    void OnDestroy() {
        updater.PreDestroy();
    }

    void Update() {
        updater.PreUpdate();
    }

    void FixedUpdate() {
        updater.PreFixedUpdate();
    }
}
