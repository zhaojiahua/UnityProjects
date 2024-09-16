using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataBlockScript1 : MonoBehaviour
{
    public float transRate_Base = 0.1f;
    [System.NonSerialized]
    public GameObject sender;
    [System.NonSerialized]
    public GameObject receiver;
    [System.NonSerialized]
    public GameObject theNearest__sender;
    [System.NonSerialized]
    public GameObject theNearest__receiver;

    ClientScript1 clientScript1;
    NetNodeScript1 netNodeScript1;

    List<GameObject> pathnetnodes = new List<GameObject>(); //随机选中途径的几个网络节点(模拟真实的数据传输情况)
    int segmentsPath = 0;
    int currentSegment = 0;
    // Start is called before the first frame update
    void Start()
    {
        clientScript1 = GameObject.FindAnyObjectByType<ClientScript1>();
        transform.localPosition = sender.transform.localPosition;

        transRate_Base = UnityEngine.Random.Range(0.05f, 0.12f);

        List<GameObject> allnetnodes_t = new List<GameObject>();
        int CurrentNetNodes_count = clientScript1.CurrentNetNodes.Count;
        List<int> node_indexs = new List<int>();
        for (int i = 0; i < CurrentNetNodes_count; i++) node_indexs.Add(i);
        int pathnodes_num = UnityEngine.Random.Range(0, CurrentNetNodes_count / 4); //途径网络节点的数量在全网络节点四分之一以下的随机数量
        for (int i = 0;i < pathnodes_num; i++)
        {
            int outIndex = UnityEngine.Random.Range(0, node_indexs.Count);
            pathnetnodes.Add(clientScript1.CurrentNetNodes[node_indexs[outIndex]]);
            node_indexs.RemoveAt(outIndex);
        }
        pathnetnodes.Insert(0, theNearest__sender);
        pathnetnodes.Add(theNearest__receiver);
        pathnetnodes.Insert(0, sender);
        pathnetnodes.Add(receiver);
        netNodeScript1 = pathnetnodes[0].GetComponent<NetNodeScript1>();
        segmentsPath = pathnetnodes.Count - 1;
        //每个数据包出现的时候就已经确定了传输路线(网络节点链)
        //Debug.Log("pathnetnodes count " + pathnetnodes.Count);
        //string ta = "they are: ";
        //foreach (GameObject item in pathnetnodes) ta += item.name + ",";
        //Debug.Log(ta + "\n------------------------------------------<<");
        moveDir = pathnetnodes[1].transform.position - pathnetnodes[0].transform.position;
    }
    Vector3 moveDir = Vector3.zero;
    float age = 0.0f;
    // Update is called once per frame
    void Update()
    {
        age += Time.deltaTime;
        if (age > 60.0f) {
            Debug.LogWarning(name+"太长时间没送到 已经丢包!");
            Destroy(gameObject);//太长时间没送到就直接杀掉
        }
        
        if (currentSegment < segmentsPath){
            //--------------- >>> 然后开始沿着节点链开始移动(传输)
            transform.position += 0.05f * netNodeScript1.netspeed * moveDir;
            if ((pathnetnodes[currentSegment + 1].transform.position - transform.position).magnitude < 0.01f) ChangeSegment();
        }
    }
    //关于报文内容
    [System.NonSerialized]
    public bool isACK = false;
    [System.NonSerialized]
    public UInt32 ACK = 0;
    [System.NonSerialized]
    public UInt32 metedata = 0;
    [System.NonSerialized]
    public UInt32 repetition = 0;   //未收到重复发送
    void ChangeSegment()
    {
        netNodeScript1 = pathnetnodes[currentSegment].GetComponent<NetNodeScript1>();
        currentSegment += 1;
        //transRate_Base = netNodeScript1.netspeed;    //每条链路上的传输速率是不一样的
        if (currentSegment < segmentsPath)moveDir = (pathnetnodes[currentSegment + 1].transform.position - pathnetnodes[currentSegment].transform.position).normalized;
        else{
            //Debug.Log(name + "传到了! metedata = "+ metedata);
            //通知接收方(调用接收方的处理函数)
            ReceiveProcessScript1 trps = pathnetnodes[currentSegment].GetComponent<ReceiveProcessScript1>();
            if (trps) trps.ProcessReceivedData(gameObject);
            else Debug.LogWarning("ReceiveProcessScript1 is not found!");
            Destroy(gameObject);
        }
    }
}
