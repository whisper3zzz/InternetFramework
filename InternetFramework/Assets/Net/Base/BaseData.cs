using System;
using System.Text;

public abstract class BaseData
{
    #region 序列化

    /// <summary>
    /// 获取字节数组容器的大小
    /// </summary>
    /// <returns></returns>
    public abstract int GetBytesCount();

    /// <summary>
    /// 序列化为字节数组
    /// </summary>
    /// <returns></returns>
    public abstract byte[] Serialize2Bytes();

    /// <summary>
    /// 泛型方法，将常用类型转换为字节数组，支持int、short、long、float、bool
    /// </summary>
    /// <typeparam name="T">具体的值类型</typeparam>
    /// <param name="value">具体的值</param>
    /// <param name="bytes">指定的字节数组</param>
    /// <param name="offset">记录当前索引位置</param>
    protected void SerializeValue2Bytes<T>(T value, byte[] bytes, ref int offset) where T : struct
    {
        byte[] valueBytes;

        if (typeof(T) == typeof(int))
        {
            valueBytes = BitConverter.GetBytes((int)(object)value);
        }
        else if (typeof(T) == typeof(short))
        {
            valueBytes = BitConverter.GetBytes((short)(object)value);
        }
        else if (typeof(T) == typeof(float))
        {
            valueBytes = BitConverter.GetBytes((float)(object)value);
        }
        else if (typeof(T) == typeof(long))
        {
            valueBytes = BitConverter.GetBytes((long)(object)value);
        }
        else if (typeof(T) == typeof(bool))
        {
            valueBytes = BitConverter.GetBytes((bool)(object)value);
        }
        else
        {
            throw new ArgumentException("Unsupported type");
        }

        valueBytes.CopyTo(bytes, offset);
        offset += valueBytes.Length;
    }

    /// <summary>
    ///     字符串转字节数组
    /// </summary>
    /// <param name="value">具体的字符串</param>
    /// <param name="bytes">指定的字节数组</param>
    /// <param name="offset">记录当前索引位置</param>
    protected void SerializeString2Bytes(string value, byte[] bytes, ref int offset)
    {
        // 先写入字符串的长度
        var strBytes = Encoding.UTF8.GetBytes(value);
        SerializeValue2Bytes(strBytes.Length, bytes, ref offset);
        // 再写入字符串的内容
        strBytes.CopyTo(bytes, offset);
        offset += strBytes.Length;
    }

    /// <summary>
    /// 自定义类型BaseData转字节数组
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="bytes">指定的字节数组</param>
    /// <param name="offset">记录当前索引位置</param>
    protected void SerializeData2Bytes(BaseData value, byte[] bytes, ref int offset)
    {
        var dataBytes = value.Serialize2Bytes();
        dataBytes.CopyTo(bytes, offset);
        offset += value.GetBytesCount();
    }

    #endregion

    #region 反序列化

    /// <summary>
    /// 从字节数组中反序列化
    /// </summary>       
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">开始的索引位置，默认为0</param>
    public abstract int DeserializeFromBytes(byte[] bytes, int offset = 0);

    /// <summary>
    /// 从字节数组中反序列化基础类型，支持int、short、long、float、bool
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    protected T DeserializeValueFromBytes<T>(byte[] bytes, ref int offset) where T : struct
    {
        T value;
        int size;
        if (typeof(T) == typeof(int))
        {
            value = (T)(object)BitConverter.ToInt32(bytes, offset);
            size = sizeof(int);
        }
        else if (typeof(T) == typeof(short))
        {
            value = (T)(object)BitConverter.ToInt16(bytes, offset);
            size = sizeof(short);
        }
        else if (typeof(T) == typeof(float))
        {
            value = (T)(object)BitConverter.ToSingle(bytes, offset);
            size = sizeof(float);
        }
        else if (typeof(T) == typeof(long))
        {
            value = (T)(object)BitConverter.ToInt64(bytes, offset);
            size = sizeof(long);
        }
        else if (typeof(T) == typeof(bool))
        {
            value = (T)(object)BitConverter.ToBoolean(bytes, offset);
            size = sizeof(bool);
        }
        else
        {
            throw new ArgumentException("Unsupported type");
        }

        offset += size;
        return value;
    }

    /// <summary>
    /// 从字节数组中反序列化byte
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    protected byte DeserializeByteFromBytes(byte[] bytes, ref int offset)
    {
        byte value = bytes[offset];
        offset += sizeof(byte);
        return value;
    }

    /// <summary>
    /// 从字节数组中反序列化字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    protected string DeserializeStringFromBytes(byte[] bytes, ref int offset)
    {
        var strLength = DeserializeValueFromBytes<int>(bytes, ref offset);
        var strBytes = new byte[strLength];
        Array.Copy(bytes, offset, strBytes, 0, strLength);
        offset += strLength;
        return Encoding.UTF8.GetString(strBytes);
    }

    /// <summary>
    /// 从字节数组中反序列化自定义类型BaseData
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T DeserializeDataFromBytes<T>(byte[] bytes, ref int offset) where T : BaseData, new()
    {
        var data = new T();
        offset += data.DeserializeFromBytes(bytes, offset);
        return data;
    }

    #endregion
}