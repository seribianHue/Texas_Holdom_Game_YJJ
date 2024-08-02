#region Server -> Client
public class P_ACK_JoinPlayer
{
    public byte id;
    public byte index;
    public string nickName;
    public P_ACK_JoinPlayer(byte id, byte index, string nickName)
    {
        this.id = id;
        this.index = index;
        this.nickName = nickName;
    }
}

public class P_ACK_QuitSomebody
{
    public byte id;
    public byte index;
    public P_ACK_QuitSomebody(byte id, byte index)
    {
        this.id = id;
        this.index = index;
    }
}

public class P_ACK_GameStart
{
    public byte id;
    public P_ACK_GameStart(byte id)
    {
        this.id = id;
    }
}

public class P_ACK_PersonalCard
{
    public byte id;
    public byte shape;
    public byte number;
    public P_ACK_PersonalCard(byte id, byte shape, byte number)
    {
        this.id = id;
        this.shape = shape;
        this.number = number;
    }
}

public class P_ACK_TableCard
{
    public byte id;
    public byte shape;
    public byte number;
    public P_ACK_TableCard(byte id, byte shape, byte number)
    {
        this.id = id;
        this.shape = shape;
        this.number = number;
    }
}

public class P_ACK_AnotherCard
{
    public byte id;
    public byte index;
    public byte shape;
    public byte number;
    public P_ACK_AnotherCard(byte id, byte index, byte shape, byte number)
    {
        this.id = id;
        this.index = index;
        this.shape = shape;
        this.number = number;
    }
}

public class P_ACK_PlayerState
{
    public byte id;
    public byte index;
    public byte state;
    public int price;
    public P_ACK_PlayerState(byte id, byte index, byte state, int price)
    {
        this.id = id;
        this.index = index;
        this.state = state;
        this.price = price;
    }
}

public class P_ACK_Winner
{
    public byte id;
    public byte index;
    public P_ACK_Winner(byte id, byte index)
    {
        this.id = id;
        this.index = index;
    }
}

#endregion

#region Clinet -> Server
public class P_REQ_JoinGame
{
    public byte id;
    public string nickName;
    public P_REQ_JoinGame(byte id, string nickName)
    {
        this.id = id;
        this.nickName = nickName;
    }
}
public class P_REQ_QuitGame
{
    public byte id;
    public byte index;
    public P_REQ_QuitGame(byte id, byte index)
    {
        this.id = id;
        this.index = index;
    }
}
public class P_REQ_ChangeState
{
    public byte id;

    public byte player_index;
    public byte state;
    public int price;
    public P_REQ_ChangeState(byte id, byte pIndex, byte state, int price)
    {
        this.id = id;
        this.player_index = pIndex;
        this.state = state;
        this.price = price;
    }
}
#endregion
