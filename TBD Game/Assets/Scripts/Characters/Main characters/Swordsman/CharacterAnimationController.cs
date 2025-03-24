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
        public bool isMoving = false;
        public bool isIdling = false;
        public RuntimeAnimatorController controller;

        private void Awake()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }

            current = Instantiate(animations.GetPrefab(direction), transform);
            animator = current.GetComponent<Animator>();
            if (animator == null)
            {
                animator = current.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = animations.GetRuntimeController(direction);
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
            else if (!isIdling)
            {
                animations.PlayIdle(animator, direction);
                isIdling = true;
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
                    current.SetActive(false);
                    Destroy(current);
                }

                current = Instantiate(prefab, transform);
                animator = current.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = current.AddComponent<Animator>();
                }
                animator.runtimeAnimatorController = animations.GetRuntimeController(direction);
                
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                Debug.Log("Error in swapping character prefab");
            }
        }
    }
}
