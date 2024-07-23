using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable=ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    public int x=0;//awp silah degistirme icin kosul
    public int y=0;//m4 silah degistirme icin kosul
    public int z=0;//pistol silah degistirme icin kosul
    [SerializeField] Image healthbarImage;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject cameraHolder;
    [SerializeField] GameObject yourGameObject;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex=-1;


    public float fireRate = 0.25f; // Saniyede kaç atış yapılabilir

    private float lastFireTime = 0f; // Son ateş zamanı


    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    const float maxHealth=200f;
    float currentHealth=maxHealth;

    PlayerManager playerManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV=GetComponent<PhotonView>();

        playerManager=PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Fareyi kilitle
        Cursor.visible = false; // Fare imleci görünmez yap
        if(PV.IsMine)
        {
            EquipItem(0);   
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy (rb);
            Destroy(ui);
        }
    }

    void Update()
    {
        if(!PV.IsMine)
            return;
        Look();
        Move();
        Jump();

        for(int i = 0; i < items.Length; i++) 
        {
            if(Input.GetKeyDown((i*1+1).ToString()))
            {
                EquipItem(i);
                break;
            }    
        }

        if(Input.GetAxisRaw("Mouse ScrollWheel")>0f)
        {
            if(itemIndex>=items.Length-1)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex+1);
            }
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0f)
        {
            if(itemIndex<=0)
            {
                EquipItem(items.Length-1);
            }
            else
            {
                EquipItem(itemIndex-1);
            }

        }
        if (itemIndex ==2)
        {
            x=0;
            y=0;
            if (Input.GetMouseButtonDown(0) && Time.time - lastFireTime >= fireRate)
            {
                items[itemIndex].Use();
                lastFireTime = Time.time;
            }
        }
        else if(itemIndex == 1)
        {
            z=0;
            y=0;
            if (Input.GetMouseButtonDown(0) && Time.time - lastFireTime >= fireRate*6)
            {
                items[itemIndex].Use();
                lastFireTime = Time.time;
            }
        }
        else
        {
            z=0;
            x=0;
            if (Input.GetMouseButton(0) && Time.time - lastFireTime >= fireRate)
            {
                items[itemIndex].Use();
                lastFireTime = Time.time;
            }
        }
        
        if(transform.position.y<-10f)
        {
            Die();
        }
    }

    void Look()
    {
        float sensitivity = mouseSensitivity; // Başlangıçta mouseSensitivity değeri kullanılır.

        // Belirli bir GameObject aktif olduğunda mouseSensitivity değerini 2 yap.
        if (yourGameObject.activeSelf)
        {
            sensitivity = 0.2f;
        }

        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * sensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * sensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }


    void Move()
    {
        Vector3 moveDir= new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;

        moveAmount=Vector3.SmoothDamp(moveAmount, moveDir*(Input.GetKey(KeyCode.LeftShift)?sprintSpeed:walkSpeed),ref smoothMoveVelocity,smoothTime);
    }

    void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space)&&grounded)
        {
            rb.AddForce(transform.up*jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if(_index==previousItemIndex)
        {
            return;
        }
        itemIndex=_index;
        items[itemIndex].itemGameObject.SetActive(true);

        if(previousItemIndex!=-1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex=itemIndex;

        if(PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex",itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(changedProps.ContainsKey("itemIndex") &&!PV.IsMine&&targetPlayer==PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded=_grounded;
    }

    void FixedUpdate()
    {
        if(!PV.IsMine)
            return;
        rb.MovePosition(rb.position+transform.TransformDirection(moveAmount)*Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(RPC_TakeDamage),PV.Owner,damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        
        currentHealth-=damage;

        healthbarImage.fillAmount=currentHealth/maxHealth;
        if(currentHealth<=0)
        {
            Die();
            PlayerManager.Find(info.Sender).GetKill();
        }
        
    }

    void Die()
    {
        playerManager.Die();
    }
}
