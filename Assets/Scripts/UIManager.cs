using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[System.Serializable]
	public class HUD : System.Object {

		[SerializeField] public GameObject[] shells = new GameObject[2];
		[SerializeField] public Image[] powerIcons = new Image[3];
		[SerializeField] public Image meterBar;
		[SerializeField] public Text damageText;
	
	}

	[SerializeField] HUD[] huds = new HUD[2];

	// Use this for initialization
	void Start() {
		
	}
	
	// Update is called once per frame
	void Update() {
		
	}

	public void SetMeter(int value, int max, int player) {
		float pct = (float)value / max;
		huds[player].meterBar.transform.localScale = new Vector3(pct, 1.0f, 1.0f);
		for (int i = 0; i < huds[player].powerIcons.Length; i++)
		{
			huds[player].powerIcons[i].enabled = false;
		}
		if (value == max)
			huds[player].powerIcons[2].enabled = true;
		if (pct >= 2.0f / 3.0f)
			huds[player].powerIcons[1].enabled = true;
		if (pct >= 1.0f / 3.0f)
			huds[player].powerIcons[0].enabled = true;
	}

	public void SetAmmo(int ammo, int player) {
		for (int i = 0; i < huds[player].shells.Length; i++)
		{
			huds[player].shells[i].SetActive(false);
		}
		if (ammo == 0)
		{
			return;
		}
		for (int i = 0; i < ammo; i++)
		{
			huds[player].shells[i].SetActive(true);
		}
	}

	public void SetDamage(int damage, int player) {
		huds[player].damageText.text = damage.ToString() + "%";
	}
}
