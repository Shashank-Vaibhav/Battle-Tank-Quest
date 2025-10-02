using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class ButtonManager : MonoBehaviour
{
    private async void Start()
    {
        // Initialize Unity Services if not already initialized
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized.");
        }
    }

    /// <summary>
    /// Load the specified scene based on authentication state.
    /// </summary>
    /// <param name="sceneName">The target scene name to load if authenticated.</param>
    public void GoToScene(string sceneName)
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("User is already signed in. Loading the scene...");
            SceneManager.LoadScene("EnterPlay2");
        }
        else
        {
            Debug.Log("User not signed in. Redirecting to the Authentication Scene...");
            SceneManager.LoadScene(sceneName); // Change to your Authentication scene name
        }
    }

    public void GoToSceneBack(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Change to your Authentication scene name
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitApplication()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
