using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Unity.Services.CloudSave;

public class NewClientAuthenticationHandler : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;

    [Header("Scene to Load After Authentication")]
    private const string MenuSceneName = "EnterPlay2"; // Replace with your target scene name.

    private static bool _isUnityInitialized = false;

    // Enum to represent authentication states
    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        Error,
        TimeOut
    }

    public static AuthState CurrentAuthState { get; private set; } = AuthState.NotAuthenticated;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject); // Keep this object across scenes
        Debug.Log("AuthenticationHandler Initialized");
        InitializeUnityServices();
    }

    /// <summary>
    /// Initialize Unity Services.
    /// </summary>
    private async void InitializeUnityServices()
    {
        if (!_isUnityInitialized)
        {
            try
            {
                Debug.Log("Initializing Unity Services...");
                await UnityServices.InitializeAsync();
                _isUnityInitialized = true;
                Debug.Log("Unity Services initialized successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Authenticate as a Guest (Anonymous Sign-In).
    /// </summary>
    public async void AuthenticateAsGuest()
    {
        if (!_isUnityInitialized) await EnsureUnityServicesInitialized();

        // Check if already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Already signed in. Redirecting to Menu...");
            GoToMenu();
            return;
        }

        if (CurrentAuthState == AuthState.Authenticated)
        {
            Debug.Log("Already authenticated.");
            return;
        }

        CurrentAuthState = AuthState.Authenticating;
        Debug.Log("Attempting Guest Authentication...");

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (AuthenticationService.Instance.IsSignedIn)
            {
                CurrentAuthState = AuthState.Authenticated;
                Debug.Log("Guest Authentication Successful!");
                GoToMenu();
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Guest Authentication Failed: {ex.Message}");
            CurrentAuthState = AuthState.Error;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}");
            CurrentAuthState = AuthState.Error;
        }
    }

    /// <summary>
    /// Sign-In with Username and Password.
    /// </summary>
    public async void SignInWithUsernamePassword()
    {
        if (!_isUnityInitialized) await EnsureUnityServicesInitialized();

        // Check if already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Already signed in. Redirecting to Menu...");
            GoToMenu();
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Username or Password cannot be empty.");
            return;
        }

        CurrentAuthState = AuthState.Authenticating;
        Debug.Log("Signing in with Username and Password...");

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            if (AuthenticationService.Instance.IsSignedIn)
            {
                CurrentAuthState = AuthState.Authenticated;
                Debug.Log("Username/Password Sign-In Successful!");
                GoToMenu();
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign-In Failed: {ex.Message}");
            CurrentAuthState = AuthState.Error;
        }
    }

    /// <summary>
    /// Sign-Up a New User and Store Additional Data (Name and Email).
    /// </summary>
    public async void SignUpUser()
    {
        if (!_isUnityInitialized) await EnsureUnityServicesInitialized();

        // Check if already signed in
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Already signed in. Redirecting to Menu...");
            GoToMenu();
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;
        string name = nameInput.text;
        string email = emailInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
        {
            Debug.LogError("All fields (Username, Password, Name, Email) are required for Sign-Up.");
            return;
        }

        Debug.Log("Signing up a new user...");

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("User Sign-Up Successful!");

            // Save additional player data
            await SavePlayerData(name, email);

            CurrentAuthState = AuthState.Authenticated;
            GoToMenu();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign-Up Failed: {ex.Message}");
            CurrentAuthState = AuthState.Error;
        }
    }

    /// <summary>
    /// Save player data (Name and Email) to Unity Cloud Save.
    /// </summary>
    private async Task SavePlayerData(string name, string email)
    {
        try
        {
            Debug.Log("Saving player data (Name and Email)...");

            var data = new Dictionary<string, object>
            {
                { "playerName", name },
                { "playerEmail", email }
            };

            await CloudSaveService.Instance.Data.ForceSaveAsync(data);

            Debug.Log("Player data saved successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save player data: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensure Unity Services are initialized.
    /// </summary>
    private async Task EnsureUnityServicesInitialized()
    {
        if (!_isUnityInitialized)
        {
            await UnityServices.InitializeAsync();
            _isUnityInitialized = true;
        }
    }

    /// <summary>
    /// Load the Menu Scene.
    /// </summary>
    private void GoToMenu()
    {
        Debug.Log($"Loading Scene: {MenuSceneName}");
        SceneManager.LoadScene(MenuSceneName);
    }
}
