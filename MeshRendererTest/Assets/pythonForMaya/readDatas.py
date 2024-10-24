fpath='E:/AAA_Working/L36zhiyuan/MeshRendererTest/Assets/mytext.txt'
with open(fpath,'r') as tf:
    lines=tf.readlines()
try:
    cmds.select('skinbones_Grp')
except ValueError:
    cmds.group(n='skinbones_Grp',em=1)
for line in lines:
    bonename_and_transf=line.split(' : (')
    transf=bonename_and_transf[1].split(') (')
    transf_t=[float(x)*100 for x in transf[0].split(',')]
    transf_r=[float(x) for x in transf[1].split(',')]
    transf_ss=transf[2].split(',')
    transf_s=[]
    transf_s.append(float(transf_ss[0]))
    transf_s.append(float(transf_ss[1]))
    transf_s.append(float(transf_ss[2].split(') ')[0]))
    bonename=bonename_and_transf[0].replace(' ','FBXASC032')
    cmds.parent(bonename,'skinbones_Grp')
    cmds.setAttr(bonename+'.rotateOrder',2)
    cmds.xform(bonename,t=transf_t,ro=transf_r,ws=1)
