# -*- coding: utf-8 -*-
"""
Spyder Editor

This is a temporary script file.
"""
import string
from os import listdir
from os.path import isfile, join
import numpy as np

# set it to path with files with text.
mypath="D:\\1\\all"

onlyfiles = [join(mypath, f) for f in listdir(mypath) if isfile(join(mypath, f))]

print("running...")

keys ={}
for c in string.printable:
    keys[c]=0

# neighbor detection

pkeys = string.ascii_lowercase + " .,;"
xlskeys = np.asarray([*pkeys],dtype=object)
xlskeys[-1]="semi"
xlskeys[-2]="coma"
xlskeys[-3]="dot"
xlskeys[-4]="spc"

forths = "arstneio"

pkidx = {}
i=0
for k in pkeys:
    pkidx[k] = i
    i+=1

adjmat = np.zeros((len(pkeys),len(pkeys)),dtype = float)

# Parsing

kc=ka=kb=""

def addWeight(a,b,c):
    ia=pkidx[a]
    ib=pkidx[b]
    ic=pkidx[c]
    #now we really need direction as typing down is far easier than typing up.
    #and for that we dont care of thirds. as well
    adjmat[ia,ic]+=0.1
    adjmat[ib,ic]+=1
    
    #but also i want to know where is one sequence often than reverse
    adjmat[ic,ia]-=0.1
    adjmat[ic,ib]-=1

for f in onlyfiles:
    with open(f, 'r',  encoding='utf-8' , errors='ignore') as file:
        data = file.read()
        for i, c in enumerate(data):
            if c == "\n" or c=="\t":
                kc=ka=kb=""
            
            if c.lower() in pkeys:
                kc=kb
                kb=ka
                ka = c.lower()
                if kc !="":
                    addWeight(ka,kb,kc)
                
            try:
                keys[c] += 1
            except:
                a=0
                
# Output


layout1= "qwfpbjluy;'"
layout2= "arstgmneio-"
layout3= "zxcd,khv./"
layouts= "()[]{}<>:;.,=-+*/"

l1 = [keys[k.lower()]+keys[k.upper()] for k in layout1]
l2 = [keys[k.lower()]+keys[k.upper()] for k in layout2]
l3 = [keys[k.lower()]+keys[k.upper()] for k in layout3]
ls = [keys[k] for k in layouts]

k1= [l+" " for l in layout1]
k2= [l+" " for l in layout2]
k3= [l+" " for l in layout3]
ks= [l+" " for l in layouts]

print("done parsing" + mypath)

# generate adjmat paste
# sort from less to most

npkeys= np.asarray([*pkeys])
npkeys[adjmat[1].argsort()]


outS = ""
for i in range(0,len(npkeys)):
    sortedIdx= adjmat[i].argsort()
    sortedLet = xlskeys[sortedIdx]
    sortedVal = adjmat[i][sortedIdx]
    
    outS += xlskeys[i]+":;"
    for l in sortedVal:
        outS += str(l) + ";";
    outS+= "\n"
    outS += xlskeys[i]+":;"
    for l in sortedLet:
        outS += l + ";";
    outS+= "\n"
    
outS+= "intersect min count with any forth \n"
    
for i in range(0,len(pkeys)):
    if npkeys[i] in forths:
        continue
    minrow = -1
    k = npkeys[i]
    for j in range(0,len(pkeys)):
        if npkeys[j] in forths:
            crow = abs(adjmat[i][j])
            if crow < minrow or minrow < 0:
                minrow=crow            
    outS += xlskeys[i]+":;"+ str(minrow) + ";"+ str(keys[k.lower()]+keys[k.upper()])+ "\n"
