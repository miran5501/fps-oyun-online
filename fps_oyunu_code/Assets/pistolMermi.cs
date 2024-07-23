using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class pistolMermi : Gun
{
    public GameObject animasyonicin_active;
    private PlayerController playerController;

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject muzzleFlashPrefab; // Namlu flaşı efekti prefabı
    private PhotonView PV;
    [Header("Ammo")]
    private int mag=48;
    private int ammo=12;
    private int magAmmo=12;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    [Header("animation")]
    public Animation animation;
    public AnimationClip reload;
    public AnimationClip geri_tepme;
    public AnimationClip pistol_alis;
    //public AnimationClip normal_pozisyon;

    public AudioSource sfx;
    public AudioClip pistol_ates;
    public AudioClip pistol_hazirlama;
    public AudioClip pistol_reload;


    void Start()
    {
        ammoText.text=ammo+"/"+mag;
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerController = GetComponentInParent<PlayerController>();
    }
    void Update()
    {
        if (playerController.z==0&& animasyonicin_active.activeSelf == true)
        {
            animation.Play(pistol_alis.name);
            sfx.clip=pistol_hazirlama;
            sfx.Play();
            playerController.z=1;
        }
        if(Input.GetKeyDown(KeyCode.R)&&animation.isPlaying==false)
        {
            Reload();
        }
    }

    public override void Use()
    {
        
        ShowMuzzleFlash();//namlu flash efekti
        
        
        if(ammo!=0&&animation.isPlaying==false)
        {
            Shoot();// Ates islemini gerceklestir.
            ApplyRecoil();// Silah geri tepmesini uygula    
        }
        else if(animation.isPlaying==false)
        {
            Reload();
        }
    }

    private void Shoot()
    {
        ammo--;//mermi azalt
        sfx.clip=pistol_ates;
        sfx.Play();

        // Silah geri tepmesi ile ilgili kodları burada kullanabilirsiniz.
        // Örneğin, her atışta rastgele bir geri tepme miktarı belirleyebilirsiniz.
        //float recoilAmount = Random.Range(0f, maxRecoil);
        //currentRecoil += recoilAmount;
        //currentRecoil = Mathf.Clamp(currentRecoil, 0f, maxRecoil);
        ammoText.text=ammo+"/"+mag;
        

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
        if(ammo!=12)
        {
            if(mag==0)
            {
                return;
            }
            animation.Play(reload.name);
            sfx.clip=pistol_reload;
            sfx.Play();
            if(mag>0)
            {
                mag=mag-(magAmmo-ammo);
                ammo=magAmmo;
            }
            ammoText.text=ammo+"/"+mag;
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
