using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[RequireComponent(typeof(MeshRenderer))]
public class CurveTextureCreater : MonoBehaviour {

    public AnimationCurve Maskcurve; //遮罩曲线图
    public AnimationCurve Offsetcurve;//位移曲线图

    //Texture2D curve2dTex;
    Material mat;

    private string filespath = "";
    private string folderpath = "";

    [ContextMenu("PreviewCurveMap")]
    public void PreviewCurveMap()
    {
        mat = this.GetComponent<MeshRenderer>().sharedMaterial;
        mat.SetTexture("_SpecularCurveMap", CreateCurveTex());
    }

    [ContextMenu("Create Curve Texture")]
    public void SetCurveFunc()
    {
        folderpath = Application.dataPath + "/CreateCurve";
        filespath = folderpath + "/" + gameObject.name + "_curve2d.png";
        SavePicture(CreateCurveTex());
        AssetDatabase.Refresh();
    }

    Texture2D CreateCurveTex()
    {
        Texture2D curve2dTex = new Texture2D(512, 2, TextureFormat.ARGB32, false);

        for (int i = 0; i < 512; i++)
        {
            float r = 0;
            float g = 0;
            if(Maskcurve != null)
            {
                r = Maskcurve.Evaluate((float)i / 512);
            }
            if(Offsetcurve != null)
            {
                g = Offsetcurve.Evaluate((float)i / 512);
            }
            Color _c = new Color(r, g, 0);
            for (int j = 0; j < 2; j++)
            {
                curve2dTex.SetPixel(i, j, _c);
            }
        }

        curve2dTex.Apply();
        return curve2dTex;
    }

    void SavePicture(Texture2D tex)
    {
        if (!Directory.Exists(folderpath))//判断一下该文件夹是否存在，若不存在先创建  
        {
            Directory.CreateDirectory(folderpath);//创建目录  
        }
        byte[] bys = tex.EncodeToPNG();//转换图片资源  
        File.WriteAllBytes(filespath, bys);//保存图片到写好的目录下  
    }
}
