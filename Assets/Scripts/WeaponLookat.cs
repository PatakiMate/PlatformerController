using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLookat : MonoBehaviour
{
    private PlayerInputController _pController;

    private void Start()
    {
        _pController = transform.root.GetComponent<PlayerInputController>();
    }
    void Update()
    {
        if (HasController() == false)
        {
            Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            Vector3 aimInput = new Vector3(_pController.AimAxis.x, _pController.AimAxis.y, 0.0f);
            if (aimInput.sqrMagnitude < 0.1f)
            {
                return;
            }
            var otherAngle = Mathf.Atan2(_pController.AimAxis.y, _pController.AimAxis.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(otherAngle, Vector3.forward);
        }
    }

    private bool HasController()
    {
        string[] names = Input.GetJoystickNames();
        if(names.Length > 0)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
