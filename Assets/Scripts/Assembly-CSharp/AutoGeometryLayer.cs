using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGeometryLayer
{
	public const int CAMERA_LAYER = 9;

	public const int GROUND_DEFAULT_DEPTH = 300;

	private Vector3 m_tileOffset = new Vector3(0f, 0f, -75f);

	public int m_width;

	public int m_height;

	public int m_tileSize;

	public int m_xTiles;

	public int m_yTiles;

	public IntPtr m_sampler;

	public IntPtr m_tileCache;

	public IntPtr[] m_dirtyTileList;

	public float m_simplifyTreshold;

	public int m_texelScale;

	public bool m_dirty = true;

	public Material m_beltMaterial;

	public Material m_frontMaterial;

	public Hashtable m_tileHash = new Hashtable();

	private Hashtable m_tilePositionalHash = new Hashtable();

	private Vector2[] m_tempVertices = new Vector2[200];

	private IntPtr[] m_tempPolylines = new IntPtr[200];

	private GenericArray<AgPolygon> m_tempAgPolys;

	private cpBB m_ensureRect;

	private cpBB m_undoRect;

	private bool m_updateEnsureRect;

	private Vector2 m_maxEnsureRect;

	private List<AgTile> m_geometryUpdateQueue = new List<AgTile>();

	private Entity m_groundBodyEntity;

	private TransformC m_groundBodyTC;

	public ChipmunkBodyC m_groundBody;

	public GroundC m_groundC;

	private bool m_plotDidChangePixel;

	private bool m_maskTextureModified;

	private IntPtr[] m_brushPaintTileList = new IntPtr[20];

	private TweenC m_highlightTween;

	private TweenC m_highlightGlowTween;

	public byte[] m_bytes;

	public byte[] m_snapshotBytes;

	public byte[] m_maxValueLookupBytes;

	public Texture2D m_maskTexture;

	public AutoGeometryLayer(Ground _groundClass, int _tileSize, int _samplesPerTile, int _texelScale, float _simplifyTreshold = 1f)
	{
		m_groundBodyEntity = EntityManager.AddEntity("AGLayerGroundBodyEntity");
		m_groundBodyEntity.m_persistent = true;
		m_groundBodyTC = TransformS.AddComponent(m_groundBodyEntity, "AGLayerGroundBodyTC");
		m_groundBody = ChipmunkProS.AddStaticBody(m_groundBodyTC);
		m_groundC = PsS.AddGround(m_groundBodyEntity, _groundClass);
		m_groundBody.customComponent = m_groundC;
		m_tileOffset.z += m_groundC.m_ground.m_zOffset;
		m_tempAgPolys = new GenericArray<AgPolygon>(50);
		m_width = AutoGeometryManager.m_width / _texelScale;
		m_height = AutoGeometryManager.m_height / _texelScale;
		m_texelScale = _texelScale;
		m_tileSize = _tileSize;
		m_bytes = new byte[m_width * m_height];
		m_maxValueLookupBytes = new byte[m_width * m_height];
		m_sampler = AutoGeometryWrapper.agSimpleSamplerNew(outputRect: new cpBB(0.5f * (float)m_texelScale, 0.5f * (float)m_texelScale, ((float)m_width - 0.5f) * (float)m_texelScale, ((float)m_height - 0.5f) * (float)m_texelScale), width: m_width, height: m_height, data: m_bytes);
		m_xTiles = AutoGeometryManager.m_width / _tileSize + 1;
		m_yTiles = AutoGeometryManager.m_height / _tileSize + 1;
		int num = (m_xTiles + 1) * (m_yTiles + 1);
		m_tileCache = AutoGeometryWrapper.agBasicTileCacheNew(m_sampler, ChipmunkProWrapper.ucpGetSpace(), _tileSize, _samplesPerTile, num, false, false);
		m_dirtyTileList = new IntPtr[num];
		m_ensureRect = new cpBB(0f, 0f, 0f, 0f);
		m_updateEnsureRect = false;
		AutoGeometryWrapper.agBasicTileCacheSetOffsets(m_tileCache, AutoGeometryManager.m_tileCacheOffset, Vector2.zero);
		AutoGeometryWrapper.agBasicTileCacheSetMarchProperties(m_tileCache, _groundClass.m_marchHard, _simplifyTreshold);
		AutoGeometryWrapper.agBasicTileCacheSetSegmentProperties(m_tileCache, 5f, 0.5f, 0.5f, 0u, uint.MaxValue, 0u);
		m_maxEnsureRect = new Vector2(512f, 512f);
		m_beltMaterial = ResourceManager.GetMaterial(m_groundC.m_ground.m_beltMaterialResourceName);
		m_frontMaterial = ResourceManager.GetMaterial(m_groundC.m_ground.m_frontMaterialResourceName);
		m_frontMaterial.SetColor("_Emission", Color.black);
		m_beltMaterial.SetColor("_Emission", Color.black);
		InitMaskTexture();
		m_frontMaterial.SetTexture("_MaskTex", m_maskTexture);
	}

	public void SetHighlight(float _emissionAmount, float _speed = 0.1f, bool _flash = false)
	{
		EntityManager.RemoveAllComponentsByType(m_groundBodyEntity, ComponentType.Tween);
		m_highlightTween = null;
		m_highlightGlowTween = null;
		float r = m_frontMaterial.GetColor("_Emission").r;
		if (!_flash)
		{
			if (_emissionAmount > 0f)
			{
				m_highlightTween = TweenS.AddTween(TweenStyle.Linear, new Vector3(r, 0f, 0f), new Vector3(_emissionAmount, 0f, 0f), _speed, 0f);
				m_highlightGlowTween = TweenS.AddTween(TweenStyle.QuadInOut, new Vector3(_emissionAmount, 0f, 0f), new Vector3(_emissionAmount * 0.5f, 0f, 0f), 1f, _speed);
				TweenS.SetAdditionalTweenProperties(m_highlightGlowTween, -1, true, TweenStyle.QuadInOut);
			}
			else
			{
				m_highlightTween = TweenS.AddTween(TweenStyle.Linear, new Vector3(r, 0f, 0f), new Vector3(_emissionAmount, 0f, 0f), _speed, 0f);
			}
		}
		else
		{
			m_highlightTween = TweenS.AddTween(TweenStyle.Linear, new Vector3(r, 0f, 0f), new Vector3(_emissionAmount, 0f, 0f), 0.2f, 0f);
			TweenS.SetAdditionalTweenProperties(m_highlightTween, 0, true, TweenStyle.Linear);
		}
		EntityManager.AddComponentToEntity(m_groundBodyEntity, m_highlightTween);
	}

	public ByteBlock ReadByteBlock(ref byte[] _bytes, cpBB _worldBB)
	{
		cpBB worldBB = _worldBB;
		_worldBB.l /= m_texelScale;
		_worldBB.b /= m_texelScale;
		_worldBB.r /= m_texelScale;
		_worldBB.t /= m_texelScale;
		int num = (int)_worldBB.r - (int)_worldBB.l;
		int num2 = (int)_worldBB.t - (int)_worldBB.b;
		ByteBlock result = default(ByteBlock);
		if (num > 0 && num2 > 0)
		{
			result.xOffset = (int)_worldBB.l;
			result.yOffset = (int)_worldBB.b;
			result.width = num;
			result.height = num2;
			result.bytes = new byte[result.width * result.height];
			result.worldBB = worldBB;
			int num3 = 0;
			for (int i = (int)_worldBB.b; i < (int)_worldBB.t; i++)
			{
				int num4 = 0;
				for (int j = (int)_worldBB.l; j < (int)_worldBB.r; j++)
				{
					if (j >= 1 && i >= 1 && j <= m_width - 2 && i <= m_height - 2)
					{
						int num5 = i * m_width + j;
						int num6 = num3 * result.width + num4;
						result.bytes[num6] = _bytes[num5];
					}
					num4++;
				}
				num3++;
			}
		}
		return result;
	}

	public void WriteByteBlock(ByteBlock _block, ref byte[] _bytes)
	{
		for (int i = 0; i < _block.height; i++)
		{
			for (int j = 0; j < _block.width; j++)
			{
				int num = j + _block.xOffset;
				int num2 = i + _block.yOffset;
				if (num >= 1 && num2 >= 1 && num <= m_width - 2 && num2 <= m_height - 2)
				{
					int num3 = num2 * m_width + num;
					int num4 = i * _block.width + j;
					_bytes[num3] = _block.bytes[num4];
				}
			}
		}
	}

	private void InitMaskTexture()
	{
		m_maskTexture = new Texture2D(m_width, m_height, TextureFormat.Alpha8, false);
		m_maskTexture.wrapMode = TextureWrapMode.Clamp;
		m_maskTexture.anisoLevel = 0;
		m_maskTexture.filterMode = FilterMode.Bilinear;
	}

	public void CopyByteArrayToMaskTexture(Texture2D _texture, byte[] _bytes)
	{
		Color[] array = new Color[_bytes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color(0f, 0f, 0f, (float)(int)_bytes[i] / 255f);
		}
		_texture.SetPixels(array);
		_texture.Apply();
	}

	public void TakeSnapshot()
	{
	}

	public void ResetUndoRect()
	{
		m_undoRect = new cpBB(0f, 0f, 0f, 0f);
	}

	public cpBB GetUndoRect()
	{
		return m_undoRect;
	}

	public bool HasUndoRect()
	{
		return m_undoRect.r - m_undoRect.l > 0f || m_undoRect.t - m_undoRect.b > 0f;
	}

	public void RevertAllDirtyTiles(byte[] source)
	{
		List<AgTile> list = new List<AgTile>();
		foreach (AgTile value in m_tileHash.Values)
		{
			if (value.dirty)
			{
				list.Add(value);
			}
		}
		foreach (AgTile item in list)
		{
			if (!item.dirty)
			{
				continue;
			}
			cpBB bb = item.bb;
			bb.l /= m_texelScale;
			bb.b /= m_texelScale;
			bb.r /= m_texelScale;
			bb.t /= m_texelScale;
			for (int i = (int)bb.b; i < (int)bb.t; i++)
			{
				for (int j = (int)bb.l; j < (int)bb.r; j++)
				{
					int num = i * m_width + j;
					if (j >= 1 && i >= 1 && j <= m_width - 2 && i <= m_height - 2)
					{
						m_bytes[num] = source[num];
					}
				}
			}
			AutoGeometryWrapper.agBasicTileCacheMarkDirtyRect(m_tileCache, item.bb);
			AutoGeometryWrapper.agBasicTileCacheEnsureRect(m_tileCache, item.bb);
			m_dirty = true;
			UpdateSegments();
		}
		CopyByteArrayToMaskTexture(m_maskTexture, m_bytes);
	}

	private cpBB clampWorldBB(cpBB _bb)
	{
		_bb.l = ToolBox.limitBetween(_bb.l, 0f, AutoGeometryManager.m_width - 1);
		_bb.b = ToolBox.limitBetween(_bb.b, 0f, AutoGeometryManager.m_height - 1);
		_bb.r = ToolBox.limitBetween(_bb.r, 0f, AutoGeometryManager.m_width - 1);
		_bb.t = ToolBox.limitBetween(_bb.t, 0f, AutoGeometryManager.m_height - 1);
		return _bb;
	}

	public void MarchTiles(cpBB _fullRect)
	{
		_fullRect = clampWorldBB(_fullRect);
		_fullRect.l = Mathf.FloorToInt(_fullRect.l / (float)m_tileSize) * m_tileSize;
		_fullRect.r = Mathf.CeilToInt(_fullRect.r / (float)m_tileSize) * m_tileSize;
		_fullRect.b = Mathf.FloorToInt(_fullRect.b / (float)m_tileSize) * m_tileSize;
		_fullRect.t = Mathf.CeilToInt(_fullRect.t / (float)m_tileSize) * m_tileSize;
		float num = _fullRect.r - _fullRect.l;
		float num2 = _fullRect.t - _fullRect.b;
		int num3 = Mathf.Min(Mathf.Min(Mathf.CeilToInt(num / (float)m_tileSize) * m_tileSize, Mathf.CeilToInt(num2 / (float)m_tileSize) * m_tileSize), 1024);
		int num4 = Mathf.CeilToInt(num / (float)num3);
		int num5 = Mathf.CeilToInt(num2 / (float)num3);
		int num6 = Mathf.CeilToInt(num / (float)num4);
		int num7 = Mathf.CeilToInt(num2 / (float)num5);
		cpBB cpBB2 = new cpBB(0f, 0f, 0f, 0f);
		Debug.Log("Generating " + m_groundC.m_ground.m_name + " material:");
		Debug.Log(string.Concat("Marching all tiles in ", _fullRect, " with splitsX: ", num4, " and splitsY: ", num5));
		Debug.Log("SplitSize: " + num6 + "/" + num7);
		for (int i = 0; i < num5; i++)
		{
			cpBB2.b = _fullRect.b + (float)(i * num7) + 32f;
			cpBB2.t = cpBB2.b + (float)num7 - 64f;
			for (int j = 0; j < num4; j++)
			{
				cpBB2.l = _fullRect.l + (float)(j * num6) + 32f;
				cpBB2.r = cpBB2.l + (float)num6 - 64f;
				cpBB2 = clampWorldBB(cpBB2);
				AutoGeometryWrapper.agBasicTileCacheMarkDirtyRect(m_tileCache, cpBB2);
				AutoGeometryWrapper.agBasicTileCacheEnsureRect(m_tileCache, cpBB2);
				m_dirty = true;
				UpdateSegments();
			}
		}
	}

	public float ReadDataFromWorldPosAsFloat(Vector2 _worldPos)
	{
		return (float)(int)ReadDataFromWorldPos(_worldPos) / 255f;
	}

	public byte ReadDataFromWorldPos(Vector2 _worldPos)
	{
		_worldPos -= AutoGeometryManager.m_tileCacheOffset;
		Vector2 vector = _worldPos / m_texelScale;
		if (vector.x >= 0f && vector.y >= 0f && vector.x <= (float)(m_width - 1) && vector.y <= (float)(m_height - 1))
		{
			int num = (int)vector.y * m_width + (int)vector.x;
			return m_bytes[num];
		}
		return 0;
	}

	public byte ReadDataFromImagePos(Vector2 _imagePos)
	{
		Vector2 vector = _imagePos;
		if (vector.x >= 0f && vector.y >= 0f && vector.x <= (float)(m_width - 1) && vector.y <= (float)(m_height - 1))
		{
			int num = (int)vector.y * m_width + (int)vector.x;
			return m_bytes[num];
		}
		return 0;
	}

	public byte ReadDataFromDataPos(int _dataPos)
	{
		return m_bytes[_dataPos];
	}

	private bool IsTileFilled(cpBB _tileBB)
	{
		Vector2 vector = new Vector2(_tileBB.l - (_tileBB.l - _tileBB.r) * 0.5f, _tileBB.t - (_tileBB.t - _tileBB.b) * 0.5f);
		Vector2 vector2 = vector / m_texelScale;
		int num = m_tileSize / m_texelScale / 2;
		int num2 = (int)vector2.x - num / 2;
		int num3 = (int)vector2.y - num / 2;
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				num4 += ReadDataFromImagePos(new Vector2(num2 + j, num3 + i));
			}
		}
		return num4 > 128 * num * num;
	}

	private void PlotPixel(float _val, int _targetByteOffset, AGDrawMode _drawMode, ref byte[] _bytes)
	{
		float num = (float)(int)_bytes[_targetByteOffset] / 255f;
		byte b = 0;
		switch (_drawMode)
		{
		case AGDrawMode.ADD:
		{
			float num4 = 1f - num;
			float num5 = 1f - _val;
			float a = 1f - num4 * num5;
			float num6 = (float)(int)m_maxValueLookupBytes[_targetByteOffset] / 255f;
			a = Mathf.Min(a, 1f - num6);
			b = (byte)(a * 255f);
			break;
		}
		case AGDrawMode.SUB:
		{
			float num2 = num;
			float num3 = 1f - _val;
			b = (byte)(num2 * num3 * 255f);
			break;
		}
		}
		if (m_bytes[_targetByteOffset] != b)
		{
			m_plotDidChangePixel = true;
		}
		_bytes[_targetByteOffset] = b;
	}

	private int GetByteOffset(float _x, float _y)
	{
		if (_x >= 1f && _y >= 1f && _x <= (float)(m_width - 1) && _y <= (float)(m_height - 1))
		{
			return (int)_y * m_width + (int)_x;
		}
		return -1;
	}

	public void PaintWithBrush(AutoGeometryBrush _brush, Vector2 _pos, AGDrawMode _drawMode, bool _soften, ref byte[] _bytes, bool _updateAG = true, bool _updateMask = false)
	{
		_pos -= AutoGeometryManager.m_tileCacheOffset;
		Vector2 pos = _pos;
		m_plotDidChangePixel = false;
		_pos /= (float)m_texelScale;
		_pos -= new Vector2((float)_brush.m_width * 0.5f, (float)_brush.m_height * 0.5f);
		for (int i = 0; i < _brush.m_height; i++)
		{
			int num = i * _brush.m_width;
			for (int j = 0; j < _brush.m_width; j++)
			{
				float num2 = _pos.x + (float)j;
				float num3 = _pos.y + (float)i;
				float num4 = 1f;
				float num5 = (float)(int)_brush.m_bytes[num + j] / 255f;
				int byteOffset;
				if (_soften)
				{
					float num6 = num2 - Mathf.Floor(num2);
					float num7 = num3 - Mathf.Floor(num3);
					int num8 = ((!(num6 < 0.5f)) ? 1 : (-1));
					int num9 = ((!(num7 < 0.5f)) ? 1 : (-1));
					float positionBetween = ToolBox.getPositionBetween(Mathf.Abs(num6 - 0.5f), 0f, 0.5f);
					float positionBetween2 = ToolBox.getPositionBetween(Mathf.Abs(num7 - 0.5f), 0f, 0.5f);
					float num10 = 0.25f * positionBetween * (1f - positionBetween2);
					float num11 = 0.25f * positionBetween2 * (1f - positionBetween);
					float num12 = 0.25f * positionBetween * positionBetween2;
					num4 = 1f - (num10 + num11 + num12);
					byteOffset = GetByteOffset(num2 + (float)num8, num3);
					if (byteOffset > 0)
					{
						PlotPixel(num10 * num5, byteOffset, _drawMode, ref _bytes);
					}
					byteOffset = GetByteOffset(num2, num3 + (float)num9);
					if (byteOffset > 0)
					{
						PlotPixel(num11 * num5, byteOffset, _drawMode, ref _bytes);
					}
					byteOffset = GetByteOffset(num2 + (float)num8, num3 + (float)num9);
					if (byteOffset > 0)
					{
						PlotPixel(num12 * num5, byteOffset, _drawMode, ref _bytes);
					}
				}
				byteOffset = GetByteOffset(num2, num3);
				if (byteOffset > 0)
				{
					PlotPixel(num4 * num5, byteOffset, _drawMode, ref _bytes);
				}
			}
		}
		if (_updateMask && m_plotDidChangePixel)
		{
			m_maskTextureModified = true;
			int num13 = _brush.m_width + 2;
			int num14 = _brush.m_height + 2;
			_pos.x -= 1f;
			_pos.y -= 1f;
			if ((int)_pos.x < 1)
			{
				num13 -= 1 - (int)_pos.x;
				_pos.x = 1f;
			}
			if ((int)_pos.y < 1)
			{
				num14 -= 1 - (int)_pos.y;
				_pos.y = 1f;
			}
			if ((int)_pos.x + num13 >= m_width - 2)
			{
				num13 -= (int)_pos.x + num13 - (m_width - 1);
			}
			if ((int)_pos.y + num14 >= m_height - 2)
			{
				num14 -= (int)_pos.y + num14 - (m_height - 1);
			}
			Color[] array = new Color[num13 * num14];
			Color color = new Color(0f, 0f, 0f, 0f);
			for (int k = 0; k < num14; k++)
			{
				int num15 = k * num13;
				for (int l = 0; l < num13; l++)
				{
					float x = _pos.x + (float)l;
					float y = _pos.y + (float)k;
					int byteOffset2 = GetByteOffset(x, y);
					if (byteOffset2 >= 0)
					{
						array[num15 + l] = new Color(0f, 0f, 0f, (float)(int)_bytes[byteOffset2] / 255f);
					}
					else
					{
						array[num15 + l] = color;
					}
				}
			}
			m_maskTexture.SetPixels((int)_pos.x, (int)_pos.y, num13, num14, array);
		}
		if (!_updateAG || !m_plotDidChangePixel)
		{
			return;
		}
		if (_drawMode == AGDrawMode.SUB)
		{
			Vector2 area = new Vector2(_brush.m_width + 2, _brush.m_height + 2) * m_texelScale;
			cpBB bb = new cpBB(pos.x - area.x / 2f, pos.y - area.y / 2f, pos.x + area.x / 2f, pos.y + area.y / 2f);
			bb = clampWorldBB(bb);
			int num16 = AutoGeometryWrapper.agBasicTileCacheGetTileCountInRect(m_tileCache, bb);
			AutoGeometryWrapper.agBasicTileCacheGetTilesInRect(m_tileCache, bb, m_brushPaintTileList);
			for (int m = 0; m < num16; m++)
			{
				IntPtr intPtr = m_brushPaintTileList[m];
				AgTile agTile = GetAgTile(intPtr, false);
				if (agTile != null)
				{
					AutoGeometryWrapper.agCachedTileSetDirty(intPtr, true);
				}
			}
			updateRect(pos, area, false);
		}
		else
		{
			updateRect(pos, new Vector2(_brush.m_width + 2, _brush.m_height + 2) * m_texelScale);
		}
	}

	private void updateRect(Vector2 _pos, Vector2 _area, bool _markDirty = true)
	{
		cpBB bb = new cpBB(_pos.x - _area.x / 2f, _pos.y - _area.y / 2f, _pos.x + _area.x / 2f, _pos.y + _area.y / 2f);
		bb = clampWorldBB(bb);
		if (_markDirty)
		{
			AutoGeometryWrapper.agBasicTileCacheMarkDirtyRect(m_tileCache, bb);
		}
		if (!m_updateEnsureRect)
		{
			m_ensureRect = bb;
			m_updateEnsureRect = true;
		}
		else
		{
			cpBB ensureRect = ChipmunkProWrapper.ucpBBMerge(bb, m_ensureRect);
			float num = ensureRect.r - ensureRect.l;
			float num2 = ensureRect.t - ensureRect.b;
			if (num * num2 > m_maxEnsureRect.x * m_maxEnsureRect.y)
			{
				UpdateSegments();
				m_ensureRect = bb;
				m_updateEnsureRect = true;
			}
			else
			{
				m_ensureRect = ensureRect;
			}
		}
		if (HasUndoRect())
		{
			m_undoRect = ChipmunkProWrapper.ucpBBMerge(m_undoRect, bb);
		}
		else
		{
			m_undoRect = bb;
		}
		m_dirty = true;
	}

	private void FixEdgeVertNormalsWithNeighbour(AgTile _tile, int x, int y)
	{
		AgTile tileNeighbour = GetTileNeighbour(_tile, x, y);
		bool flag = false;
		if (tileNeighbour != null && tileNeighbour.edgeVerts != null && tileNeighbour.edgeVerts.Length > 4)
		{
			for (int i = 0; i < _tile.edgeVerts.Length - 4; i++)
			{
				for (int j = 0; j < tileNeighbour.edgeVerts.Length - 4; j++)
				{
					if (_tile.edgeVerts[i].pos == tileNeighbour.edgeVerts[j].pos)
					{
						Vector2 normal = _tile.edgeVerts[i].normal;
						Vector2 normal2 = tileNeighbour.edgeVerts[j].normal;
						Vector2 vector = (normal + normal2) / 2f;
						_tile.edgeVerts[i].fixedNormal = vector;
						_tile.edgeVerts[i].cTangent = new Vector2(normal2.y, 0f - normal2.x);
						if (vector != tileNeighbour.edgeVerts[j].fixedNormal)
						{
							tileNeighbour.edgeVerts[j].fixedNormal = vector;
							tileNeighbour.edgeVerts[j].cTangent = new Vector2(normal.y, 0f - normal.x);
							flag = true;
						}
					}
				}
			}
		}
		if (flag && !m_geometryUpdateQueue.Contains(tileNeighbour))
		{
			m_geometryUpdateQueue.Add(tileNeighbour);
		}
	}

	private Vector2 GetFixedEdgeVertNormalForPoint(AgTile _tile, Vector2 _pos)
	{
		if (_tile.edgeVerts != null)
		{
			for (int i = 0; i < _tile.edgeVerts.Length - 4; i++)
			{
				if (_pos == _tile.edgeVerts[i].pos)
				{
					return _tile.edgeVerts[i].fixedNormal;
				}
			}
		}
		return Vector2.one;
	}

	public void GenerateEdgeVertArray(AgTile _tile)
	{
		IntPtr set = AutoGeometryWrapper.agCachedTileGetPolylineSet(_tile.tilePtr);
		int num = AutoGeometryWrapper.agPolylineSetGetLineCount(set);
		AutoGeometryWrapper.agPolylineSetGetLines(set, m_tempPolylines);
		_tile.edgeVerts = null;
		if (num <= 0)
		{
			return;
		}
		List<AgTileEdgeVert> list = new List<AgTileEdgeVert>();
		for (int i = 0; i < num; i++)
		{
			if (!AutoGeometryWrapper.agPolylineIsLooped(m_tempPolylines[i]))
			{
				int num2 = AutoGeometryWrapper.agPolylineGetVertCount(m_tempPolylines[i]);
				if (num2 > 1)
				{
					AutoGeometryWrapper.agPolyLineGetVertices(m_tempPolylines[i], m_tempVertices);
					Vector2 vector = m_tempVertices[0] - m_tempVertices[1];
					Vector2 vector2 = m_tempVertices[num2 - 2] - m_tempVertices[num2 - 1];
					vector = new Vector2(vector.y, 0f - vector.x).normalized;
					vector2 = new Vector2(vector2.y, 0f - vector2.x).normalized;
					AgTileEdgeVert item = new AgTileEdgeVert(AutoGeometryManager.m_tileCacheOffset + m_tempVertices[0], vector);
					AgTileEdgeVert item2 = new AgTileEdgeVert(AutoGeometryManager.m_tileCacheOffset + m_tempVertices[num2 - 1], vector2);
					item.polyline = m_tempPolylines[i];
					list.Add(item);
					list.Add(item2);
				}
			}
		}
		Vector2 pos = AutoGeometryManager.m_tileCacheOffset + new Vector2(_tile.bb.l, _tile.bb.b);
		Vector2 pos2 = AutoGeometryManager.m_tileCacheOffset + new Vector2(_tile.bb.r, _tile.bb.b);
		Vector2 pos3 = AutoGeometryManager.m_tileCacheOffset + new Vector2(_tile.bb.r, _tile.bb.t);
		Vector2 pos4 = AutoGeometryManager.m_tileCacheOffset + new Vector2(_tile.bb.l, _tile.bb.t);
		list.Add(new AgTileEdgeVert(pos, Vector2.zero, true));
		list.Add(new AgTileEdgeVert(pos2, Vector2.zero, true));
		list.Add(new AgTileEdgeVert(pos3, Vector2.zero, true));
		list.Add(new AgTileEdgeVert(pos4, Vector2.zero, true));
		_tile.edgeVerts = list.ToArray();
	}

	private int findNearestEdgeVertex(AgTile _tile, Vector2 _tileCenter, int _index)
	{
		int result = -1;
		float num = 99999f;
		for (int i = 0; i < _tile.edgeVerts.Length; i++)
		{
			if (i != _index)
			{
				float sqrMagnitude = (_tile.edgeVerts[i].pos - _tile.edgeVerts[_index].pos).sqrMagnitude;
				if (sqrMagnitude < num && ToolBox.Sign(_tile.edgeVerts[_index].pos, _tile.edgeVerts[i].pos, _tileCenter) < 0f)
				{
					num = sqrMagnitude;
					result = i;
				}
			}
		}
		return result;
	}

	private void GenerateTileGeometry(AgTile _tile)
	{
		AgPolygon agPolygon = new AgPolygon();
		IntPtr set = AutoGeometryWrapper.agCachedTileGetPolylineSet(_tile.tilePtr);
		int num = AutoGeometryWrapper.agPolylineSetGetLineCount(set);
		AutoGeometryWrapper.agPolylineSetGetLines(set, m_tempPolylines);
		Vector2 pos = _tile.pos;
		List<Mesh> list = new List<Mesh>();
		m_tempAgPolys.Clear();
		bool flag = true;
		if (_tile.edgeVerts.Length > 4)
		{
			int num2 = 0;
			while (num2 >= 0)
			{
				Vector2 pos2 = _tile.edgeVerts[num2].pos;
				AgPolygon agPolygon2 = new AgPolygon();
				for (int i = 0; i < _tile.edgeVerts.Length; i++)
				{
					IntPtr polyline = _tile.edgeVerts[num2].polyline;
					if (polyline != (IntPtr)0)
					{
						if (!_tile.edgeVerts[num2].wasTravelled)
						{
							int num3 = AutoGeometryWrapper.agPolylineGetVertCount(polyline);
							agPolygon.Clear();
							AutoGeometryWrapper.agPolyLineGetVertices(polyline, m_tempVertices);
							for (int j = 0; j < num3; j++)
							{
								Vector2 vector = AutoGeometryManager.m_tileCacheOffset + m_tempVertices[j];
								Vector2 vector2;
								if (j == 0 || j == num3 - 1)
								{
									vector2 = GetFixedEdgeVertNormalForPoint(_tile, vector);
								}
								else
								{
									Vector2 vector3 = m_tempVertices[j - 1] - m_tempVertices[j];
									Vector2 vector4 = m_tempVertices[j] - m_tempVertices[j + 1];
									vector3 = new Vector2(vector3.y, 0f - vector3.x).normalized;
									vector4 = new Vector2(vector4.y, 0f - vector4.x).normalized;
									vector2 = (vector3 + vector4) / 2f;
								}
								agPolygon2.vertices.Add(vector);
								agPolygon2.extraData.Add(vector2);
								agPolygon.vertices.Add(vector);
								agPolygon.extraData.Add(vector2);
							}
							if (_tile.regenerateCollisionShapes)
							{
								GenerateCollisionShapesFromPolyLine(_tile, ref m_tempVertices, num3, _tile.edgeVerts[num2].cTangent, _tile.edgeVerts[num2 + 1].cTangent);
							}
							if (agPolygon.vertices.Count > 0)
							{
								list.Add(AutogeometryVisuals.CreateBeltMeshFromVertexArray(agPolygon, m_groundC.m_ground.m_depth, m_tileOffset, false, pos, m_groundC.m_ground.m_smoothingAngle));
							}
							_tile.edgeVerts[num2].wasTravelled = true;
							num2++;
						}
					}
					else
					{
						agPolygon2.vertices.Add(_tile.edgeVerts[num2].pos);
						agPolygon2.extraData.Add(new Vector2(0f, 0f));
					}
					num2 = findNearestEdgeVertex(_tile, pos, num2);
					if (num2 < 0 || _tile.edgeVerts[num2].pos == pos2)
					{
						break;
					}
				}
				flag = false;
				m_tempAgPolys.AddItem(agPolygon2);
				num2 = -1;
				for (int k = 0; k < _tile.edgeVerts.Length; k++)
				{
                    if (_tile.edgeVerts[k].polyline != (IntPtr)0 && !_tile.edgeVerts[k].wasTravelled)
                    {
						num2 = k;
						break;
					}
				}
			}
			for (int l = 0; l < _tile.edgeVerts.Length; l++)
			{
				_tile.edgeVerts[l].wasTravelled = false;
			}
		}
		bool flag2 = true;
		bool flag3 = false;
		int num4 = 0;
		for (int m = 0; m < num; m++)
		{
			IntPtr polyline2 = m_tempPolylines[m];
			if (!AutoGeometryWrapper.agPolylineIsLooped(polyline2))
			{
				continue;
			}
			AgPolygon agPolygon3 = new AgPolygon();
			int num5 = AutoGeometryWrapper.agPolylineGetVertCount(polyline2);
			AutoGeometryWrapper.agPolyLineGetVertices(polyline2, m_tempVertices);
			float num6 = 0f;
			for (int n = 0; n < num5 - 1; n++)
			{
				num6 += (m_tempVertices[n + 1].x - m_tempVertices[n].x) * (m_tempVertices[n + 1].y + m_tempVertices[n].y);
			}
			agPolygon3.isHole = num6 < 0f;
			if (num4 == 0 && agPolygon3.isHole)
			{
				flag3 = true;
			}
			if (Mathf.Abs(num6) > 0.01f)
			{
				if (_tile.regenerateCollisionShapes)
				{
					GenerateCollisionShapesFromPolyLine(_tile, ref m_tempVertices, num5, Vector2.zero, Vector2.zero);
				}
				for (int num7 = 0; num7 < num5; num7++)
				{
					agPolygon3.vertices.Add(AutoGeometryManager.m_tileCacheOffset + m_tempVertices[num7]);
					int rolledValue = ToolBox.getRolledValue(num7 - 1, 0, num5 - 1);
					int rolledValue2 = ToolBox.getRolledValue(num7 + 1, 0, num5 - 1);
					Vector2 vector5 = m_tempVertices[rolledValue] - m_tempVertices[num7];
					Vector2 vector6 = m_tempVertices[num7] - m_tempVertices[rolledValue2];
					vector5 = new Vector2(vector5.y, 0f - vector5.x).normalized;
					vector6 = new Vector2(vector6.y, 0f - vector6.x).normalized;
					Vector2 vector7 = vector5 + vector6 / 2f;
					agPolygon3.extraData.Add(vector7);
				}
				if (agPolygon3.vertices.Count > 0)
				{
					list.Add(AutogeometryVisuals.CreateBeltMeshFromVertexArray(agPolygon3, m_groundC.m_ground.m_depth, m_tileOffset, true, pos, m_groundC.m_ground.m_smoothingAngle));
				}
				if (!agPolygon3.isHole)
				{
					flag2 = false;
				}
				m_tempAgPolys.AddItem(agPolygon3);
				num4++;
			}
		}
		PrefabS.RemoveComponentsByEntity(_tile.TC.p_entity);
		AgPolygon _poly = new AgPolygon();
		if (flag && num4 > 0)
		{
			if (flag2 || flag3)
			{
				addSquarePoly(ref _poly, _tile);
				m_tempAgPolys.AddItem(_poly);
			}
		}
		else if (m_tempAgPolys.m_aliveCount == 0 && IsTileFilled(_tile.bb))
		{
			addSquarePoly(ref _poly, _tile);
			m_tempAgPolys.AddItem(_poly);
		}
		if (m_tempAgPolys.m_aliveCount > 0)
		{
			PrefabS.CreatePrefabFromMesh(_tile.TC, AutogeometryVisuals.CreateFrontFaceMeshFromPolygon(m_tempAgPolys.ToArray(), m_tileOffset, pos), 9, m_frontMaterial, true, true);
		}
		if (list.Count > 0)
		{
			PrefabC prefabC = PrefabS.CreatePrefabFromMeshArray(_tile.TC, list.ToArray(), 9, m_beltMaterial, true);
			prefabC.p_gameObject.renderer.castShadows = false;
			prefabC.p_gameObject.renderer.receiveShadows = false;
		}
		_tile.regenerateCollisionShapes = false;
	}

	private void addSquarePoly(ref AgPolygon _poly, AgTile _tile)
	{
		_poly.vertices = new List<Vector2>();
		int num = _tile.edgeVerts.Length - 4;
		_poly.vertices.Add(_tile.edgeVerts[num + 3].pos);
		_poly.vertices.Add(_tile.edgeVerts[num + 2].pos);
		_poly.vertices.Add(_tile.edgeVerts[num + 1].pos);
		_poly.vertices.Add(_tile.edgeVerts[num].pos);
	}

	private AgTile GetAgTile(IntPtr _tilePtr, bool _setAsDirty)
	{
		AgTile agTile = m_tileHash[_tilePtr] as AgTile;
		if (agTile != null && _setAsDirty)
		{
			agTile.dirty = true;
		}
		return agTile;
	}

	private int GetTilePositionalHashKey(Vector2 _tileCenter)
	{
		int num = Mathf.CeilToInt(_tileCenter.x / (float)m_tileSize);
		int num2 = Mathf.CeilToInt(_tileCenter.y / (float)m_tileSize);
		return num2 * m_xTiles + num;
	}

	private AgTile GetTileNeighbour(AgTile _tile, int _x, int _y)
	{
		int num = Mathf.CeilToInt(_tile.pos.x / (float)m_tileSize) + _x;
		int num2 = Mathf.CeilToInt(_tile.pos.y / (float)m_tileSize) + _y;
		int num3 = num2 * m_xTiles + num;
		return m_tilePositionalHash[num3] as AgTile;
	}

	private AgTile CreateAgTile(IntPtr _tilePtr)
	{
		cpBB bb = AutoGeometryWrapper.agCachedTileGetBB(_tilePtr);
		Vector2 vector = AutoGeometryManager.m_tileCacheOffset + new Vector2(bb.l - (bb.l - bb.r) * 0.5f, bb.t - (bb.t - bb.b) * 0.5f);
		AgTile agTile = new AgTile(20);
		agTile.TC = EntityManager.AddEntityWithTC();
		agTile.TC.transform.position = vector;
		agTile.bb = bb;
		agTile.pos = vector;
		agTile.dirty = true;
		agTile.tilePtr = _tilePtr;
		m_tileHash.Add(_tilePtr, agTile);
		m_tilePositionalHash.Add(GetTilePositionalHashKey(agTile.pos), agTile);
		return agTile;
	}

	public void ClearAgTileDirtyFlags()
	{
		foreach (AgTile value in m_tileHash.Values)
		{
			value.dirty = false;
		}
	}

	private void RemoveAgTile(IntPtr _tilePtr)
	{
		AgTile agTile = m_tileHash[_tilePtr] as AgTile;
		if (agTile != null && !agTile.dirty)
		{
			for (int i = 0; i < agTile.shapeCount; i++)
			{
				ChipmunkProWrapper.ucpRemoveShape(agTile.shapes[i]);
			}
			if (agTile.TC.p_entity != null)
			{
				EntityManager.RemoveEntity(agTile.TC.p_entity);
			}
			agTile.edgeVerts = null;
			m_tileHash.Remove(_tilePtr);
			m_tilePositionalHash.Remove(GetTilePositionalHashKey(agTile.pos));
		}
	}

	private void EnsureRect()
	{
		if (m_updateEnsureRect)
		{
			m_updateEnsureRect = false;
			AutoGeometryWrapper.agBasicTileCacheEnsureRect(m_tileCache, m_ensureRect);
		}
	}

	private void ClearTileCollisionShapes(AgTile _tile)
	{
		for (int i = 0; i < _tile.shapeCount; i++)
		{
			ChipmunkProWrapper.ucpRemoveShape(_tile.shapes[i]);
		}
		_tile.shapeCount = 0;
		_tile.regenerateCollisionShapes = true;
	}

	private void GenerateCollisionShapesFromPolyLine(AgTile _tile, ref Vector2[] _vertices, int _vertCount, Vector2 _prevTangent, Vector2 _nextTangent)
	{
		IntPtr body = m_groundBody.body;
		if (_vertCount <= 0)
		{
			return;
		}
		for (int i = 0; i < _vertCount - 1; i++)
		{
			Vector2 vector = AutoGeometryManager.m_tileCacheOffset + _vertices[i];
			Vector2 vector2 = AutoGeometryManager.m_tileCacheOffset + _vertices[i + 1];
			if (_tile.shapeCount >= _tile.shapes.Length)
			{
				Array.Resize(ref _tile.shapes, _tile.shapeCount * 2);
			}
			IntPtr intPtr = ChipmunkProWrapper.ucpSegmentShapeNew(body, vector, vector2, 6f, (ucpCollisionType)2);
			ChipmunkProWrapper.ucpShapeSetElasticity(intPtr, m_groundC.m_ground.m_elasticity);
			ChipmunkProWrapper.ucpShapeSetFriction(intPtr, m_groundC.m_ground.m_friction);
			Vector2 prev = vector;
			Vector2 next = vector2;
			if (i > 0 && i < _vertCount - 2)
			{
				if (i > 0)
				{
					prev = AutoGeometryManager.m_tileCacheOffset + _vertices[i - 1];
				}
				if (i < _vertCount - 2)
				{
					next = AutoGeometryManager.m_tileCacheOffset + _vertices[i + 2];
				}
				ChipmunkProWrapper.ucpSegmentShapeSetNeighbors(intPtr, prev, next);
			}
			else if (i == _vertCount - 2)
			{
				ChipmunkProWrapper.ucpSegmentShapeSetNextNeighborTangent(intPtr, _nextTangent);
			}
			else if (i == 0)
			{
				ChipmunkProWrapper.ucpSegmentShapeSetPrevNeighborTangent(intPtr, -_prevTangent);
			}
			ChipmunkProWrapper.ucpSpaceAddStaticShape(intPtr);
			_tile.shapes[_tile.shapeCount] = intPtr;
			_tile.shapeCount++;
		}
	}

	private void FixNormalsWithNeighbours(AgTile _tile)
	{
		if (_tile.edgeVerts != null && _tile.edgeVerts.Length > 4)
		{
			FixEdgeVertNormalsWithNeighbour(_tile, 1, 0);
			FixEdgeVertNormalsWithNeighbour(_tile, 0, 1);
			FixEdgeVertNormalsWithNeighbour(_tile, -1, 0);
			FixEdgeVertNormalsWithNeighbour(_tile, 0, -1);
		}
	}

	public void UpdateSegments()
	{
		if (!m_dirty)
		{
			return;
		}
		m_dirty = false;
		EnsureRect();
		int num = AutoGeometryWrapper.agBasicTileCacheGetDirtyTileCount(m_tileCache);
		if (num == 0)
		{
			return;
		}
		AutoGeometryWrapper.agBasicTileCacheGetDirtyTileList(m_tileCache, m_dirtyTileList);
		m_geometryUpdateQueue.Clear();
		ChipmunkProWrapper.ucpClearCollisionLists();
		for (int i = 0; i < num; i++)
		{
			IntPtr intPtr = m_dirtyTileList[i];
			AgTile agTile = GetAgTile(intPtr, true);
			if (agTile != null)
			{
				ClearTileCollisionShapes(agTile);
			}
			IntPtr set = AutoGeometryWrapper.agCachedTileGetPolylineSet(intPtr);
			int num2 = AutoGeometryWrapper.agPolylineSetGetLineCount(set);
			if (num2 > 0)
			{
				if (agTile == null)
				{
					agTile = CreateAgTile(intPtr);
				}
				GenerateEdgeVertArray(agTile);
				m_geometryUpdateQueue.Add(agTile);
				continue;
			}
			cpBB tileBB = AutoGeometryWrapper.agCachedTileGetBB(intPtr);
			bool flag = IsTileFilled(tileBB);
			if (agTile != null || flag)
			{
				if (agTile == null)
				{
					agTile = CreateAgTile(intPtr);
				}
				PrefabS.RemoveComponentsByEntity(agTile.TC.p_entity);
				agTile.edgeVerts = null;
				if (flag)
				{
					Vector2[] array = new Vector2[4];
					array[3] = AutoGeometryManager.m_tileCacheOffset + new Vector2(tileBB.l, tileBB.b);
					array[2] = AutoGeometryManager.m_tileCacheOffset + new Vector2(tileBB.r, tileBB.b);
					array[1] = AutoGeometryManager.m_tileCacheOffset + new Vector2(tileBB.r, tileBB.t);
					array[0] = AutoGeometryManager.m_tileCacheOffset + new Vector2(tileBB.l, tileBB.t);
					AgPolygon[] array2 = new AgPolygon[1]
					{
						new AgPolygon()
					};
					array2[0].vertices.Add(array[0]);
					array2[0].vertices.Add(array[1]);
					array2[0].vertices.Add(array[2]);
					array2[0].vertices.Add(array[3]);
					PrefabS.CreatePrefabFromMesh(agTile.TC, AutogeometryVisuals.CreateFrontFaceMeshFromPolygon(array2, m_tileOffset, agTile.pos), 9, m_frontMaterial, true, true);
				}
				else
				{
					RemoveAgTile(intPtr);
				}
			}
		}
		if (m_geometryUpdateQueue.Count > 0)
		{
			AgTile[] array3 = m_geometryUpdateQueue.ToArray();
			for (int j = 0; j < array3.Length; j++)
			{
				FixNormalsWithNeighbours(array3[j]);
			}
		}
		foreach (AgTile item in m_geometryUpdateQueue)
		{
			GenerateTileGeometry(item);
		}
		ChipmunkProS.HandleCollisionEvents();
	}

	public void Update()
	{
		if (m_highlightTween != null || m_highlightGlowTween != null)
		{
			float num = 0f;
			num = ((m_highlightTween == null) ? m_highlightGlowTween.currentValue.x : m_highlightTween.currentValue.x);
			Color color = new Color(num, num, num, 0f);
			m_frontMaterial.SetColor("_Emission", color);
			m_beltMaterial.SetColor("_Emission", color);
			if (m_highlightTween != null && m_highlightTween.hasFinished)
			{
				TweenS.RemoveComponent(m_highlightTween);
				m_highlightTween = null;
			}
		}
		if (m_maskTextureModified)
		{
			m_maskTexture.Apply();
			m_maskTextureModified = false;
		}
	}

	public void Destroy()
	{
		Debug.Log("Removing material: " + m_groundC.m_ground.m_name);
		EntityManager.RemoveEntity(m_groundBodyEntity);
		foreach (AgTile value in m_tileHash.Values)
		{
			for (int i = 0; i < value.shapeCount; i++)
			{
				ChipmunkProWrapper.ucpRemoveShape(value.shapes[i]);
			}
			value.shapeCount = 0;
			value.shapes = null;
			if (value.TC.p_entity != null)
			{
				EntityManager.RemoveEntity(value.TC.p_entity);
			}
			value.edgeVerts = null;
		}
		m_tileHash.Clear();
		m_tilePositionalHash.Clear();
		m_geometryUpdateQueue.Clear();
		m_tileHash = null;
		m_tilePositionalHash = null;
		m_geometryUpdateQueue = null;
		m_tempPolylines = null;
		m_tempVertices = null;
		m_bytes = null;
		m_snapshotBytes = null;
		m_maxValueLookupBytes = null;
		UnityEngine.Object.DestroyImmediate(m_maskTexture);
		m_maskTexture = null;
		AutoGeometryWrapper.agSimpleSamplerFree(m_sampler);
		AutoGeometryWrapper.agBasicTileCacheFree(m_tileCache);
	}
}
