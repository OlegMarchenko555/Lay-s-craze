using UnityEngine;
using System.Collections;

namespace WhackAMole
{
    public class WAMMole : MonoBehaviour
    {
        // A referemce to the Game Controller, which is taken by the first time this script runs, and is remembered across all other scripts of this type
        static GameObject gameController;

        public GameObject ParticleObj;

        // The animated part of the pack. By default this is taken from this object
        internal Animator packAnimator;

        [Tooltip("health of pack")]
        public int health = 1;

        [Tooltip("sound of wanted pack")]
        public AudioClip CorrectClip;
        public AudioClip WrongClip;

        [Tooltip("type of pack")]
        public int power = 0;

        [Tooltip("index of pack")]
        public int index = 1;

        [Tooltip("A multiplier for the bonus we get when hitting this target")]
        public int bonusMultiplier = 1;

        [Tooltip("The tag of the object that can hit this pack")]
        public string targetTag = "Player";

        // Is the pack dead?
        internal bool isDead = false;

        // How long to wait before showing the pack
        internal float showTime = 0;

        // How long to wait before hiding the pack, after it has been revealed
        public float hideDelay = 0;

        [Tooltip("The animation name when showing a pack")]
        public string animationShow = "packShow";

        [Tooltip("The animation name when hiding a pack")]
        public string animationHide = "packHide";

        [Tooltip("The animation name when hitting a pack")]
        public string animationHit = "packHit";

        // Use this for initialization
        void Start()
        {
            // Hold the gamcontroller object in a variable for quicker access
            if (gameController == null) gameController = GameObject.FindGameObjectWithTag("GameController");

            // The animator of the pack. This holds all the animations and the transitions between them
            packAnimator = GetComponent<Animator>();

            float fAngle = Random.Range(-7.0f, 7.0f);
            transform.localEulerAngles = new Vector3(0, 0, fAngle);
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        void Update()
        {
            // Count down the time until the pack is hidden
            if (isDead == false && hideDelay > 0)
            {
                hideDelay -= Time.deltaTime;

                // If enough time passes, hide the pack
                if (hideDelay <= 0) Hidepack();
            }
        }

        /// <summary>
        /// Raises the trigger enter2d event. Works only with 2D physics.
        /// </summary>
        /// <param name="other"> The other collider that this object touches</param>
        void OnTriggerEnter2D(Collider2D other)
        {
            // Check if we hit the correct target
            if (isDead == false && other.tag == targetTag)
            {
                // Give hit bonus for this target
                gameController.SendMessage("HitBonus", this.transform);

                if(GetComponent<WAMMole>().power == 1)
                {
                    ParticleObj.SetActive(true);
                }

                // Change the health of the target
                ChangeHealth(-1);

                hideDelay = 0;
            }
        }

        /// <summary>
        /// Changes the health of the target, and checks if it should die
        /// </summary>
        /// <param name="changeValue"></param>
        public void ChangeHealth(int changeValue)
        {
            // Chnage health value
            health += changeValue;

            if (health > 0)
            {
                // Animated the hit effect
//                packAnimator.Play(animationHit, -1, 0f);
            }
            else
            {
                // Health reached 0, so the target is dead
                Die();
            }
        }

        /// <summary>
        /// Kills the object and gives it a random animation from a list of death animations
        /// </summary>
        public void Die()
        {
            // The pack is now dead. It can't move.
            isDead = true;

            packAnimator.Play(animationHit);
        }

        /// <summary>
        /// Hides the target, animating it and then sets it to hidden
        /// </summary>
        void Hidepack()
        {
            // Play the hiding animation
            GetComponent<Animator>().Play(animationHide);

            if (PlayerPrefs.GetInt("Pack") == GetComponent<WAMMole>().index)
            {
                if (FindObjectOfType<WAMGameController>().bEffect && FindObjectOfType<WAMGameController>().iEffect == 3)
                {

                }
                else
                {
                    gameController.SendMessage("ChangeLife", -1);
                }
            }
        }

        /// <summary>
        /// Destroys the target object ( this is called from the Animator Die clip )
        /// </summary>
        public void RemoveTarget()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Shows the regular pack
        /// </summary>
        /// <returns>The target.</returns>
        public void ShowPack(float showDuration)
        {
            // The pack is not dead anymore, so it can appear from the hole
            isDead = false;

            // Set the health of the pack to 1 hit
            health = 1;

            // Play the show animation
            GetComponent<Animator>().Play(animationShow);

            // Set how long to wait before hiding the pack again
            hideDelay = showDuration;
        }

        public void PlaySound()
        {
            if(GetComponent<WAMMole>().power == 0)
            {
                if(PlayerPrefs.GetInt("Pack") == GetComponent<WAMMole>().index)
                {
                    GetComponent<AudioSource>().PlayOneShot(CorrectClip);
                }
                else
                {
                    GetComponent<AudioSource>().PlayOneShot(WrongClip);
                }
            }
            else
            {
                GetComponent<AudioSource>().Play();
            }
        }
    }
}
