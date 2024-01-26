using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PreciseLocaleText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = string.Format(
			"LANGUAGE ID: {0} \n" +
			"LANGUAGE: {1} \n " +
			"REGION: {2} \n " +
			"CURRENCY CODE: {3} \n " +
			"CURRENCY SYMBOL: {4}", 
			PreciseLocale.GetLanguageID(),
			PreciseLocale.GetLanguage(), 
			PreciseLocale.GetRegion(), 
			PreciseLocale.GetCurrencyCode(),
			PreciseLocale.GetCurrencySymbol()
		);
	}

}
