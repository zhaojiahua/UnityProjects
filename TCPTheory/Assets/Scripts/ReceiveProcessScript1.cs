using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveProcessScript1 : MonoBehaviour
{
    // Start is called before the first frame update
    ClientScript1 clientScript1;
    public GameObject dataBlockPrefab;
    public GameObject sDataBlockPrefab;
    void Start()
    {
        clientScript1 = GameObject.Find("GameStartObject").GetComponent<ClientScript1>();
    }

    // Update is called once per frame
    void Update()
    {    }
    public void ProcessReceivedData(GameObject indb){
        DataBlockScript1 tdbScript1 = indb.GetComponent<DataBlockScript1>();
        if (tdbScript1.isACK){
            GameObject sindb = clientScript1.allSDataBlocks[(int)(tdbScript1.ACK - 1)];
            sindb.GetComponent<SDataBlockScript1>().received=true;//根据收到的ACK确认相应的数据块
            sindb.GetComponent<MeshRenderer>().material.color = Color.gray;
            clientScript1.MoveSenderSDataBlocks();
        }
        else{//如果不是ACK,说明是正常的报文,收到后立即发送ACK给对方确认
            //首先判断这个内容是否已经存在(如果存在直接丢掉)
            UInt32 metedata=indb.GetComponent<DataBlockScript1>().metedata;
            if(!clientScript1.allReceivedSDataBlocks.Contains(metedata)){
                clientScript1.allReceivedSDataBlocks.Add(metedata);
                clientScript1.allReceivedSDataBlocks.Sort();
                GameObject tsdb = Instantiate(sDataBlockPrefab,clientScript1.sDataBlocks_Receiver_Grp.transform);
                tsdb.name=indb.name.Replace("_send","_recieved");
                tsdb.transform.position += new Vector3(metedata*0.05f,0,0);
                clientScript1.MoveReceiverSDataBlocks();
            }
            //无论内容是否已经存在都要发送相应的ACK给对方
            GameObject tdb = Instantiate(dataBlockPrefab);
            tdb.GetComponent<DataBlockScript1>().sender = GameObject.Find("Client2");//指定发送方是谁
            tdb.GetComponent<DataBlockScript1>().receiver = GameObject.Find("Client1");//指定接收方是谁
            tdb.GetComponent<DataBlockScript1>().theNearest__sender = clientScript1.theNearest__receiver;//指定发送方网关
            tdb.GetComponent<DataBlockScript1>().theNearest__receiver = clientScript1.theNearest__sender;//指定接收方网关
            tdb.name = indb.name + "_ACK";
            tdb.transform.position = tdb.GetComponent<DataBlockScript1>().sender.transform.position;
            tdb.GetComponent<DataBlockScript1>().isACK = true;
            tdb.GetComponent<DataBlockScript1>().ACK = metedata + 1;//ACK的内容就是收到的数据块的下一个数据块编号
            tdb.GetComponent<MeshRenderer>().material.color=Color.gray;//ACK数据包的颜色都是灰色
        }
    }
}
