using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMessage : BaseData
{
    public override int GetBytesCount()
    {
        throw new System.NotImplementedException();
    }

    public override byte[] Serialize2Bytes()
    {
        throw new System.NotImplementedException();
    }

    public override int DeserializeFromBytes(byte[] bytes, int offset = 0)
    {
        throw new System.NotImplementedException();
    }

    public virtual int GetMsgTypeFromId()
    {
        return 0;
    }
}