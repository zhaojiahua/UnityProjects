using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDataBlockScript1 : MonoBehaviour
{
    [System.NonSerialized]
    public bool sended = false;
    [System.NonSerialized]
    public bool received = false;
    [System.NonSerialized]
    public UInt32 index=0;
    [System.NonSerialized]
    GameObject sender;
    [System.NonSerialized]
    GameObject receiver;
    [System.NonSerialized]
    public UInt32 repetition = 0;   //未收到重复发送
    // Start is called before the first frame update
    void Start()
    {
    }
    float c_time=0.0f;
    // Update is called once per frame
    void Update()
    {
        if(sended) c_time+=Time.deltaTime; //一旦发送出去就开始计时
        if(c_time>8 && !received) {
            repetition++;
            sended =false; //一旦计时超过8秒还没有收到回信就立马重传
            c_time=0.0f;
        }
    }
}
