//#define GOOGLE_MOBILE_ADS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Ump;
using GoogleMobileAds.Ump.Api;
using GoogleMobileAds.Api;
#endif

public static class UmpManager
{
    public static bool useCMPCheck = false;
    private static bool isPassed = false;

#if GOOGLE_MOBILE_ADS
    private static ConsentForm _consentForm;
#endif

    // Start is called before the first frame update
    public static void Init()
    {
#if GOOGLE_MOBILE_ADS
        if (useCMPCheck)
        {
            Debug.Log("Ump.Init");

            MobileAds.Initialize(initStatus => {
                SetupUMP();
            });
        }
#endif
    }

#if GOOGLE_MOBILE_ADS
    static void SetupUMP()
    {
        Debug.Log("Ump.Setup");
        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {
                "9753CF1E3AB47BDA8E66855D428AD9B6",
                "875ECC7203F2B4A1168491FFDBC8010E"
            }
        };

        // Here false means users are not under age.
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,
            ConsentDebugSettings = debugSettings,
        };

        // Check the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    static void OnConsentInfoUpdated(FormError error)
    {
        Debug.Log("Ump.update");

        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        if (ConsentInformation.IsConsentFormAvailable())
        {
            LoadConsentForm();
        }
    }
#endif

    public static void Show_Ump()
    {
#if GOOGLE_MOBILE_ADS

        isPassed = true;

#if UNITY_EDITOR
        NextStep();
#else
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if (formError != null)
            {
                // Consent gathering failed.
                UnityEngine.Debug.LogError($"Failed to load UMP consent form. Error code: {formError.ErrorCode}, message: {formError.Message}");

                isPassed = false;
            }

            NextStep();
        });
#endif

#endif
    }

#if GOOGLE_MOBILE_ADS

    private static void NextStep()
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("Is_Agree : {0}", Is_Agree()));

        string CMPString = PlayerPrefs.GetString("IABTCF_AddtlConsent");        
        Debug.Log(CodeManager.GetMethodName() + string.Format("CMPString : {0}", CMPString));
        
        if (CMPString.Contains("2878"))
        {
            Debug.Log(CodeManager.GetMethodName() + "setConsent : True");
            IronSource.Agent.setConsent(true);
        }
        else
        {
            Debug.Log(CodeManager.GetMethodName() + "setConsent : False");
            IronSource.Agent.setConsent(false);
        }

        GlobalData.Instance.userManager.UpdateAgree(
            agreeCMP:true,
            agreeCMPState:Is_Agree()
        );
    }

    static void LoadConsentForm()
    {
        Debug.Log("Ump.Load");

        // Loads a consent form.
        ConsentForm.Load(OnLoadConsentForm);
    }

    static void OnLoadConsentForm(ConsentForm consentForm, FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // The consent form was loaded.
        // Save the consent form for future requests.
        _consentForm = consentForm;

        // You are now ready to show the form.
        if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
        {
            //_consentForm.Show(OnShowForm);
        }
    
    #if CMP_TEST_ENABLE
        _consentForm.Show(OnShowForm);
    #endif
    }


    static void OnShowForm(FormError error)
    {
        Debug.Log("Ump.Show");

        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // Handle dismissal by reloading form.
        LoadConsentForm();
    }

    public static bool Is_Agree()
    {
        if (isPassed)
        {
            return CMPDataAccess.Is_Agreed();
        }

        return false;
    }

#endif
}
