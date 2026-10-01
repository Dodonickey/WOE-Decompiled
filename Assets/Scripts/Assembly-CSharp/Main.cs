using System.Globalization;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

public class Main : MonoBehaviour
{
	public static float m_gameTime;

	public static float m_gameDeltaTime;

	public static int m_gameTicks;

	public static int m_targetFPS = 60;

	public static IGame m_currentGame;

	public static bool m_paused;

	private void Start()
	{
        //Extras had to be added to fix some issues specifically with the standalone windows builds.
        #region Extras
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        UnityEngine.Debug.logger.logEnabled = false;
#endif
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;//For languages that use a , as a decimal seperator.
        ZipConstants.DefaultCodePage = 65001; //weird error
        IpConfig.Load();//Added in order to make chaning server url 10x easier since this game is basically dead and youll likely be using a custom server.
		#endregion
		m_currentGame = new PsGame("PlaySomething", "0-0-1");
        m_currentGame.Initialize(new StartupScene("StartupScene"));
        //m_currentGame.Initialize(new FrameworkTestScene("FrameworkTestScene"));
    }

	private void Update()
	{
		m_currentGame.Update();
	}
}
