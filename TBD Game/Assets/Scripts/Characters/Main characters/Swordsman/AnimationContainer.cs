using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Characters.Main_characters.Swordsman
{
    public class AnimationContainer : MonoBehaviour
    {
        public GameObject Down;
        public RuntimeAnimatorController DownController;

        public GameObject Up;
        public RuntimeAnimatorController UpController;


        public GameObject GetPrefab(string direction)
        {
            switch (direction)
            {
                case "Down":
                    return Down;
                case "Up":
                    return Up;
                default:
                    return null;
            }
        }

        public RuntimeAnimatorController GetRuntimeController(string direction)
        {
            switch (direction)
            {
                case "Down":
                    return DownController;
                case "Up":
                    return UpController;
                default:
                    return null;
            }
        }

        public void PlayIdle(Animator animator, string direction)
        {
            float transitionTime = (float)0.5;

            switch(direction)
            {
                case "Down":
                    animator.CrossFade("idle_down_hub.anim", transitionTime);
                    break;
                case "Up":
                    animator.CrossFade("idle_up_hub.anim", transitionTime);
                    break;
            }
        }
    }
}
