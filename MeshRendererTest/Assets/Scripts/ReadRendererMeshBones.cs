using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

public class ReadRendererMeshBones : MonoBehaviour
{
    // Start is called before the first frame update
    SkinnedMeshRenderer meshrenderer = null;
    void Start()
    {
        //配置text路径
        _textPath = Application.dataPath + "/mytext.txt";
        //获取SkinnedMeshRenderer组件
        for (int i = 0; i < transform.childCount; ++i)
        {
            print(transform.GetChild(i).gameObject.GetType());
            if (transform.GetChild(i).gameObject.name == "body_body")
            {
                meshrenderer = transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>();
                break;
            }
        }        
    }
    public void OutData()
    {
        ClearText();
        //
        Animator animator = transform.GetComponent<Animator>();
        if (animator != null) { 
            animator.SetFloat("aniSpeed", 0);
            //读取SkinedMesh的所有SkinedBones的transform
            foreach (Transform tr in meshrenderer.bones)
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
            print("textPath "+_textPath);
            //print("skinned bones num "+ meshrenderer.bones.Length+" bindposenum "+mesh.bindposes.Length);
        }
        else { print("animator is null"); }
    }
    string _textPath;
    StreamWriter writer;
    StreamReader reader;
    public void WriteIntoText(string text)
    {
        FileInfo file = new FileInfo(_textPath);
        if (file.Exists){writer = file.AppendText();}
        else writer = file.CreateText();
        writer.WriteLine(text);
        writer.Flush();
        writer.Close();
    }
    private void ClearText()
    {
        FileInfo fileInfo = new FileInfo(_textPath);
        if (fileInfo.Exists) { 
            fileInfo.Delete();
            fileInfo.Refresh();
        }
    }


    // Update is called once per frame
    void Update()
    {
    }
}
