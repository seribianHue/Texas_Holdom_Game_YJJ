using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get { return instance; } 
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] GameObject LobbyUI;
    public void SetLobbyUI(bool onoff)
    {
        LobbyUI.SetActive(onoff);
    }

    [SerializeField] GameObject StartGameBTN;
    public void SetStartGameBTN(bool onoff) { StartGameBTN.SetActive(onoff); }

    [SerializeField] GameObject SendMyInfoBTN;
    public void SetSendMyInfoBTN(bool onoff) {  SendMyInfoBTN.SetActive(onoff); }

    //Fold Check Button
    [SerializeField] Button FoldBTN;
    public void SetFoldBTNInteractable(bool onoff) { FoldBTN.interactable = onoff; }
    [SerializeField] Button CheckBTN;
    public void SetCheckBTNInteractable(bool onoff) { CheckBTN.interactable = onoff; }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
