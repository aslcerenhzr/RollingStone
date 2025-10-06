using UnityEngine;

public class Heart : StateMachineBehaviour
{
    private bool hasStopped = false;

    // Animasyon başladığında
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasStopped = false;
    }

    // Her frame çağrılır (animasyon devam ederken)
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animasyonun sonuna gelindiyse
        if (!hasStopped && stateInfo.normalizedTime >= 1f)
        {
            animator.speed = 0f; // 🔹 animasyonu durdur
            hasStopped = true;
        }
    }
}

