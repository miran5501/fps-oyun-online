using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class awpmermi : Gun
{
    public GameObject animasyonicin_active;
    private PlayerController playerController;

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject muzzleFlashPrefab; // Namlu flaşı efekti prefabı
    private PhotonView PV;
    [Header("Ammo")]
    private int magawp=25;
    private int ammo=5;
    private int magAmmo=5;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    [Header("animation")]
    public Animation animation;
    public AnimationClip reload;
    public AnimationClip geri_tepme;
    public AnimationClip awp_alis;

    public AudioSource sfx;
    public AudioSource sfx2;
    public AudioClip awp_ates;
    public AudioClip awp_hazirlama;
    public AudioClip awp_reload;



    void Start()
    {
        ammoText.text=ammo+"/"+magawp;
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerController = GetComponentInParent<PlayerController>();
    }
    void Update()
    {
        if (playerController.x==0&& animasyonicin_active.activeSelf == true)
        {
            animation.Play(awp_alis.name);
            sfx.clip=awp_hazirlama;
            sfx.Play();
            playerController.x=1;
        }
        
        if(Input.GetKeyDown(KeyCode.R)&&animation.isPlaying==false)
        {
            Reload();
        }
    }

    public override void Use()
    {
        // Sadece bu fonksiyon çağrıldığında namlu flaşı efektini göster.
        ShowMuzzleFlash();
        

        if(ammo!=0&&animation.isPlaying==false)
        {
            Shoot();// Ates islemini gerceklestir.
            ApplyRecoil();// Silah geri tepmesini uygula    
        }
        else if(animation.isPlaying==false)
        {
            Reload();
        }
        

        // Silah geri tepmesini uygula
        
    }


    private void Shoot()
    {
        ammo--;
        sfx2.clip=awp_ates;
        sfx2.Play();
        // Silah geri tepmesi ile ilgili kodları burada kullanabilirsiniz.
        // Örneğin, her atışta rastgele bir geri tepme miktarı belirleyebilirsiniz.
        //float recoilAmount = Random.Range(0f, maxRecoil);
        //currentRecoil += recoilAmount;
        //currentRecoil = Mathf.Clamp(currentRecoil, 0f, maxRecoil);
        ammoText.text=ammo+"/"+magawp;
        sfx.clip=awp_hazirlama;
        sfx.Play();
        

        // Diğer ateş işlemi kodları...
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            IDamageable damageable = hit.collider.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(((GunInfo)itemInfo).damage);
            }

            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    void Reload()
    {
        if(ammo!=5)
        {
            if(magawp==0)
            {
                return;
            }
            animation.Play(reload.name);
            sfx.clip=awp_reload;
            sfx.Play();

            if(magawp>0)
            {
               magawp=magawp-(magAmmo-ammo);
               ammo=magAmmo;
            }
            ammoText.text=ammo+"/"+magawp;
        }
        else
        {
            return;
        }
        
    }

    private void ApplyRecoil()
    {
        animation.Play(geri_tepme.name);
    }

    [PunRPC]
    private void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 1f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    private void ShowMuzzleFlash()
    {
        if (muzzleFlashPrefab != null)
        {
            muzzleFlashPrefab.SetActive(true);

            // Belirli bir süre sonra namlu flaşı efektini devre dışı bırakın.
            StartCoroutine(HideMuzzleFlash());
        }
    }

    private IEnumerator HideMuzzleFlash()
    {
        yield return new WaitForSeconds(0.1f);

        if (muzzleFlashPrefab != null)
        {
            muzzleFlashPrefab.SetActive(false);
        }
    }
}
