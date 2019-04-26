using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WhackAMole.Types;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace WhackAMole
{
	/// <summary>
	/// This script controls the game, starting it, following game progress, and finishing it with game over.
	/// </summary>
	public class WAMGameController : MonoBehaviour 
	{
        //The player object that aims and attacks. In our case this is the hammer object.
        internal Transform playerObject;

        [Tooltip("How fast the player object moves. This is only for keyboard and gamepad controls")]
        public float playerObjectSpeed = 15;

        // The player animator holds all the animations of the player object
        internal Animator playerAnimator;

        // Are we using the mouse now?
        internal bool usingMouse = false;

        // The position we are aiming at now
        internal Vector3 aimPosition;

        [Tooltip("How long to wait before starting the game. Ready?GO! time")]
        public float startDelay = 1;

        [Tooltip("The effect displayed before starting the game")]
        public Transform readyGoEffect;

        [Tooltip("How many seconds are left before game over")]
        public float timeLeft = 180;

        [Tooltip("The text object that displays the time")]
        public Text timeText;

        [Tooltip("How many lifes are left before game over")]
        public float lifeLeft = 5;

        [Tooltip("The text object that displays the life")]
        public Text lifeText;

        [Tooltip("A list of positions where the targets can appear")]
        public Transform[] spawnPositions;

        [Tooltip("A list of targets ( The packs that appear and hide in the holes )")]
        public Transform[] packs;

        [Tooltip("A list of targets ( The powers that appear and hide in the holes )")]
        public Transform[] powers;

        [Tooltip("How many targets to show at once")]
        public int maximumTargets = 5;

        [Tooltip("How long to wait before showing the targets")]
        public float birthDelay = 0.4f;
        public float birthDefaultDelay = 0.4f;
        private float fBirthTime;

        public int iSuccessCount = 0;

        public float hideDefaultDelay = 1.0f;
        public float hideDelay;


        //      [Tooltip("How long to wait before hiding the targets again")]
        //      public float hideDelay = 2;
        //internal float hideDelayCount = 0;

        [Tooltip("The attack button, click it or tap it to attack with the hammer")]
        public string attackButton = "Fire1";

        [Tooltip("How many points we get when we hit a target. This bonus is increased as we hit more targets")]
        public int hitTargetBonus = 40;

        [Tooltip("Counts the current hit streak, which multiplies your bonus for each hit. The streak is reset after the targets hide")]
        internal int streak = 1;

        [Tooltip("The bonus effect that shows how much bonus we got when we hit a target")]
        public Transform bonusEffect;

        [Tooltip("The score of the player")]
        public int score = 0;

        [Tooltip("The score of the player")]
        public bool bPower = false;
        public int iPower = -1;
        public float fPowerTotalTime = 5.0f;
        public Text PowerTimeText;

        [Tooltip("The score text object which displays the current score of the player")]
        public Transform scoreText;
		internal int highScore = 0;
		internal int scoreMultiplier = 1;

		// Various canvases for the UI
		public Transform gameCanvas;
		public Transform gameOverCanvas;

		// Is the game over?
		internal bool  isGameOver = false;


        public List<Sprite> PowerSpriteList = new List<Sprite>();
        public Image PowerImg;
        public GameObject PowerPos;

        public List<Sprite> EffectSpriteList = new List<Sprite>();
        public SpriteRenderer EffectImg;

        // Various sounds and their source
        public AudioClip soundGameOver;
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;
		
		// A general use index
		internal int index = 0;

		//public Transform slowMotionEffect;


		void Awake()
		{
            if ( playerObject == null )
            {
                playerObject = GameObject.FindGameObjectWithTag("Player").transform;

                playerAnimator = playerObject.GetComponent<Animator>();
            }

            fBirthTime = birthDelay;
            hideDelay = hideDefaultDelay;
        }

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Start()
		{
			// Check if we are running on a mobile device. If so, remove the playerObject as we don't need it for taps
			if ( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android )    
			{
				// If a playerObject is assigned, hide it
				//if ( playerObject )    playerObject.gameObject.SetActive(false);
				
				//playerObject = null;
			}

			//Update the score
			UpdateScore();
            
			//Hide the cavases
			if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);

			//Get the highscore for the player
			highScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "HighScore", 0);

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			//// Reset the spawn delay
			//hideDelayCount = 0;

			// Move the targets from one side of the screen to the other, and then reset them
			/*foreach ( Transform movingTarget in packs )
			{
				movingTarget.SendMessage("HideMole");
			}*/

			// Create the ready?GO! effect
			if ( readyGoEffect )    Instantiate( readyGoEffect );
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void  Update()
		{
            if (bPower)
            {
                if (fPowerTotalTime <= 0)
                {
                    switch (iPower)
                    {
                        case 0:
                            scoreMultiplier = 1;
                            break;
                        case 1:
                            break;
                        case 2:
                            EffectImg.enabled = false;
                            hideDelay = hideDefaultDelay;
                            birthDelay = birthDefaultDelay;
                            break;
                        case 3:
                            break;
                        case 4:
                            break;
                        case 5:
                            break;
                        case 6:
                            EffectImg.enabled = false;
                            hideDelay = hideDefaultDelay;
                            birthDelay = birthDefaultDelay;
                            break;
                        case 7:
                            EffectImg.enabled = false;
                            hideDelay = hideDefaultDelay;
                            birthDelay = birthDefaultDelay;
                            ShowTargets(maximumTargets);
                            break;
                    }
                    PowerTimeText.text = "";
                    PowerImg.enabled = false;
                    bPower = false;
                }
                else
                {
                    PowerTimeText.text = ((int)fPowerTotalTime).ToString();
                    fPowerTotalTime -= Time.deltaTime;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(GameOver(0));
            }

            // Delay the start of the game
            if ( startDelay > 0 )
			{
				startDelay -= Time.deltaTime;
			}
			else
			{
				//If the game is over, listen for the Restart and MainMenu buttons
				if ( isGameOver == true )
				{
				}
				else
				{
					// Count down the time until game over
					if ( timeLeft > 0 )
					{
						// Count down the time
						timeLeft -= Time.deltaTime;

						// Update the timer
						UpdateTime();
					}

					// Keyboard and Gamepad controls
					if ( playerObject)
					{
						// If we move the mouse in any direction, then mouse controls take effect
						if ( Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 || Input.GetMouseButtonDown(0) || Input.touchCount > 0 )    usingMouse = true;

						// We are using the mouse, hide the playerObject
						if ( usingMouse == true )
						{
							// Calculate the mouse/tap position
							aimPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
							
							// Make sure it's 2D
							aimPosition.z = 0;
						}

						// If we press gamepad or keyboard arrows, then mouse controls are turned off
						if ( Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 )    
						{
							usingMouse = false;
						}

						// Move the playerObject based on gamepad/keyboard directions
						aimPosition += new Vector3( Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), aimPosition.z) * playerObjectSpeed * Time.deltaTime;
					
						// Limit the position of the playerObject to the edges of the screen
						// Limit to the left screen edge
						if ( aimPosition.x < Camera.main.ScreenToWorldPoint(Vector3.zero).x )    aimPosition = new Vector3( Camera.main.ScreenToWorldPoint(Vector3.zero).x, aimPosition.y, aimPosition.z);
						
						// Limit to the right screen edge
						if ( aimPosition.x > Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x )    aimPosition = new Vector3( Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x, aimPosition.y, aimPosition.z);
						
						// Limit to the bottom screen edge
						if ( aimPosition.y < Camera.main.ScreenToWorldPoint(Vector3.zero).y )    aimPosition = new Vector3( aimPosition.x, Camera.main.ScreenToWorldPoint(Vector3.zero).y, aimPosition.z);
						
						// Limit to the top screen edge
						if ( aimPosition.y > Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height).y )    aimPosition = new Vector3( aimPosition.x, Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height).y, aimPosition.z);

                        // Place the playerObject at the position of the mouse/tap, with an added offset
                        //playerObject.position = aimPosition;

                        playerObject.eulerAngles = Vector3.forward * (playerObject.position.x - aimPosition.x) * -10;

                        // Move the hammer towards the aim posion
                        //if ( Vector3.Distance(playerObject.position, aimPosition) > Time.deltaTime * playerObjectSpeed )    playerObject.position = Vector3.MoveTowards(playerObject.position, aimPosition, Time.deltaTime * playerObjectSpeed);
                        //else    playerObject.position = aimPosition;
                        playerObject.position = aimPosition;
                        // If we press the shoot button, SHOOT!
                        //if ( usingMouse == false && Input.GetButtonDown(attackButton) )    Shoot();

                        if ( playerObject && !EventSystem.current.IsPointerOverGameObject() && Input.GetButtonUp(attackButton) )
                        {
                            playerAnimator.Play("HammerDown");
                        }
                    }

                    if(fBirthTime > 0)
                    {
                        fBirthTime -= Time.deltaTime;
                    }
                    else
                    {
                        fBirthTime = birthDelay;
                        ShowTargets(maximumTargets);
                    }

                    //// Count down to the next target spawn
                    //if (hideDelayCount > 0)
                    //{
                    //    hideDelayCount -= Time.deltaTime;

                    //}
                    //else
                    //{
                    //    // Reset the spawn delay count
                    //    hideDelayCount = birthDelay;

                    //    ShowTargets(maximumTargets);
                    //}
                }
            }
		}

		/// <summary>
		/// Updates the timer text, and checks if time is up
		/// </summary>
		void UpdateTime()
		{
			// Update the timer text
			if ( timeText )    
			{
                int iMin = (int)(timeLeft / 60.0f);
                int iSecond = (int)(timeLeft - (float)(iMin) * 60.0f);
				timeText.text = iMin.ToString() + ":" + iSecond.ToString("00");
			}

			// Time's up!
			if ( timeLeft <= 0 )    
			{
				StartCoroutine(GameOver(0));
			}
		}

		/// <summary>
		/// Shows a random batch of targets. Due to the random nature of selection, some targets may be selected more than once making the total number of targets to appear smaller.
		/// </summary>
		/// <param name="targetCount">The maximum number of target that will appear</param>
		void ShowTargets( int targetCount )
		{
            //// Remove any targets from previous levels
            //WAMMole[] previousTargets = GameObject.FindObjectsOfType<WAMMole>();

            //// Go through each object found, and remove it
            //foreach ( WAMMole previousTarget in previousTargets )
            //{
            //    Destroy(previousTarget.gameObject);
            //}

			// Limit the number of tries when showing targets, so we don't get stuck in an infinite loop
			int maximumTries = targetCount * 5;

            int isPower = Mathf.FloorToInt(Random.Range(0, 3));

            // Show several targets that are within the game area
            while ( targetCount > 0 && maximumTries > 0 )
			{
				maximumTries--;

                // Choose a random target
                int randomPack = 0;
                int randomPower = 0;

                randomPack = Mathf.FloorToInt(Random.Range(0, packs.Length));
                randomPower = Mathf.FloorToInt(Random.Range(0, powers.Length));

                // Choose a random spawn position
                int randomPosition = Mathf.FloorToInt(Random.Range(0, spawnPositions.Length));

                int positionChangeTries = spawnPositions.Length;

                // If the random spawn position is occupied by another target, go to the next position
                while ( spawnPositions[randomPosition].GetComponentInChildren<WAMMole>() && positionChangeTries > 0 )
                {
                    // Go to the next available position
                    if (randomPosition < spawnPositions.Length - 1) randomPosition++;
                    else randomPosition = 0;

                    // Reduce from the number of tries
                    positionChangeTries--;
                }
                
                if (!spawnPositions[randomPosition].GetComponentInChildren<WAMMole>())
                {
                    if(targetCount == 1 && isPower > 0)
                    {
                        // Create the target object at the spawn position
                        Transform newTarget = Instantiate(powers[randomPower], spawnPositions[randomPosition].position, Quaternion.identity);
                        // Put the target inside the spawn position object
                        newTarget.SetParent(spawnPositions[randomPosition]);

                        // Show a random animation state for the spawned target
                        newTarget.SendMessage("ShowPack", hideDelay);

                        targetCount--;
                    }
                    else
                    {
                        // Create the target object at the spawn position
                        Transform newTarget = Instantiate(packs[randomPack], spawnPositions[randomPosition].position, Quaternion.identity);
                        // Put the target inside the spawn position object
                        newTarget.SetParent(spawnPositions[randomPosition]);

                        // Show a random animation state for the spawned target
                        newTarget.SendMessage("ShowPack", hideDelay);

                        targetCount--;
                    }
                }
            }

			// Reset the streak when showing a batch of new targets
			streak = 1;
		}

		/// <summary>
		/// Give a bonus when the target is hit. The bonus is multiplied by the number of targets on screen
		/// </summary>
		/// <param name="hitSource">The target that was hit</param>
		void HitBonus( Transform hitSource )
		{
            if(hitSource.GetComponent<WAMMole>().power == 0)
            {
                if (PlayerPrefs.GetInt("Pack") == hitSource.GetComponent<WAMMole>().index)
                {
                    // If we have a bonus effect
                    if (bonusEffect)
                    {
                        // Create a new bonus effect at the hitSource position
                        Transform newBonusEffect = Instantiate(bonusEffect, hitSource.position + Vector3.up, Quaternion.identity) as Transform;

                        // Display the bonus value multiplied by a streak
                        //if (hitSource.GetComponent<WAMMole>().bonusMultiplier >= 0) newBonusEffect.Find("Text").GetComponent<Text>().text = "+" + (hitTargetBonus * scoreMultiplier).ToString();
                        //else 

                        if(iPower == 0 && bPower)
                        {
                            newBonusEffect.Find("Text").GetComponent<Text>().color = Color.green;
                            newBonusEffect.Find("Text").GetComponent<Outline>().effectColor = Color.black;
                            newBonusEffect.Find("Text").GetComponent<Text>().text = (hitTargetBonus * scoreMultiplier).ToString();
                        }
                        else
                        {
                            newBonusEffect.Find("Text").GetComponent<Text>().text = (hitTargetBonus * scoreMultiplier).ToString();
                        }
                        // Rotate the bonus text slightly
                        newBonusEffect.eulerAngles = Vector3.forward * Random.Range(-10, 10);

                        // Add the bonus to the score
                        ChangeScore(hitTargetBonus * scoreMultiplier);// streak * hitSource.GetComponent<WAMMole>().bonusMultiplier);

                        iSuccessCount++;
                        int iSpeed = iSuccessCount / 5;

                        if(iSuccessCount == iSpeed * 5)
                        {
                            birthDefaultDelay *= (1.0f - iSpeed * 0.1f);
                            hideDefaultDelay *= (1.0f - iSpeed * 0.1f);
                        }

                        if (birthDefaultDelay < 0.1f)
                        {
                            birthDefaultDelay = 0.1f;
                        }

                        if (hideDefaultDelay < 0.2f)
                        {
                            hideDefaultDelay = 0.2f;
                        }
                    }
                }
                else
                {
                    if(bPower && iPower == 3)
                    {

                    }
                    else
                    {
                        ChangeLife(-1);
                    }
                }
            }
            else
            {
                switch(hitSource.GetComponent<WAMMole>().index)
                {
                    case 0:
                        hideDelay = hideDefaultDelay;
                        birthDelay = birthDefaultDelay;
                        scoreMultiplier = 2;

                        EffectImg.sprite = EffectSpriteList[0];
                        EffectImg.enabled = true;

                        iPower = 0;
                        fPowerTotalTime = 10.0f;
                        PowerTimeText.text = "10";
                        PowerImg.sprite = PowerSpriteList[0];
                        PowerImg.enabled = true;
                        bPower = true;
                        break;
                    case 1:
                        EffectImg.enabled = false;

                        // Remove any targets from previous levels
                        WAMMole[] previousTargets1 = GameObject.FindObjectsOfType<WAMMole>();

                        // Go through each object found, and remove it
                        foreach (WAMMole previousTarget in previousTargets1)
                        {
                            if (PlayerPrefs.GetInt("Pack") != previousTarget.index && previousTarget.power == 0)
                            {
                                Destroy(previousTarget.gameObject);
                            }
                        }
                        break;
                    case 2:
                        scoreMultiplier = 1;
                        hideDelay = hideDefaultDelay * 2;
                        birthDelay = birthDefaultDelay * 2;

                        EffectImg.sprite = EffectSpriteList[2];
                        EffectImg.enabled = true;

                        // Remove any targets from previous levels
                        WAMMole[] previousTargets2 = GameObject.FindObjectsOfType<WAMMole>();

                        // Go through each object found, and remove it
                        foreach (WAMMole previousTarget in previousTargets2)
                        {
                            previousTarget.hideDelay *= 2;
                            if (previousTarget.hideDelay > hideDelay)
                            {
                                previousTarget.hideDelay = hideDelay;
                            }
                        }

                        iPower = 2;
                        fPowerTotalTime = 5.0f;
                        PowerTimeText.text = "5";
                        PowerImg.sprite = PowerSpriteList[2];
                        PowerImg.enabled = true;
                        bPower = true;
                        break;
                    case 3:
                        hideDelay = hideDefaultDelay;
                        birthDelay = birthDefaultDelay;
                        scoreMultiplier = 1;

                        EffectImg.sprite = EffectSpriteList[3];
                        EffectImg.enabled = true;

                        iPower = 3;
                        fPowerTotalTime = 10.0f;
                        PowerTimeText.text = "10";
                        PowerImg.sprite = PowerSpriteList[3];
                        PowerImg.enabled = true;
                        bPower = true;
                        break;
                    case 4:
                        EffectImg.sprite = EffectSpriteList[4];
                        EffectImg.enabled = true;

                        StartCoroutine(GameOver(0));
                        break;
                    case 5:
                        EffectImg.sprite = EffectSpriteList[5];
                        EffectImg.enabled = true;

                        // Remove any targets from previous levels
                        WAMMole[] previousTargets5 = GameObject.FindObjectsOfType<WAMMole>();

                        // Go through each object found, and remove it
                        foreach (WAMMole previousTarget in previousTargets5)
                        {
                            if (PlayerPrefs.GetInt("Pack") == previousTarget.index && previousTarget.power == 0)
                            {
                                Destroy(previousTarget.gameObject);
                            }
                        }
                        break;
                    case 6:
                        scoreMultiplier = 1;
                        hideDelay = hideDefaultDelay / 2.0f;
                        birthDelay = birthDefaultDelay / 2.0f;

                        EffectImg.sprite = EffectSpriteList[6];
                        EffectImg.enabled = true;

                        // Remove any targets from previous levels
                        WAMMole[] previousTargets6 = GameObject.FindObjectsOfType<WAMMole>();

                        // Go through each object found, and remove it
                        foreach (WAMMole previousTarget in previousTargets6)
                        {
                            previousTarget.hideDelay /= 2;
                        }

                        iPower = 6;
                        fPowerTotalTime = 10.0f;
                        PowerTimeText.text = "10";
                        PowerImg.sprite = PowerSpriteList[6];
                        PowerImg.enabled = true;
                        bPower = true;
                        break;
                    case 7:
                        scoreMultiplier = 1;
                        hideDelay = 10;
                        birthDelay = 10;

                        EffectImg.sprite = EffectSpriteList[7];
                        EffectImg.enabled = true;

                        // Remove any targets from previous levels
                        WAMMole[] previousTargets7 = GameObject.FindObjectsOfType<WAMMole>();

                        // Go through each object found, and remove it
                        foreach (WAMMole previousTarget in previousTargets7)
                        {
                            previousTarget.hideDelay = hideDelay;
                        }

                        iPower = 7;
                        fPowerTotalTime = 5.0f;
                        PowerTimeText.text = "5";
                        PowerImg.sprite = PowerSpriteList[7];
                        PowerImg.enabled = true;
                        bPower = true;
                        break;
                }
            }

            // Increase the hit streak
            //			streak++;
        }

        /// <summary>
        /// Change the score
        /// </summary>
        /// <param name="changeValue">Change value</param>
        void ChangeScore(int changeValue)
        {
            score += changeValue;

            //Update the score
            UpdateScore();
        }

        /// <summary>
        /// Updates the score value and checks if we got to the next level
        /// </summary>
        void UpdateScore()
        {
            //Update the score text
            if (scoreText) scoreText.GetComponent<Text>().text = score.ToString();
        }

        /// <summary>
        /// Change the life
        /// </summary>
        /// <param name="changeValue">Change value</param>
        void ChangeLife(int changeValue)
        {
            lifeLeft += changeValue;

            //Update the score
            UpdateLife();
        }

        /// <summary>
        /// Updates the life value and checks if we got to the next level
        /// </summary>
        void UpdateLife()
        {
            if (lifeLeft <= 0)
            {
                lifeLeft = 0;

                //Update the score text
                if (lifeText) lifeText.GetComponent<Text>().text = lifeLeft.ToString();

                StartCoroutine(GameOver(0));
            }
            else
            {
                //Update the score text
                if (lifeText) lifeText.GetComponent<Text>().text = lifeLeft.ToString();
            }
        }

        /// <summary>
        /// Set the score multiplier ( Get double score for hitting and destroying targets )
        /// </summary>
        void SetScoreMultiplier( int setValue )
		{
			// Set the score multiplier
			scoreMultiplier = setValue;
		}
		
        /// <summary>
        /// Runs the game over event and shows the game over screen
        /// </summary>
        IEnumerator GameOver(float delay)
		{
			isGameOver = true;

			yield return new WaitForSeconds(delay);
			
			//Remove the pause and game screens
            if ( gameCanvas )    gameCanvas.gameObject.SetActive(false);

            //Show the game over screen
            if ( gameOverCanvas )    
			{
				//Show the game over screen
				gameOverCanvas.gameObject.SetActive(true);
				
				//If there is a source and a sound, play it from the source
				if ( soundSource && soundGameOver )    
				{
					soundSource.GetComponent<AudioSource>().pitch = 1;
					
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
				}
			}
		}

		/// <summary>
		/// Restart the current level
		/// </summary>
		void  Restart()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  MainMenu()
		{
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            Application.Quit();
        }
    }
}