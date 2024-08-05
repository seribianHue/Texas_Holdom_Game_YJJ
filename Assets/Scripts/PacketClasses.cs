#region Server -> Client
using System;

public class P_ACK_JoinPlayer
{
    public byte id = Convert.ToByte((int)PacketID.ACK_JOIN_PLAYER);
    public byte index;
    public string nickName;
    public string UUID;
    public P_ACK_JoinPlayer(byte index, string nickName, string UUID)
    {
        this.index = index;
        this.nickName = nickName;
        this.UUID = UUID;
    }
}

public class P_ACK_QuitSomebody
{
    public byte id = Convert.ToByte((int)PacketID.ACK_QUIT_SOMEBODY);
    public byte index;
    public P_ACK_QuitSomebody(byte index)
    {
        this.index = index;
    }
}

public class P_ACK_GameStart
{
    public byte id = Convert.ToByte((int)PacketID.ACK_GAME_START);
    public P_ACK_GameStart()
    {
    }
}

public class P_ACK_PersonalCard
{
    public byte id = Convert.ToByte((int)PacketID.ACK_PERSONAL_CARD);
    public byte shape;
    public byte number;
    public P_ACK_PersonalCard(byte shape, byte number)
    {
        this.shape = shape;
        this.number = number;
    }
}

public class P_ACK_TableCard
{
    public byte id = Convert.ToByte((int)PacketID.ACK_TABLE_CARD);
    public byte shape;
    public byte number;
    public P_ACK_TableCard(byte shape, byte number)
    {
        this.shape = shape;
        this.number = number;
    }
}

public class P_ACK_AnotherCard
{
    public byte id = Convert.ToByte((int)PacketID.ACK_ANOTHER_CARD);
    public byte index;
    public byte shape;
    public byte number;
    public P_ACK_AnotherCard(byte index, byte shape, byte number)
    {
        this.index = index;
        this.shape = shape;
        this.number = number;
    }
}

public class P_ACK_PlayerState
{
    public byte id = (byte)PacketID.ACK_PLAYER_STATE_INFO;
    public byte index;
    public byte state;
    public int price;
    public P_ACK_PlayerState(byte index, byte state, int price)
    {
        this.index = index;
        this.state = state;
        this.price = price;
    }
}

public class P_ACK_Winner
{
    public byte id = Convert.ToByte((int)PacketID.ACK_WINNER_INFO);
    public byte index;
    public P_ACK_Winner(byte index)
    {
        this.index = index;
    }
}

#endregion

#region Clinet -> Server
public class P_REQ_JoinGame
{
    public byte id = Convert.ToByte((int)PacketID.REQ_JOINGAME);
    public string nickName;
    public string UUID;
    public P_REQ_JoinGame(string nickName, string UUID)
    {
        this.nickName = nickName;
        this.UUID = UUID;
    }
}
public class P_REQ_QuitGame
{
    public byte id = Convert.ToByte((int)PacketID.REQ_QUIT);
    public byte index;
    public P_REQ_QuitGame(byte index)
    {
        this.index = index;
    }
}
public class P_REQ_ChangeState
{
    public byte id = Convert.ToByte((int)PacketID.REQ_CHANGESTATE);

    public byte player_index;
    public byte state;
    public int price;
    public P_REQ_ChangeState(byte pIndex, byte state, int price)
    {
        this.player_index = pIndex;
        this.state = state;
        this.price = price;
    }
}
#endregion
