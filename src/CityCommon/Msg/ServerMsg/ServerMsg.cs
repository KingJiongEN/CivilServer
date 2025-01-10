using System;

[Serializable]
public struct ServerMsg
{
    public string from;
    public string msg;
    public string channel;

    public ServerMsg(string from, string msg, string channel)
    {
        this.from = from;
        this.msg = msg;
        this.channel = channel;
    }

    public override string ToString()
    {
        return (MyStringBuilder.Create("[") + channel + "." + from + "]" + msg).ToStr();
    }
}