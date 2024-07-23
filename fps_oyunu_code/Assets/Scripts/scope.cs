using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scope : MonoBehaviour
{
    public Camera camera1; // İlk kamera
    public Camera camera2; // İkinci kamera
    public Canvas canvas1; // İlk canvas
    public Canvas canvas2; // İkinci canvas
    public Transform weaponTransform; // Silahın Transform bileşeni

    private bool isCamera1Active = true; // İlk kamera açık mı?

    private Vector3 defaultWeaponPosition; // Silahın başlangıç pozisyonu

    private void Start()
    {
        // Kamera FOV değerlerini kaydet
        

        // Başlangıçta sadece ilk kamera ve ilk canvas açık olsun
        camera1.gameObject.SetActive(true);
        camera2.gameObject.SetActive(false);
        canvas1.gameObject.SetActive(true);
        canvas2.gameObject.SetActive(false);

        // Silahın başlangıç pozisyonunu kaydet
        defaultWeaponPosition = weaponTransform.localPosition;
    }

    private void Update()
    {
        // Sağ tıklama algılama
        if (Input.GetMouseButtonDown(1))
        {
            // İlk kamera açıksa kapat ve ikinciyi aç
            if (isCamera1Active)
            {
                camera1.gameObject.SetActive(false);
                camera2.gameObject.SetActive(true);
                canvas1.gameObject.SetActive(false);
                canvas2.gameObject.SetActive(true);
                isCamera1Active = false;

                // İlk kameranın FOV'u varsayılanına geri yükle

                // Silahı bir tık aşağıya kaydır
                weaponTransform.localPosition = defaultWeaponPosition - Vector3.up;
            }
            // İkinci kamera açıksa kapat ve ilki aç
            else
            {
                camera2.gameObject.SetActive(false);
                camera1.gameObject.SetActive(true);
                canvas2.gameObject.SetActive(false);
                canvas1.gameObject.SetActive(true);
                isCamera1Active = true;

                // Silahın pozisyonunu başlangıç pozisyonuna geri yükle
                weaponTransform.localPosition = defaultWeaponPosition;
            }
        }
    }
}
