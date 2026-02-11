using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagingSkills : MonoBehaviour
{
    [Header("UI References")]
    public Image iconDash;         // Gold Box UI Image
    public Image icon2ndSkill;     // Bronze Box UI Image
    public TMP_Text notifyText;    // Notification Text

    [Header("Notification Styling")]
    [SerializeField] private float notifyFontSize = 28f;   // tweak in Inspector if needed
    [SerializeField] private Color notifyFontColor = Color.white;

    public void SkillUnlock(string nameSkill)
    {
        if (notifyText != null)
        {
            notifyText.color = notifyFontColor;     // white
            notifyText.fontSize = notifyFontSize;   // moderate size
        }

        if (nameSkill == "Dash")
        {
            if (iconDash != null)
                iconDash.color = new Color(1f, 1f, 1f, 1f);

            if (notifyText != null)
                notifyText.text = "Dash Skill Unlocked!  Shift to Dash";
        }
        else if (nameSkill == "Skill 2nd")
        {
            if (icon2ndSkill != null)
                icon2ndSkill.color = new Color(1f, 1f, 1f, 1f);

            if (notifyText != null)
                notifyText.text = "Skill 2nd Unlocked!!!!";
        }

        CancelInvoke(nameof(ClearText));
        Invoke(nameof(ClearText), 3f);
    }

    void ClearText()
    {
        if (notifyText != null)
            notifyText.text = "";
    }
}
