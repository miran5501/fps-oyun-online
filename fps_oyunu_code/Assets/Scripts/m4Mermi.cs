using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class m4Mermi : Gun
{
    public GameObject animasyonicin_activem4;
    private PlayerController playerController;
    //private Recoil Recoil_Script;

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject muzzleFlashPrefab; // Namlu flaşı efekti prefabı
    private PhotonView PV;
    [Header("Ammo")]
    private int mag=120;
    private int ammo=30;
    private int magAmmo=30;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    [Header("animation")]
    public Animation animation;
    public AnimationClip reload;
    //public AnimationClip geri_tepme;
    public AnimationClip m4_alis;

    public AudioSource sfx;
    public AudioClip m4_ates;
    public AudioClip m4_reload;
    public AudioClip m4_hazirlama;

    // Geri tepme ile ilgili değişkenler
    [Header("Recoil")]
    [SerializeField] private float maxRecoil = 0.7f; // Maksimum geri tepme miktarı
    [SerializeField] private float recoilSpeed = 1f; // Geri tepme hızı
    private float currentRecoil = 0f;

    // Geri tepme miktarını kontrol etmek için ek değişkenler
    private float verticalRecoil = 0f;
    private float zRecoil = 0f;
    private float zRecoilbelirleme=0f;

    public float minX, maxX;
    public float minY, maxY;
    public Transform camera;
    Vector3 rot;


    void Start()
    {
        ammoText.text=ammo+"/"+mag;
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerController = GetComponentInParent<PlayerController>();
        //Recoil_Script=transform.Find("CameraRot/CameraRecoil").GetComponent<Recoil>();
    }
    void Update()
    {
        if (playerController.y==0&& animasyonicin_activem4.activeSelf == true)
        {
            animation.Play(m4_alis.name);
            sfx.clip=m4_hazirlama;
            sfx.Play();
            playerController.y=1;
        }
        if(Input.GetKeyDown(KeyCode.R)&&animation.isPlaying==false)
        {
            Reload();
        }
        rot = camera.transform.localRotation.eulerAngles;
        if (rot.x != 0 || rot.y != 0)
        {
            camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 3);

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
        // Silah geri tepmesini uygula
        
    }

    private void Shoot()
    {
        //Recoil_Script.RecoilFire();
        ammo--;//mermi azalt
        sfx.clip=m4_ates;
        sfx.Play();

        // Silah geri tepmesi ile ilgili kodları burada kullanabilirsiniz.
        // Örneğin, her atışta rastgele bir geri tepme miktarı belirleyebilirsiniz.
        float recoilAmount = Random.Range(0f, maxRecoil);
        currentRecoil += recoilAmount;
        currentRecoil = Mathf.Clamp(currentRecoil, 0f, maxRecoil);
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
        recoil();
    }
    private void recoil()
    {
        float recX = Random.Range(minX, maxX);
        float recY = Random.Range(minY, maxY);
        camera.transform.localRotation = Quaternion.Euler(rot.x - recY, rot.y + recX, rot.z);
    }
    private void ApplyRecoil()
    {
        // Silahın geri tepmesini uygula
        verticalRecoil = Random.Range(0f, currentRecoil); // Y ekseninde rastgele geri tepme
        if(zRecoil<=2f)
        {
            zRecoilbelirleme=Random.Range(0f,0.2f);
            zRecoil = zRecoil+zRecoilbelirleme; // Z ekseninde çok küçük bir rastgele geri tepme
        }
        else
        {
            zRecoil=0f;
        }
        
        // Silahın pozisyonunu güncelle
        Vector3 recoilVector = new Vector3(0f, verticalRecoil, zRecoil);
        transform.localPosition -= recoilVector * recoilSpeed * Time.deltaTime;

        // Silahın pozisyonunu sınırla (isteğe bağlı olarak)
        // Örneğin, aşağıdaki kodla silahın pozisyonunu sınırlayabilirsiniz.
        // transform.localPosition = Vector3.ClampMagnitude(transform.localPosition, maxRecoil);

        // Geri tepmeyi azalt
        currentRecoil -= Time.deltaTime * recoilSpeed;
        currentRecoil = Mathf.Clamp(currentRecoil, 0f, maxRecoil);
    }

    void Reload()
    {
        if(ammo!=30)
        {
            if(mag==0)
            {
                return;
            }
            animation.Play(reload.name);
            sfx.clip=m4_reload;
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



    /*private void ApplyRecoil()
    {
        animation.Play(geri_tepme.name);
    }*/

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
