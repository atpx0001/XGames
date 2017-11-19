using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public delegate void PickPhotoDlgCallback(byte[] data);
/// <summary>
/// 选图对话框
/// </summary>
public class PickPhotoDlg {
    public enum Source {
        /// <summary>
        /// 相册
        /// </summary>
        Album,
        /// <summary>
        /// 相机
        /// </summary>
        Camera
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    public static void Show(PickPhotoDlgCallback callback, Source source, int imgWidth, int imgHeight) {
        GC.Collect();
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "All Files\0*.jpg;*.png\0\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        //ofn.initialDir = UnityEngine.Application.dataPath;//默认路径
        ofn.title = "选择图片";
        ofn.defExt = "jpg";//显示文件的类型
        //注意 一下项目不一定要全选 但是0x00000008项不要缺少
        //ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if(GetOpenFileName(ofn)) {
            //System.IO.File.ReadAllBytes(ofn.file);
            //WWW www = new WWW("file://" + WWW.EscapeURL(ofn.file.Replace("\\", "/"), System.Text.UTF8Encoding.Default));
            byte[] data = System.IO.File.ReadAllBytes(ofn.file); //www.bytes;
            //byte[] data = www.bytes;
            if(data != null) {
                callback(data);
            } else {
                callback(null);
            }
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    public static void Show(PickPhotoDlgCallback callback, Source source, int imgWidth, int imgHeight) {
        GC.Collect();
        Updater.Instance.GetType();
        AndroidJavaClass PhotoPicker = new AndroidJavaClass("com.go2play.PhotoPicker");
        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        picker = new PhotoPickerCallback(callback);
        if(source == Source.Album) {
            PhotoPicker.CallStatic("pickPhoto", activity, imgWidth, imgHeight, picker);
        } else {
            PhotoPicker.CallStatic("takePhoto", activity, imgWidth, imgHeight, picker);
        }
    }

    static PhotoPickerCallback picker;
#endif

#if UNITY_IOS && !UNITY_EDITOR
    public static void Show(PickPhotoDlgCallback callback, Source source, int imgWidth, int imgHeight) {
        GC.Collect();
        Updater.Instance.GetType();
        PickPhotoDlg.callback = callback;
        if(source == Source.Album) {
            g2p_pickPhoto(imgWidth, imgHeight, _IOS_PickPhotoCallback);
        } else {
            g2p_takePhoto(imgWidth, imgHeight, _IOS_PickPhotoCallback);
        }
    }

    static PickPhotoDlgCallback callback;
    delegate void IOS_PickPhotoCallback(IntPtr data, int len);
    [AOT.MonoPInvokeCallback(typeof(IOS_PickPhotoCallback))]
    static void _IOS_PickPhotoCallback(IntPtr ptr, int len) {
        byte[] data = null;
        if(len > 0) {
            data = new byte[len];
            Marshal.Copy(ptr, data, 0, len);
        }
        if(callback != null) {
            GameUtil.RunInMainThread(() => {
                callback(data);
                callback = null;
            });
        }
    }

    [DllImport("__Internal")]
    static extern int g2p_takePhoto(int w, int h, IOS_PickPhotoCallback callback);

    [DllImport("__Internal")]
    static extern int g2p_pickPhoto(int w, int h, IOS_PickPhotoCallback callback);
#endif
}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName {
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
public class PhotoPickerCallback: AndroidJavaProxy {
    public PickPhotoDlgCallback callback;
    public bool done = false;
    public byte[] data;
    public PhotoPickerCallback(PickPhotoDlgCallback callback)
        : base("com.go2play.IPhotoPickerCallback") {
        this.callback = callback;
    }
    public void onResult(AndroidJavaObject result) {
        if(result != null) {
            AndroidJavaObject obj = result.Get<AndroidJavaObject>("data");
            data = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj.GetRawObject());
        }
        if(callback != null) {
            GameUtil.RunInMainThread(() => {
                callback(data);
            });
        }
    }
}
#endif