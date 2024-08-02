public enum HAND
{
    ROYAL_FLUSH, STRAIGHT_FLUSH, FOUR_KIND, FUll_HOUSE,
    FLUSH, STRAIGHT, THREE_KIND, TWO_PAIR, PAIR, HIGH_CARD
}

public enum Rounds
{
    SETTING, FIRST, SECOND, THIRD, FINALL
}

public enum ClientToServer
{
    REQ_JOINGAME = 0,   //byte, byte[] (Encoding string to byte array)
    REQ_QUIT,           //byte, byte(my index)
    REQ_CHANGESTATE     //byte, byte(my index), byte(my state), int(byte[4] raise / bitconverter)
}

public enum ServerToClient
{
    ACK_JOIN_PLAYER = 0,      //byte, byte(index), byte[] (Encoding string to byte array)
    ACK_QUIT_SOMEBODY,      //byte, byte(index)
    ACK_GAME_START,         //byte
    ACK_PERSONAL_CARD,      //byte, byte(shape), byte(number)
    ACK_TABLE_CARD,         //byte, byte(shape), byte(number)
    ACK_ANOTHER_CARD,       //byte, byte(index), byte(shape), byte(number)
    ACK_PLAYER_STATE_INFO,  //byte, byte(index)
    ACK_WINNER_INFO        //byte, byte(index)
};