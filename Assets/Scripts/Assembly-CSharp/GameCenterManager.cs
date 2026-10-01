using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public static class GameCenterManager
{
	public class GameCenterPictureDownloader
	{
		public Action<Texture2D> m_callback;

		public GameCenterPictureDownloader(string _gameCenterId)
		{
			Social.LoadUsers(new string[1] { _gameCenterId }, ImageLoaded);
		}

		private void ImageLoaded(IUserProfile[] users)
		{
			m_callback(users[0].image);
		}
	}

	public static bool m_loginComplete;

    public static void Login(Action _GCLoginComplete)
    {
#if UNITY_IPHONE || UNITY_EDITOR
        Social.localUser.Authenticate(delegate (bool success)
        {
            if (success)
            {
                Debug.Log("GC Login successful");
                string message = "Username: " + Social.localUser.userName + "\nGC ID: " + Social.localUser.id + "\nIsUnderage: " + Social.localUser.underage;
                Debug.Log(message);
            }
            else
            {
                Debug.Log("GC Login failed");
            }
            if (Social.localUser.authenticated)
            {
                PlayerPrefsX.SetGameCenterId(Social.localUser.id);
                PlayerPrefsX.SetGameCenterName(Social.localUser.userName);
            }
            else
            {
                PlayerPrefsX.SetGameCenterId(null);
            }
            m_loginComplete = true;
            _GCLoginComplete();
        });
#else
        Debug.Log("GC Login skipped (Game Center is iOS only)");
        PlayerPrefsX.SetGameCenterId(null);
        PlayerPrefsX.DeleteKey("GameCenterName");
        m_loginComplete = true;
        _GCLoginComplete();
#endif
    }

    public static void Logout()
	{
		Debug.Log("GC Logout");
		PlayerPrefsX.DeleteKey("GameCenterId");
		ServerManager.ReloadFriends();
	}

	public static void NoLogin()
	{
		m_loginComplete = true;
	}

	public static bool IsLoggedIn()
	{
		return PlayerPrefsX.GetGameCenterId() != null;
	}

	public static void GetPicture(string _gameCenterId, Action<Texture2D> _callback)
	{
		if (_gameCenterId == PlayerPrefsX.GetGameCenterId())
		{
			_callback(Social.localUser.image);
			return;
		}
		GameCenterPictureDownloader gameCenterPictureDownloader = new GameCenterPictureDownloader(_gameCenterId);
		gameCenterPictureDownloader.m_callback = (Action<Texture2D>)Delegate.Combine(gameCenterPictureDownloader.m_callback, _callback);
	}
}
