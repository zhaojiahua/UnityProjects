using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ClientScript1 : MonoBehaviour
{
    public GameObject sDataBlockPrefab;
    public GameObject dataBlockPrefab;
    public GameObject netNodePrefab;
    public GameObject EmptyPrefab;
    public GameObject SlideWinPrefab;
    GameObject sender;
    GameObject receiver;
    List<GameObject> CurrentDataBlocks= new List<GameObject>(); //�����д��ڵ�(���ڴ�������ݿ�)
    public List<GameObject> CurrentNetNodes= new List<GameObject>(); //�����е�����ڵ�(���շ��ͷ��ͷ������ؽڵ㶼û����������)

    [System.NonSerialized]
    public GameObject theNearest__sender = null; //��sender�����һ������ڵ�(�൱��sender������)
    [System.NonSerialized]
    public GameObject theNearest__receiver = null; //��receiver�����һ������ڵ�(�൱��receiver������)

    public AnimationCurve pdf_forRanddistance = null;   //���ʷֲ�����
    [System.NonSerialized]
    public AnimationCurve cdf_forRanddistance = null;   //�����ۻ��ֲ�����
    //ע�������pdf����һ����0-1֮���
    AnimationCurve GetCdfFromPdf(AnimationCurve inpdfcrv){
        AnimationCurve cdfcrv = new AnimationCurve();
        //�����㷨(�ָ����,����ָ��100��)
        float[] pdfvalues = new float[100];
        float allare = 0.0f;
        for(int i = 0; i < 100; ++i){
            pdfvalues[i] = inpdfcrv.Evaluate(i * 0.01f);
            allare += pdfvalues[i];
        }
        //����10����Ϳ��Դ��¶���cdf����
        for(int i = 0; i < 10; ++i)
        {
            float tvalue = 0.0f;
            for(int j = 0; j < i*10; ++j)
            {
                tvalue += pdfvalues[j];
            }
            //cdfvalues[i]= tvalue;
            cdfcrv.AddKey(tvalue/ allare, i * 0.1f);
        }
        cdfcrv.AddKey(1, 1);
        return cdfcrv; 
    }
    float RandFromCurve(AnimationCurve cdfcrv,float intimev)
    {
        return cdfcrv.Evaluate(intimev);
    }
    // Start is called before the first frame update
    void Start()
    {
        cdf_forRanddistance = GetCdfFromPdf(pdf_forRanddistance);

        sender =GameObject.FindGameObjectWithTag("Send");
        receiver = GameObject.FindGameObjectWithTag("Receive");

        //Debug.Log("--->>> sender name:"+sender.name);
        //Debug.Log("--->>> receiver name:" + receiver.name);

        float theNearestdistance__sender = 1000000.0f;
        float theNearestdistance__receiver = 1000000.0f;

        //�������20������ڵ�(·�����򽻻���),����λ����sender��reciever����Ľڵ�
        if (netNodePrefab)
        {
            GameObject NetNodes_Grp = Instantiate(EmptyPrefab);
            NetNodes_Grp.name = "NetNodes_Grp";
            for (int i = 0; i < 20; i++)
            {
                Vector3 randdir = UnityEngine.Random.insideUnitSphere;
                randdir.y *= 0.1f;
                float randdis = 30.0f * RandFromCurve(cdf_forRanddistance, UnityEngine.Random.Range(0.0f, 1.0f));
                //float randdis = UnityEngine.Random.Range(1.0f,30.0f);
                Vector3 nodepos = randdis * randdir;
                GameObject t_netnodego = Instantiate(netNodePrefab, nodepos, Quaternion.identity);
                t_netnodego.name = "NetNode" + string.Format("{0:D2}", i);
                CurrentNetNodes.Add(t_netnodego);
                t_netnodego.transform.SetParent(NetNodes_Grp.transform, true);
                float nearsetsenderdistance = (nodepos - sender.transform.position).magnitude;
                float nearsetrecieverdistance = (nodepos - receiver.transform.position).magnitude;
                if (nearsetsenderdistance < theNearestdistance__sender)
                {
                    theNearestdistance__sender = nearsetsenderdistance;
                    theNearest__sender = t_netnodego;
                }
                if (nearsetrecieverdistance < theNearestdistance__receiver)
                {
                    theNearestdistance__receiver = nearsetrecieverdistance;
                    theNearest__receiver = t_netnodego;
                }
            }
            CurrentNetNodes.Remove(theNearest__receiver); //�Ƴ����շ������ؽڵ�
            CurrentNetNodes.Remove(theNearest__sender); //�Ƴ����ͷ������ؽڵ�
            Debug.DrawLine(theNearest__sender.transform.position, sender.transform.position, Color.red, 1000.0f);
            Debug.DrawLine(theNearest__receiver.transform.position, receiver.transform.position, Color.blue, 1000.0f);
        }
        else Debug.LogWarning("netNodePrefab is not set!");
        dpdComp = GameObject.Find("DropdownSizeB").GetComponent<Dropdown>();
        textinputsize = GameObject.Find("InputFieldSizeB").GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    float c_time = 0.0f;
    void Update()
    {
        c_time+= Time.deltaTime;
        if (c_time > 1.0f)
        {
            c_time = 0.0f;
            //if (dataBlockPrefab)
            //{
            //    GameObject t_datablock=Instantiate(dataBlockPrefab) as GameObject;
            //    CurrentDataBlocks.Add(t_datablock);
            //}
        }
    }

    Dropdown dpdComp;   //�������ݴ�С�ĵ�λ
    //TextMeshPro textinputsize;  //�������ݿ�Ĵ�С
    TMP_InputField textinputsize;
    public void DropBntChanged()
    {
        //Debug.Log("---- <<<------"+ dpdComp.value);
    }
    public void InputSizeEnd()
    {
        if (textinputsize) Debug.Log("---- data size ------ " + textinputsize.text);
        else Debug.Log("textinputsize empty!");
    }
    List<GameObject> inslides = new List<GameObject>();
    public void GenerateAndSendClick()
    {
        if (textinputsize.text == "") Debug.LogWarning("������Data size!");
        else{
            UInt64 datasize = (UInt64)(Convert.ToDouble(textinputsize.text) * Math.Pow(1000, dpdComp.value));
            UInt32 fullpacknum = (UInt32)(datasize / 1000);
            UInt32 rem = 1;
            if (datasize % 1000 == 0) rem = 0;
            UInt32 packnum = fullpacknum + rem;
            GameObject sDataBlocks_Grp = Instantiate(EmptyPrefab);
            sDataBlocks_Grp.name = "sDataBlocks_Grp";
            for (int i = 0; i < packnum; ++i){
                GameObject tdb = Instantiate(sDataBlockPrefab);
                tdb.transform.position = new Vector3(0.05f * i, 0, 0);
                tdb.name = "sdatablock" + string.Format("{0:D4}", i);
                tdb.transform.SetParent(sDataBlocks_Grp.transform, true);
            }
            sDataBlocks_Grp.transform.position += sender.transform.position;
            //���ɻ�������(�������ڵĴ�СҪС���ܰ������Ķ���֮һ)
            GameObject slideWind = Instantiate(SlideWinPrefab, sDataBlocks_Grp.transform);
            Transform[] childTransforms=slideWind.GetComponentsInChildren<Transform>();
            UInt32 slideWinSize = (packnum / 3);
            childTransforms[3].position += new Vector3(slideWinSize * 0.05f, 0, 0);  //�������ڵĳ���С���ܳ��ȵĶ���֮һ
            childTransforms[2].position = (childTransforms[3].position + childTransforms[1].position) * 0.5f;
            childTransforms[2].localScale = new Vector3((childTransforms[3].position.x - childTransforms[1].position.x) / 0.05f, 1, 1);
            slideWind.name = "SlideWindow";
        }
        

    }
}
