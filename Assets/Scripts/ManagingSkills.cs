using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagingSkills : MonoBehaviour{

    [Header("UI References")]

    public Image iconDash;   //Gold Box UI Image 
    public Image icon2ndSkill; //Bronze Box UI Image 
    public TMP_Text notifyText; //Notification Text 

    public void SkillUnlock(string nameSkill){
        
        if (nameSkill == "Dash"){
            
            //Making Alpha to full 255 to once unlocked the icon 
            iconDash.color = new Color(1, 1, 1, 1);
            notifyText.text = "Dash Skill Unlocked!!!!";
        }

        else if (nameSkill == "Skill 2nd"){
            //Making Alpha to full 255 to once unlocked the icon 
            icon2ndSkill.color = new Color(1, 1, 1, 1);
            notifyText.text = "Skill 2nd Unlocked!!!!";
        }
        
        //After 3 seconds text goes away
        Invoke("ClearText", 3f);
    }

    void ClearText() { 
        notifyText.text = ""; 
    }
}
