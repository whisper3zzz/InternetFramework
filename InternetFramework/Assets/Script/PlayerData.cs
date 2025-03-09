using System.Text;

public class PlayerData : BaseData
{
    public int PlayerId;
    public string PlayerName;
    public int PlayerLevel;

    public override int GetBytesCount()
    {
        return 4 + 4 + 4 + Encoding.UTF8.GetBytes(PlayerName).Length;
    }

    public override byte[] Serialize2Bytes()
    {
        int index = 0;
        byte[] bytes = new byte[GetBytesCount()];

        SerializeString2Bytes(PlayerName, bytes, ref index);
        SerializeValue2Bytes(PlayerId, bytes, ref index);
        SerializeValue2Bytes(PlayerLevel, bytes, ref index);
        return bytes;
    }

    public override int DeserializeFromBytes(byte[] bytes, int offset = 0)
    {
        int index = offset;
        PlayerName = DeserializeStringFromBytes(bytes, ref index);
        PlayerId = DeserializeValueFromBytes<int>(bytes, ref index);
        PlayerLevel = DeserializeValueFromBytes<int>(bytes, ref index);
        return index - offset;
    }
}