using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Characters.Main_characters.Swordsman
{
    public class CharacterAnimationController : MonoBehaviour
    {
        public GameObject current;
        public AnimationContainer animations;
        private string direction = "Down";

        public Animator animator;
        private Vector2 input;
        private bool isMoving;
        public RuntimeAnimatorController controller;

        private void Awake()
        {
            current = animations.GetPrefab(direction);
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (!isMoving)
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");

                if (input != Vector2.zero)
                {
                    if (direction != GetMainDirection(input))
                    {
                        direction = GetMainDirection(input);
                        ChangePrefab(animations.GetPrefab(direction));
                    }
                }
            }
        }

        public static string GetMainDirection(Vector2 input)
        {
            /*if (input == Vector2.zero)
                return "None"; // No movement

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? "Right" : "Left";
            }
            else
            {
                return input.y > 0 ? "Up" : "Down";
            }*/

            //only implementing up/down animations for now
            return input.y > 0 ? "Up" : "Down";
        }

        public void ChangePrefab(GameObject prefab)
        {
            try
            {
                if (current != null)
                {
                    Destroy(current);
                }

                current = Instantiate(prefab, transform);
                animator = current.GetComponent<Animator>();
                
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                Debug.Log("Error in swapping character prefab");
            }
        }
    }
}
