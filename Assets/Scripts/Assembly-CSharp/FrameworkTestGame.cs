using System;
using UnityEngine;

public class FrameworkTestGame : IGame
{
	private string _projectCode;

	private string _projectVersion;

	private IScene _currentScene;

	private SceneManager _sceneManager;

	private float m_prevTime;

	private float m_currentTime;

	private float m_delta;

	private float m_cumulatedFrameTime;

	private static float m_dt = 1f / 60f;

	public string m_projectCode
	{
		get
		{
			return _projectCode;
		}
		set
		{
			_projectCode = value;
		}
	}

	public string m_projectVersion
	{
		get
		{
			return _projectVersion;
		}
		set
		{
			_projectVersion = value;
		}
	}

	public IScene m_currentScene
	{
		get
		{
			return _currentScene;
		}
		set
		{
			_currentScene = value;
		}
	}

	public SceneManager m_sceneManager
	{
		get
		{
			return _sceneManager;
		}
		set
		{
			_sceneManager = value;
		}
	}

	public FrameworkTestGame(string _projectCode, string _projectVersion)
	{
		m_sceneManager = new SceneManager();
		Application.targetFrameRate = 60;
		m_projectCode = _projectCode;
		m_projectVersion = _projectVersion;
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
		Debug.Initialize(false, false, false, false);
#else
        Debug.Initialize(true, true, true, false);
#endif
        ResourceManager.GenerateResourceGroupFromFolder("Framework");
		EntityManager.Initialize();
		TransformS.Initialize();
		PrefabS.Initialize();
		TextS.Initialize();
		TouchAreaS.Initialize();
		TweenS.Initialize();
		EventS.Initialize();
		SpriteS.Initialize();
		CameraS.Initialize();
		Array values = Enum.GetValues(typeof(ucpCollisionType));
		ChipmunkProS.Initialize((int)values.GetValue(values.Length - 1));
		Font font = TextS.AddFont(ResourceManager.GetMaterial("Framework/Museo14-material"), ResourceManager.GetTextAsset("Framework/Museo14-properties"), 1000, 256, 128, 1f, CameraS.m_uiCamera);
		Style style = TextS.AddStyle("body", font);
		style.color = new Color(1f, 1f, 1f, 1f);
		TextS.SetStyle("body");
		DebugDraw.Initialize();
	}

	public void Initialize(IScene _scene)
	{
		m_sceneManager.ChangeScene(_scene, new BasicLoadingScene());
	}

	public void RemoveComponent(IComponent _c)
	{
	}

	public void Update()
	{
		BundleLoader.Update();
		if (!BundleLoader.NeedsToWait())
		{
			m_currentTime = Time.time;
			Main.m_gameDeltaTime = m_currentTime - m_prevTime;
			m_prevTime = m_currentTime;
			Main.m_gameTime += m_dt;
			TouchAreaS.Update();
			LevelManager.Update();
			m_sceneManager.UpdateLogic();
			ChipmunkProS.Update(m_dt);
			TweenS.Update();
			TransformS.Update();
			SpriteS.Update();
			PrefabS.Update();
			TextS.Update();
			EventS.Update();
			CameraS.Update();
			UIManager.Update();
			EntityManager.Update();
		}
	}
}
