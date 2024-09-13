using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataBlockScript1 : MonoBehaviour
{
    public float transRate_Base = 0.1f;
    GameObject sender;
    GameObject receiver;

    ClientScript1 clientScript1;
    NetNodeScript1 netNodeScript1;

    List<GameObject> pathnetnodes = new List<GameObject>(); //���ѡ��;���ļ�������ڵ�(ģ����ʵ�����ݴ������)
    int segmentsPath = 0;
    int currentSegment = 0;
    // Start is called before the first frame update
    void Start()
    {
        clientScript1 = GameObject.FindAnyObjectByType<ClientScript1>();

        sender = GameObject.FindGameObjectWithTag("Send");
        transform.localPosition = sender.transform.localPosition;
        receiver = GameObject.FindGameObjectWithTag("Receive");

        transRate_Base = UnityEngine.Random.Range(0.05f, 0.15f);

        List<GameObject> allnetnodes_t = new List<GameObject>();
        int CurrentNetNodes_count = clientScript1.CurrentNetNodes.Count;
        List<int> node_indexs = new List<int>();
        for (int i = 0; i < CurrentNetNodes_count; i++) node_indexs.Add(i);
        int pathnodes_num = UnityEngine.Random.Range(0, CurrentNetNodes_count / 3); //;������ڵ��������ȫ����ڵ�����֮һ���µ��������
        for (int i = 0;i < pathnodes_num; i++)
        {
            int outIndex = UnityEngine.Random.Range(0, node_indexs.Count);
            pathnetnodes.Add(clientScript1.CurrentNetNodes[node_indexs[outIndex]]);
            node_indexs.RemoveAt(outIndex);
        }
        pathnetnodes.Insert(0, clientScript1.theNearest__sender);
        pathnetnodes.Add(clientScript1.theNearest__receiver);
        pathnetnodes.Insert(0, sender);
        pathnetnodes.Add(receiver);
        netNodeScript1 = pathnetnodes[0].GetComponent<NetNodeScript1>();
        segmentsPath = pathnetnodes.Count - 1;
        //ÿ�����ݰ����ֵ�ʱ����Ѿ�ȷ���˴���·��(����ڵ���)
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
            Debug.LogWarning(name+"̫��ʱ��û�͵� �Ѿ�����!");
            Destroy(gameObject);//̫��ʱ��û�͵���ֱ��ɱ��
        }
        
        if (currentSegment < segmentsPath){
            //--------------- >>> Ȼ��ʼ���Žڵ�����ʼ�ƶ�(����)
            transform.position += 0.05f * netNodeScript1.netspeed * moveDir;
            if ((pathnetnodes[currentSegment + 1].transform.position - transform.position).magnitude < 0.01f) ChangeSegment();
        }
    }

    void ChangeSegment()
    {
        netNodeScript1 = pathnetnodes[currentSegment].GetComponent<NetNodeScript1>();
        currentSegment += 1;
        //transRate_Base = netNodeScript1.netspeed;    //ÿ����·�ϵĴ��������ǲ�һ����
        if (currentSegment < segmentsPath)moveDir = (pathnetnodes[currentSegment + 1].transform.position - pathnetnodes[currentSegment].transform.position).normalized;
        else{
            Debug.Log(name + "������!");
            Destroy(gameObject);
        }
    }
}
