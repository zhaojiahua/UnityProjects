using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetNodeScript1 : MonoBehaviour
{
    [System.NonSerialized]
    public float netspeed=0.15f;
    public AnimationCurve pdf_forNetSpeed = null;   //概率分布函数
    [System.NonSerialized]
    public AnimationCurve cdf_forNetSpeed = null;   //概率累积分布函数
    AnimationCurve GetCdfFromPdf(AnimationCurve inpdfcrv)
    {
        AnimationCurve cdfcrv = new AnimationCurve();
        //积分算法(分割求和,这里分割成100份)
        float[] pdfvalues = new float[100];
        float allare = 0.0f;
        for (int i = 0; i < 100; ++i)
        {
            pdfvalues[i] = inpdfcrv.Evaluate(i * 0.01f);
            allare += pdfvalues[i];
        }
        //采样10个点就可以大致定型cdf曲线
        for (int i = 0; i < 10; ++i)
        {
            float tvalue = 0.0f;
            for (int j = 0; j < i * 10; ++j)
            {
                tvalue += pdfvalues[j];
            }
            //cdfvalues[i]= tvalue;
            cdfcrv.AddKey(tvalue / allare, i * 0.1f);
        }
        cdfcrv.AddKey(1, 1);
        return cdfcrv;
    }
    float RandFromCurve(AnimationCurve cdfcrv, float intimev)
    {
        return cdfcrv.Evaluate(intimev);
    }
    // Start is called before the first frame update
    void Start()
    {
        cdf_forNetSpeed = GetCdfFromPdf(pdf_forNetSpeed);
        netspeed = 0.14f * RandFromCurve(cdf_forNetSpeed, UnityEngine.Random.Range(0.0f, 1.0f)) + 0.06f;
    }

    // Update is called once per frame
    void Update()
    {
        //网速时时刻刻都在变化
        netspeed = 0.2f * RandFromCurve(cdf_forNetSpeed, UnityEngine.Random.Range(0.0f, 1.0f)) + 0.04f;
    }
}
