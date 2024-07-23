using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class akmermi : Gun
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject muzzleFlashPrefab; // Namlu flaşı efekti prefabı
    private PhotonView PV;
    [Header("Ammo")]
    public int mag=5;
    public int ammo=30;
    public int magAmmo=30;

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;

    [Header("animation")]
    public Animation animation;
    public AnimationClip reload;

    // Geri tepme ile ilgili değişkenler
    [Header("Recoil")]
    [SerializeField] private float maxRecoil = 1f; // Maksimum geri tepme miktarı
    [SerializeField] private float recoilSpeed = 3f; // Geri tepme hızı
    private float currentRecoil = 0f;

    // Geri tepme miktarını kontrol etmek için ek değişkenler
    private float verticalRecoil = 0f;
    private float zRecoil = 0f;

    void Start()
    {
        magText.text=mag.ToString();
        ammoText.text=ammo+"/"+magAmmo;
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
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
        // Silah geri tepmesi ile ilgili kodları burada kullanabilirsiniz.
        // Örneğin, her atışta rastgele bir geri tepme miktarı belirleyebilirsiniz.
        float recoilAmount = Random.Range(0f, maxRecoil);
        currentRecoil += recoilAmount;
        currentRecoil = Mathf.Clamp(currentRecoil, 0f, maxRecoil);
        magText.text=mag.ToString();
        ammoText.text=ammo+"/"+magAmmo;
        

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
        if(ammo!=30)
        {
            if(mag==0)
            {
                return;
            }
            animation.Play(reload.name);

            if(mag>0)
            {
                mag--;
                ammo=magAmmo;
            }
            magText.text=mag.ToString();
            ammoText.text=ammo+"/"+magAmmo;
        }
        else
        {
            return;
        }
        
    }

    private void ApplyRecoil()
    {
        // Silahın geri tepmesini uygula
        verticalRecoil = Random.Range(0f, currentRecoil); // Y ekseninde rastgele geri tepme
        zRecoil = Random.Range(-0.01f, 0.01f); // Z ekseninde çok küçük bir rastgele geri tepme

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
