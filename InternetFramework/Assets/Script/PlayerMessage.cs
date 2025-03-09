public class PlayerMessage : BaseMessage
{
    public int PlayerId;
    public PlayerData PlayerData;

    public override byte[] Serialize2Bytes()
    {
        int index = 0;
        byte[] bytes = new byte[GetBytesCount()];
        SerializeValue2Bytes(GetMsgTypeFromId(), bytes, ref index);
        SerializeValue2Bytes(PlayerId, bytes, ref index);
        SerializeValue2Bytes(PlayerData, bytes, ref index);
        return bytes;
    }

    public override int DeserializeFromBytes(byte[] bytes, int offset = 0)
    {
        int index = offset;
        PlayerId = DeserializeValueFromBytes<int>(bytes, ref index);
        PlayerData = DeserializeValueFromBytes<PlayerData>(bytes, ref index);
        return index - offset;
    }

    public override int GetBytesCount()
    {
        return 4 + 4 + PlayerData.GetBytesCount();
    }

    public override int GetMsgTypeFromId()
    {
        return 1;
    }
}