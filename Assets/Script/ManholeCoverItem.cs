using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManholeCoverItem : Item
{
    public List<ManholeCoverItem> links;

    public Animator m_animator;

    public int InOrOut = 0;

    private void Awake()
    {
        m_animator.speed = 0;
    }

    public void JumpOut()
    {
        m_animator.speed = 1;
        m_animator.Play("JingGai_Animation", 0, 0);
        AudioPlay.Instance.PlayJumpOut();
        InOrOut = 0;
    }

    public void JumpIn()
    {
        m_animator.speed = 1;
        m_animator.Play("JingGai_Animation", 0, 0);
        AudioPlay.Instance.PlayJumpIn();
        InOrOut = 1;
    }

    public void OpenSound()
    {
        if(InOrOut==1)
        {
            AudioPlay.Instance.PlaySFX(40);
        }
        else
        {
            AudioPlay.Instance.PlaySFX(42);
        }
    }

    public void CloseSound()
    {
        if (InOrOut == 1)
        {
            AudioPlay.Instance.PlaySFX(41);
        }
        else
        {
            AudioPlay.Instance.PlaySFX(43);
        }
    }
}
