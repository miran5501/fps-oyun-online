using UnityEngine;

public class PlayAnimationInRange : MonoBehaviour
{
    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload; // Oynatmak istediğiniz animasyonun adı


    private void Update()
    {
        // 'T' tuşuna bastığınızda animasyonu oynatın
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload1();
            
        }
    }
    void Reload1()
    {
            animation.Play(reload.name);
    }
}
