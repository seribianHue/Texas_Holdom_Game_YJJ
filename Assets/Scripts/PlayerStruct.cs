using Unity.Networking.Transport;

public class Player
{
    public string nickname;
    string uuid;
    public string UUID { get => uuid; }
    public NetworkConnection connection;

    public Card card1;
    public Card card2;
    public HAND playerHand;
    public Card highestCard;
    public bool isFold = false;

    public Player(string nickname, string uuid)
    {
        this.nickname = nickname;
        this.uuid = uuid;
    }
}
