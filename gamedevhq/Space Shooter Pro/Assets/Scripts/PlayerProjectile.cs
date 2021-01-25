using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerProjectile : MonoBehaviour
    {
        protected WeaponManager _weaponManager;
        protected virtual void Start()
        {
            _weaponManager = GameObject.Find("Weapon_Manager").GetComponent<WeaponManager>();
            if (_weaponManager == null) {
                Debug.LogError("Weapons manager is null when creating player weapon.");
            }
        }

        protected virtual void OnDestroy()
        {
            _weaponManager.RemovePlayerShot(this.transform);
        }
    }
}