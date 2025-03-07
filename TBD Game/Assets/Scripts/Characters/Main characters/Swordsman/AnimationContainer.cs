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
                    return Down;
            }
        }

        public RuntimeAnimatorController GetAnimator(string direction)
        {
            switch (direction)
            {
                case "Down":
                    return DownController;
                case "Up":
                    return UpController;
                default:
                    return DownController;
            }
        }
    }
}
