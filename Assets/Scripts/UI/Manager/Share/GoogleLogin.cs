using Google;
using System.Threading.Tasks;
using System.Collections;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoogleLogin : MonoBehaviour
{
    public string GoogleAPI = "209969054683-n6mqmht4i1ajql2jr167jd6plta351p6.apps.googleusercontent.com";

    private GoogleSignInConfiguration _configuration;

    private Firebase.Auth.FirebaseAuth _auth;
    private Firebase.Auth.FirebaseUser _user;

    private bool isGoogleSignInInitialized = false;

    private void Start()
    {
        InitFirebase();
    }
    private void InitFirebase()
    {
        _auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    }
    public void Login()
    {
        if ((!isGoogleSignInInitialized))
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration()
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };

            isGoogleSignInInitialized=true;
        }
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId=GoogleAPI
        };
        GoogleSignIn.Configuration.RequestEmail = true;

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                Debug.Log("Cancelled");
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
                Debug.Log("Faulted " + task.Exception);
            }
            else
            {
                Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if(authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                    }
                    else if(authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                        Debug.Log("Faulted In Auth" + task.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        Debug.Log("Success");
                        _user = _auth.CurrentUser;
                        Debug.Log(_user.DisplayName);
                        Debug.Log(_user.Email); 
                    }
                });
            }
        });
    }
}