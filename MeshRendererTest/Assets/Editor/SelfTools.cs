using log4net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SelfTools : MonoBehaviour
{
    [MenuItem("GameObject/导出动作Pose", false, 4)]
    static public void ExportPose()
    {
        GameObject selectedobj = Selection.activeGameObject;
        animator = selectedobj.GetComponentInParent<Animator>();
        SkinnedMeshRenderer meshrenderer = selectedobj.GetComponent<SkinnedMeshRenderer>();
        var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshrenderer);
        print(UnityEditor.AssetDatabase.GetAssetPath(prefabAsset));

        _textPath = Application.dataPath + "/mytext.txt";
        OutData(meshrenderer);
    }
    static Animator animator;
    static void OutData(SkinnedMeshRenderer meshrenderer)
    {
        ClearText();
        //
        if (animator != null)
        {
            animator.SetFloat("aniSpeed", 0);
            //读取SkinedMesh的所有SkinedBones的transform
            foreach (UnityEngine.Transform tr in meshrenderer.bones)
            {
                String tempStr = "";
                tempStr += tr.name + " : ";
                Vector3 lpos = tr.position;
                Quaternion lq = tr.rotation;
                Quaternion rightQ;
                rightQ.x = lq.x;
                rightQ.y = -lq.y;
                rightQ.z = -lq.z;
                rightQ.w = lq.w;
                Vector3 lrot = rightQ.eulerAngles;
                Vector3 lsca = tr.localScale;
                tempStr += "(" + (double)-lpos.x + ",";
                tempStr += (double)lpos.y + ",";
                tempStr += (double)lpos.z + ") ";
                tempStr += "(" + (double)lrot.x + ",";
                tempStr += (double)lrot.y + ",";
                tempStr += (double)lrot.z + ") ";
                tempStr += "(" + (double)lsca.x + ",";
                tempStr += (double)lsca.y + ",";
                tempStr += (double)lsca.z + ") ";
                WriteIntoText(tempStr);
            }
            print("textPath " + _textPath);
            animator.SetFloat("aniSpeed", 1);
            //print("skinned bones num "+ meshrenderer.bones.Length+" bindposenum "+mesh.bindposes.Length);
        }
        else { print("animator is null"); }
    }
    static string _textPath;
    static StreamWriter writer;
    static StreamReader reader;
    static void WriteIntoText(string text)
    {
        FileInfo file = new FileInfo(_textPath);
        if (file.Exists) { writer = file.AppendText(); }
        else writer = file.CreateText();
        writer.WriteLine(text);
        writer.Flush();
        writer.Close();
    }
    static void ClearText()
    {
        FileInfo fileInfo = new FileInfo(_textPath);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
            fileInfo.Refresh();
        }
    }
    // Start is called before the first frame update
    void Start()
    {    }
    // Update is called once per frame
    void Update()
    {    }
}
