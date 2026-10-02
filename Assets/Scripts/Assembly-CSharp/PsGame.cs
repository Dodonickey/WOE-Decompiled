using System;
using UnityEngine;

public class PsGame : IGame
{
	private string _projectCode;

	private string _projectVersion;

	private IScene _currentScene;

	private SceneManager _sceneManager;

	private float m_delta;

	private float m_cumulatedFrameTime;

	public static TextC m_debugTXC;

	public static float m_dt = 1f / 60f;

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

	public PsGame(string _projectCode, string _projectVersion)
	{
		m_sceneManager = new SceneManager();
		Application.targetFrameRate = 60;
		m_projectCode = _projectCode;
		m_projectVersion = _projectVersion;
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
		Debug.Initialize(false, false, false, false);
#else
        Debug.Initialize(true, true, true, true);
#endif
		ResourceManager.GenerateResourceGroupFromFolder("Framework");
		ResourceManager.GenerateResourceGroupFromFolder("PlaySomething/Autogeometry");
		ResourceManager.GenerateResourceGroupFromFolder("PlaySomething/Ground");
		ResourceManager.GenerateResourceGroupFromFolder("PlaySomething/UI/UiAtlas");
		ResourceManager.GenerateResourceGroupFromFolder("PlaySomething/FX/Shadows");
		ResourceManager.GenerateResourceGroupFromFolder("PlaySomething/FX/ParticleFx");
		ResourceManager.GenerateResourceGroupFromFolder("PlaySomething/FX/SpecialMaterials");
		ResourceManager.LoadAndAddResourceToGroup("Fonts/HurmeRegular", "Fonts");
		ResourceManager.LoadAndAddResourceToGroup("Fonts/HurmeSemiBold", "Fonts");
		ResourceManager.LoadAndAddResourceToGroup("Fonts/HurmeBold", "Fonts");
		ResourceManager.LoadAndAddResourceToGroup("Fonts/HurmeSemiBoldMN", "Fonts");
		ResourceManager.LoadAndAddResourceToGroup("Fonts/KGLetHerGo", "Fonts");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/PlayerUnits/Alien/test", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/PlayerUnits/Alien/AlienRagdollAnimation", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/PlayerUnits/Alien/AlienAnimatorController", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/PlayerUnits/Alien/AlienAnimatorOverrideController", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/PlayerUnits/UnitPoliceCar/PoliceCarPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Goals/GoalBot/GoalBotPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Obstacles/MetalCrate/MetalCratePrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Obstacles/WoodenCrate/WoodenCratePrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Obstacles/ExplodingBarrel/ExplodingBarrelPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/SpeedRamp/SpeedRampPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/DangerPlatform/DangerPlatformPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/BigGear/GearPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/SawBlade/SawBladePrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/BreakingPlatforms/BreakingPlatformMediumPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/MetalPlatforms/MetalPlatformSmallPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/MetalPlatforms/MetalPlatformMediumPrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Static/MetalPlatforms/MetalPlatformLargePrefab", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Dynamic/Planks/Box", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Dynamic/Planks/BoxPlank", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Dynamic/Planks/Plank500", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Units/Props/Dynamic/Planks/Plank250", "Units");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Environments/Desert/Middle/Props/Cliff1/DesertMiddlePropCliff1", "Props");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Environments/Desert/Middle/Props/Cliff2/DesertMiddlePropCliff2", "Props");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Environments/Desert/Background/Terrain/DesertBackgroundTerrain", "Desert");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Environments/Desert/Front/Terrain/DesertFrontTerrain", "Desert");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Environments/Desert/Road/Terrain/DesertRoadTerrain", "Desert");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Environments/Desert/Background/DesertBackgroundMat", "Desert");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Shaders/UI/AnonymousProfile", "UI");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Shaders/UI/ScreenshotSeparateAlpha", "UI");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Shaders/UI/ScreenshotMat", "UI");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Shaders/UI/ProfileMat", "UI");
		ResourceManager.LoadAndAddResourceToGroup("PlaySomething/Shaders/UI/LoadingScreenFadeMat", "UI");
		EntityManager.Initialize();
		TransformS.Initialize();
		PrefabS.Initialize();
		TextS.Initialize();
		TextMeshS.Initialize();
		TouchAreaS.Initialize();
		TweenS.Initialize();
		EventS.Initialize();
		TimerS.Initialize();
		SpriteS.Initialize();
		ProjectorS.Initialize();
		CameraS.Initialize();
		SoundS.Initialize(CameraS.m_mainCamera.gameObject);
		SoundS.SetMute(PsState.m_mute);
		Array values = Enum.GetValues(typeof(PsCollisionType));
		ChipmunkProS.Initialize((int)values.GetValue(values.Length - 1));
		ChipmunkProWrapper.ucpSpaceSetCollisionSlop(1f);
		ChipmunkProWrapper.ucpSpaceSetSleepTimeThreshold(1f);
		ChipmunkProWrapper.ucpSpaceSetIdleSpeedThreshold(5f);
		ChipmunkProWrapper.ucpSpaceSetIterations(10);
		ChipmunkProWrapper.ucpSpaceSetGravity(new Vector2(0f, -550f));
		PsGlobalCollisionHandlers.Initialize();
		PsUnitDatabase.Initialize();
		PsS.Initialize();
		DebugDraw.m_lineWidth = 3f;
		Font font = TextS.AddFont(ResourceManager.GetMaterial("Framework/Museo14-material"), ResourceManager.GetTextAsset("Framework/Museo14-properties"), 1000, 256, 128, 1f, CameraS.m_uiCamera);
		Style style = TextS.AddStyle("body", font);
		style.color = new Color(1f, 1f, 1f, 1f);
		TextS.SetStyle("body");
		DebugDraw.Initialize();
		PsState.m_uiSheet = SpriteS.AddSpriteSheet(CameraS.m_uiCamera, ResourceManager.GetMaterial("UiAtlas/UiAtlasMat"), ResourceManager.GetTextAsset("UiAtlas/UiAtlas"), 1f);
	}

	public void Initialize(IScene _scene)
	{
		m_sceneManager.ChangeScene(_scene);
	}

	public void RemoveComponent(IComponent _c)
	{
		switch (_c.m_componentType)
		{
		case (ComponentType)32:
			PsS.RemoveGround(_c as GroundC);
			break;
		case (ComponentType)31:
			PsS.RemoveItem(_c as ItemC);
			break;
		case (ComponentType)30:
			PsS.RemoveUnit(_c as UnitC);
			break;
		}
	}

	public void Update()
	{
		BundleLoader.Update();
		if (!BundleLoader.NeedsToWait())
		{
			Main.m_gameDeltaTime = m_dt;
			Main.m_gameTicks++;
			Main.m_gameTime += m_dt;
			ChipmunkProS.Update((!PsState.m_editorPaused && !PsState.m_gamePaused) ? m_dt : 0f);
			TouchAreaS.Update();
			m_sceneManager.UpdateLogic();
			UIManager.Update();
			LevelManager.Update();
			PsS.Update();
			EntityManager.UpdateLogic();
			TweenS.Update();
			TransformS.Update();
			SpriteS.Update();
			PrefabS.Update();
			TextS.Update();
			TextMeshS.Update();
			EventS.Update();
			TimerS.Update();
			ProjectorS.Update();
			CameraS.Update();
			SoundS.Update();
			EntityManager.Update();
			PostRequestQueue.Update();
			ServerManager.Update();
			CacheManager.Update();
			if (PsState.m_transformGizmo != null)
			{
				PsState.m_transformGizmo.UpdatePosition();
			}
		}
	}
}
