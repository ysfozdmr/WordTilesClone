using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
   public void ShowMenu()
    {
        gameObject.SetActive(true);

    }
    public void HideMenu()
    {
        gameObject.SetActive(false);
    }
}
