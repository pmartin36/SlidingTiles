using TMPro;
using UnityEngine;

public class LeaderboardRow : MonoBehaviour
{
	public TMP_Text Rank;
	public TMP_Text UserName;
	public TMP_Text Time;

    void Start() {
		
    }

    void Update() {
        
    }

	public void SetRow(int rank, string name, string time, Material m) {
		SetValues(rank, name, time);
		SetMaterial(m);
	}

	public void SetValues(int rank, string name, string time) {
		Rank.text = rank.ToString();
		UserName.text = name;
		Time.text = time;
	}

	public void SetMaterial(Material m) {
		Rank.fontMaterial = m;
		UserName.fontMaterial = m;
		Time.fontMaterial = m;
	}

	public void SetSize(float textRatio) {
		RectTransform rt = Rank.GetComponent<RectTransform>();
		Rank.fontSize *= textRatio;
		var rts = rt.sizeDelta;
		rts.x *= textRatio;
		rt.sizeDelta = rts;

		rt = UserName.GetComponent<RectTransform>();
		UserName.fontSize *= textRatio;
		rts = rt.sizeDelta;
		rts.x *= textRatio;
		rt.sizeDelta = rts;

		rt = Time.GetComponent<RectTransform>();
		Time.fontSize *= textRatio;
		rts = rt.sizeDelta;
		rts.x *= textRatio;
		rt.sizeDelta = rts;
	}
}
