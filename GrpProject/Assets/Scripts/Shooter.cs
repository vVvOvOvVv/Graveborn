using UnityEngine;
using TMPro;
using EZCameraShake;
using System.Collections;
using UnityEngine.UI;

public class Shooter : MonoBehaviour
{  
    [SerializeField] private Weapon currentWeapon;   

    // TMPro elements from the HUD
    [SerializeField] private TextMeshProUGUI currentAmmoTxt, reserveAmmoTxt;
    // Slider element to indicate reload time
    [SerializeField] private Slider reloadSlider;
    private GameObject reloadSliderObj;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    // ensure order: gunshot, empty mag but attempts to shoot, reload

    public bool gamePaused;

    // determine method of fire - hold down or click LMB
    private void ShootInput()
    {
        if (currentWeapon.allowButtonHold)
            currentWeapon.shooting = Input.GetKey(KeyCode.Mouse0);
        else currentWeapon.shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && !gamePaused &&
            currentWeapon.currentAmmo < currentWeapon.maxAmmo && !currentWeapon.isReloading)
        { 
            audioSource.PlayOneShot(clips[2]);
            currentWeapon.Reload();
            StartCoroutine(VisualizeReload());
        }

        // shoot
        if (currentWeapon.readyToShoot && currentWeapon.shooting && 
            !currentWeapon.isReloading && currentWeapon.currentAmmo > 0 &&
            !gamePaused)
        {
            currentWeapon.bulletsShot = currentWeapon.bulletsPerTap;
            CameraShaker.Instance.ShakeOnce(1f, 1f, 0.1f, 0.1f);
            currentWeapon.Shoot();
            for (int i = 0; i < currentWeapon.bulletsPerTap; i++)
                audioSource.PlayOneShot(clips[0]);
        } else if (currentWeapon.shooting && currentWeapon.currentAmmo == 0 && // attempts to shoot empty
            !gamePaused) 
            audioSource.PlayOneShot(clips[1]);

        // Switch weapons based on key input
        if (Input.GetKeyDown(KeyCode.Alpha1) && !gamePaused) 
        {
            Inventory.Instance.SwitchWeaponByIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !gamePaused)
        { 
            Inventory.Instance.SwitchWeaponByIndex(1);
        } 
    }

    public void UpdateAmmoText()
    {
        currentAmmoTxt.SetText(currentWeapon.currentAmmo.ToString());
        reserveAmmoTxt.SetText(currentWeapon.reserveAmmo.ToString());
    }

    private IEnumerator VisualizeReload()
    { 
        float elapsedTime = 0f;
        reloadSliderObj.SetActive(true);

        while (elapsedTime < currentWeapon.reloadTime)
        {
            elapsedTime += Time.deltaTime;
            reloadSlider.value = Mathf.Lerp(0, 1, elapsedTime / currentWeapon.reloadTime);
            yield return null; // wait for next frame
        }
         
        reloadSlider.value = 0f;
        reloadSliderObj.SetActive(false); 
    }

    void Start()
    { 
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
        gamePaused = false;
        if (reloadSlider != null)
            reloadSliderObj = reloadSlider.gameObject;
        if (reloadSliderObj != null) 
            reloadSliderObj.SetActive(false); // hide from player
        audioSource = GetComponent<AudioSource>();
    } 

    void Update()
    {
        currentWeapon = Inventory.Instance.GetCurrentWeapon();
        if (currentWeapon != null && !gamePaused)
        {
            ShootInput();  
            UpdateAmmoText();
        } 
    }
}
