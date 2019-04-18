using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace WhackAMole
{
	/// <summary>
	/// Includes functions for loading levels and URLs. It's intended for use with UI Buttons
	/// </summary>
	public class WAMLoadLevel : MonoBehaviour
	{
        [Tooltip("How many seconds to wait before loading a level or URL")]
        public float loadDelay = 1;

        [Tooltip("The name of the level to be loaded")]
        public string levelName = "";

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
		}

		/// <summary>
		/// Loads the level.
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public void LoadLevel()
		{
            if(levelName == "End")
            {
                Application.Quit();
            }
            else
            {
                Time.timeScale = 1;

                // Execute the function after a delay
                Invoke("ExecuteLoadLevel", loadDelay);
            }
        }

		/// <summary>
		/// Executes the Load Level function
		/// </summary>
		void ExecuteLoadLevel()
		{
			SceneManager.LoadScene(levelName);
		}

		/// <summary>
		/// Restarts the current level.
		/// </summary>
		public void RestartLevel()
		{
			Time.timeScale = 1;

			// Execute the function after a delay
			Invoke("ExecuteRestartLevel", loadDelay);
		}
		
		/// <summary>
		/// Executes the Load Level function
		/// </summary>
		void ExecuteRestartLevel()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}