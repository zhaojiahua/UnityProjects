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
    [System.NonSerialized]
    public GameObject sDataBlocks_Grp;
    [System.NonSerialized]
    public GameObject sDataBlocks_Receiver_Grp;
    List<GameObject> CurrentDataBlocks= new List<GameObject>(); //场景中存在的(正在传输的数据块)
    public List<GameObject> CurrentNetNodes= new List<GameObject>(); //场景中的网络节点(接收方和发送方的网关节点都没有算在里面)

    [System.NonSerialized]
    public GameObject theNearest__sender = null; //离sender最近的一个网络节点(相当于sender的网关)
    [System.NonSerialized]
    public GameObject theNearest__receiver = null; //离receiver最近的一个网络节点(相当于receiver的网关)

    public AnimationCurve pdf_forRanddistance = null;   //概率分布函数
    [System.NonSerialized]
    public AnimationCurve cdf_forRanddistance = null;   //概率累积分布函数
    //注意输入的pdf曲线一定是0-1之间的
    AnimationCurve GetCdfFromPdf(AnimationCurve inpdfcrv){
        AnimationCurve cdfcrv = new AnimationCurve();
        //积分算法(分割求和,这里分割成100份)
        float[] pdfvalues = new float[100];
        float allare = 0.0f;
        for(int i = 0; i < 100; ++i){
            pdfvalues[i] = inpdfcrv.Evaluate(i * 0.01f);
            allare += pdfvalues[i];
        }
        //采样10个点就可以大致定型cdf曲线
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
    float RandFromCurve(AnimationCurve cdfcrv,float intimev){
        return cdfcrv.Evaluate(intimev);
    }
    // Start is called before the first frame update
    void Start()
    {
        cdf_forRanddistance = GetCdfFromPdf(pdf_forRanddistance);

        sender =GameObject.Find("Client1");
        receiver = GameObject.Find("Client2");

        sDataBlocks_Grp = Instantiate(EmptyPrefab);
        sDataBlocks_Grp.name = "sDataBlocks_Grp";
        sDataBlocks_Receiver_Grp = Instantiate(EmptyPrefab);
        sDataBlocks_Receiver_Grp.name = "sDataBlocks_Receiver_Grp";

        //Debug.Log("--->>> sender name:"+sender.name);
        //Debug.Log("--->>> receiver name:" + receiver.name);

        float theNearestdistance__sender = 1000000.0f;
        float theNearestdistance__receiver = 1000000.0f;

        //随机生成15个网络节点(路由器或交换机),并定位出离sender和reciever最近的节点
        if (netNodePrefab)
        {
            GameObject NetNodes_Grp = Instantiate(EmptyPrefab);
            NetNodes_Grp.name = "NetNodes_Grp";
            for (int i = 0; i < 15;i++)
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
            CurrentNetNodes.Remove(theNearest__receiver); //移除接收方的网关节点
            CurrentNetNodes.Remove(theNearest__sender); //移除发送方的网关节点
            Debug.DrawLine(theNearest__sender.transform.position, sender.transform.position, Color.red, 1000.0f);
            Debug.DrawLine(theNearest__receiver.transform.position, receiver.transform.position, Color.blue, 1000.0f);
        }
        else Debug.LogWarning("netNodePrefab is not set!");
        dpdComp = GameObject.Find("DropdownSizeB").GetComponent<Dropdown>();
        textinputsize = GameObject.Find("InputFieldSizeB").GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    //float c_time = 0.0f;
    List<GameObject> insliderDBs = new List<GameObject>();
    List<GameObject> inReceiversliderDBs = new List<GameObject>();
    void Update()
    {
        foreach(GameObject DB in insliderDBs){
            //时刻检测滑动窗口内的数据块有没有发送出去,如果有未发送的数据块立刻发送出去
            if (!DB.GetComponent<SDataBlockScript1>().sended) { 
                SendDataBlock(DB);
                DB.GetComponent<SDataBlockScript1>().sended=true;
            }
            //时刻检测滑动窗口内的数据块有没有确认收到的,如果有确认收到的做上标记,并判断是否滑动窗口

        }
    }
    void SendDataBlock(GameObject indb){
        GameObject tdb = Instantiate(dataBlockPrefab);
        tdb.GetComponent<DataBlockScript1>().sender = GameObject.Find("Client1");//指定发送方是谁
        tdb.GetComponent<DataBlockScript1>().receiver = GameObject.Find("Client2");//指定接收方是谁
        tdb.GetComponent<DataBlockScript1>().theNearest__sender = theNearest__sender;//指定发送方网关
        tdb.GetComponent<DataBlockScript1>().theNearest__receiver = theNearest__receiver;//指定接收方网关
        tdb.GetComponent<DataBlockScript1>().metedata = indb.GetComponent<SDataBlockScript1>().index;
        tdb.name = indb.name + "_send_repetition"+indb.GetComponent<SDataBlockScript1>().repetition;
        tdb.transform.position = sender.transform.position;
        Material tdbmat=tdb.GetComponent<MeshRenderer>().material;
        Color tcolor=tdbmat.color;
        tdbmat.color =new Color(tcolor.r+0.4f* indb.GetComponent<SDataBlockScript1>().repetition, tcolor.g, tcolor.b,1);
        indb.GetComponent<MeshRenderer>().material.color = Color.yellow;
    }

    Dropdown dpdComp;   //输入数据大小的单位
    //TextMeshPro textinputsize;  //输入数据块的大小
    TMP_InputField textinputsize;

    [System.NonSerialized]
    public List<GameObject> inslides = new List<GameObject>();
    [System.NonSerialized]
    public List<GameObject> allSDataBlocks = new List<GameObject>();
    [System.NonSerialized]
    public List<UInt32> allReceivedSDataBlocks = new List<UInt32>();//接收方已经接收到的所有的数据块序号
    UInt32 slideWinSize=0;
    public void GenerateAndSendClick(){
        if (textinputsize.text == "") Debug.LogWarning("请输入Data size!");
        else{
            UInt64 datasize = (UInt64)(Convert.ToDouble(textinputsize.text) * Math.Pow(1000, dpdComp.value));
            UInt32 fullpacknum = (UInt32)(datasize / 1000);
            UInt32 rem = 1;
            if (datasize % 1000 == 0) rem = 0;
            UInt32 packnum = fullpacknum + rem;
            slideWinSize = (packnum / 3);
            if (slideWinSize > 6) slideWinSize = 6;
            if (slideWinSize < 1) slideWinSize = 1;
            for (int i = 0; i < packnum; ++i){
                GameObject tdb = Instantiate(sDataBlockPrefab);
                tdb.GetComponent<SDataBlockScript1>().index = (UInt32)i;
                tdb.transform.position = new Vector3(0.05f * i, 0, 0);
                tdb.name = "sdatablock" + string.Format("{0:D4}", i);
                tdb.transform.SetParent(sDataBlocks_Grp.transform, true);
                allSDataBlocks.Add(tdb);
                if (i<slideWinSize) insliderDBs.Add(tdb);
            }
            sDataBlocks_Grp.transform.position += sender.transform.position;
            sDataBlocks_Receiver_Grp.transform.position += receiver.transform.position;
            //生成滑动窗口(滑动窗口的大小要小于总包数量的二分之一)
            GameObject slideWind = GenerateSlideWind(sender,slideWinSize);
            //同时在接收方也生成相同的滑动窗口
            GameObject slideWind_receiver = GenerateSlideWind(receiver,slideWinSize);
        }
    }
    GameObject GenerateSlideWind(GameObject parentgo,UInt32 winsize){
        GameObject slideWind = Instantiate(SlideWinPrefab, parentgo.transform);
        Transform[] childTransforms=slideWind.GetComponentsInChildren<Transform>();
        childTransforms[3].position += new Vector3(winsize * 0.05f, 0, 0);
        childTransforms[2].position = (childTransforms[3].position + childTransforms[1].position) * 0.5f;
        childTransforms[2].localScale = new Vector3((childTransforms[3].position.x - childTransforms[1].position.x) / 0.05f, 1, 1);
        slideWind.name = parentgo.name+"slideWind";
        return slideWind;
    }

    public void MoveSenderSDataBlocks(){
        UInt32 ti=0;
        while (ti<insliderDBs.Count){
            if(insliderDBs[(int)ti].GetComponent<SDataBlockScript1>().received){ti++;} 
            else break;
        }
        sDataBlocks_Grp.transform.position += new Vector3(-ti*0.05f,0,0);
        for(int i=0;i<ti;++i){
            UInt32 nex_index=insliderDBs[insliderDBs.Count-1].GetComponent<SDataBlockScript1>().index;
            if(nex_index<(allSDataBlocks.Count-1)) insliderDBs.Add(allSDataBlocks[(int)nex_index+1]);
            else Debug.Log("------->> 发送完毕 没有更多数据了!");
            insliderDBs.RemoveAt(0);
        }
    }
    UInt32 baseStartIndex=0;
    public void MoveReceiverSDataBlocks(){
        UInt32 oldbaseStartIndex=baseStartIndex;
        while (baseStartIndex<allReceivedSDataBlocks.Count){
            if(allReceivedSDataBlocks[(int)(baseStartIndex)]==baseStartIndex){
                baseStartIndex++;
            }
            else break;
        }
        //Debug.Log("oldbaseStartIndex : "+oldbaseStartIndex);
        //Debug.Log("baseStartIndex : "+baseStartIndex);
        sDataBlocks_Receiver_Grp.transform.position += new Vector3(-(baseStartIndex-oldbaseStartIndex)*0.05f,0,0);
    }
}
