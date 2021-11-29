using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSetter : MonoBehaviour
{
    [SerializeField] GameObject en, fa;
    public Action setLanguage;
    void Start()
    {
        LanguageManager.SettingLanguage += LanguageSet;
        if (PlayerPrefs.GetString("language") == "FA")
        {
            en.SetActive(false);
            fa.SetActive(true);
        }
        else if (PlayerPrefs.GetString("language") == "EN")
        {
            en.SetActive(true);
            fa.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        LanguageManager.SettingLanguage -= LanguageSet;
    }

    private void LanguageSet()
    {
        if (PlayerPrefs.GetString("language") == "FA")
        {
            en.SetActive(false);
            fa.SetActive(true);
        }
        else if (PlayerPrefs.GetString("language") == "EN")
        {
            en.SetActive(true);
            fa.SetActive(false);
        }
    }
}
