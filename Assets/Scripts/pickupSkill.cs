using UnityEngine;

public class pickupSkill: MonoBehaviour {
    public string nameSkill; 
    public ManagingSkills managerSkill;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")){//Makes the ball the "Player" Tag in the Inspector
            
            managerSkill.SkillUnlock(nameSkill); 
            Destroy(gameObject); 
            
        }
    }
}