using System;
using System.Collections.Generic;
using UnityEngine;

public static class TouchAreaS
{
    // Internal touch representation to unify hardware touches & mouse input
    private struct CustomTouch
    {
        public int fingerId;
        public Vector2 position;
        public Vector2 deltaPosition;
        public TouchPhase phase;
        public int tapCount;
    }

    public static DynamicArray<TLTouch> m_touches;

    private static List<TLTouch> m_touchRemoveList;

    private static DynamicArray<TouchAreaC> m_areas;

    public static bool m_abort;

    private static Vector2 m_prevMousePosition;

    private static bool m_mouseActive;

    private static List<CustomTouch> m_activeTouches;

    private static Touch2[] m_mouseTouches;

    private static TransformC m_colliderHelperTC;

    private static bool m_colliderMeshesCreated;

    private static Mesh m_rectMesh;

    private static Mesh m_circleMesh;

    private static Camera m_touchCameraFilter;

    private static TEvent[] m_touchEvents;

    private static int m_touchEventCount;

    public static void Initialize()
    {
        Input.simulateMouseWithTouches = false;
        m_touches = new DynamicArray<TLTouch>(10);
        m_touchRemoveList = new List<TLTouch>();
        m_areas = new DynamicArray<TouchAreaC>();
        m_activeTouches = new List<CustomTouch>(10);
        m_mouseActive = false;
        m_mouseTouches = new Touch2[10];
        for (int i = 0; i < 10; i++)
        {
            m_mouseTouches[i] = new Touch2();
        }
        m_touchEvents = new TEvent[50];
        for (int j = 0; j < m_touchEvents.Length; j++)
        {
            m_touchEvents[j] = new TEvent();
            m_touchEvents[j].m_touches = new TLTouch[10];
            m_touchEvents[j].m_touchHots = new bool[10];
            m_touchEvents[j].m_touchPhases = new TouchAreaPhase[10];
            m_touchEvents[j].touchCount = 0;
        }
    }

    private static void CreateColliderMeshes()
    {
        Entity entity = EntityManager.AddEntity();
        entity.m_persistent = true;
        m_colliderHelperTC = TransformS.AddComponent(entity, "Touch Area Helper Transform");
        Vector2[] circle = DebugDraw.GetCircle(1f, 8, Vector2.zero);
        m_circleMesh = PrefabS.CreateMeshFromVector2Array(circle);
        Vector2[] rect = DebugDraw.GetRect(1f, 1f, Vector2.zero);
        m_rectMesh = PrefabS.CreateMeshFromVector2Array(rect);
        m_colliderMeshesCreated = true;
    }

    private static Mesh GetCircleMesh(float _radius)
    {
        if (!m_colliderMeshesCreated)
        {
            CreateColliderMeshes();
        }
        Mesh mesh = UnityEngine.Object.Instantiate(m_circleMesh) as Mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= _radius;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Mesh GetRectMesh(float _width, float _height)
    {
        if (!m_colliderMeshesCreated)
        {
            CreateColliderMeshes();
        }
        Mesh mesh = UnityEngine.Object.Instantiate(m_rectMesh) as Mesh;
        Vector3 scale = new Vector3(_width, _height, 1f);
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Scale(scale);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static void ResizeRectCollider(TouchAreaC _c, float _width, float _height)
    {
        if (_c.m_colliderShape != ColliderShape.Rect)
        {
            Debug.LogWarning("Trying to resize non-rect collider");
            return;
        }
        _c.m_collider.sharedMesh = null;
        _c.m_collider.sharedMesh = GetRectMesh(_width, _height);
    }

    public static void ResizeCircleCollider(TouchAreaC _c, float _radius)
    {
        if (_c.m_colliderShape != ColliderShape.Circle)
        {
            Debug.LogWarning("Trying to resize non-circle collider");
            return;
        }
        _c.m_collider.sharedMesh = null;
        _c.m_collider.sharedMesh = GetCircleMesh(_radius);
    }

    public static void AddTouchEventListener(TouchAreaC _c, TouchEventDelegate _touchEventHandler)
    {
        if (_c.m_delegatedCount == 0)
        {
            _c.d_TouchEventDelegate = _touchEventHandler;
        }
        else
        {
            _c.d_TouchEventDelegate = (TouchEventDelegate)Delegate.Combine(_c.d_TouchEventDelegate, _touchEventHandler);
        }
        _c.m_delegatedCount++;
    }

    public static void RemoveTouchEventListener(TouchAreaC _c, TouchEventDelegate _touchEventHandler)
    {
        if (_c.m_delegatedCount > 0)
        {
            _c.d_TouchEventDelegate = (TouchEventDelegate)Delegate.Remove(_c.d_TouchEventDelegate, _touchEventHandler);
            _c.m_delegatedCount--;
        }
    }

    public static void RemoveAllTouchEventListeners(TouchAreaC _c)
    {
        if (_c.m_delegatedCount > 0)
        {
            Delegate[] invocationList = _c.d_TouchEventDelegate.GetInvocationList();
            Delegate[] array = invocationList;
            foreach (Delegate obj in array)
            {
                _c.d_TouchEventDelegate = (TouchEventDelegate)Delegate.Remove(_c.d_TouchEventDelegate, (TouchEventDelegate)obj);
            }
            _c.m_delegatedCount = 0;
        }
    }

    private static TLTouch AddTouch(Vector2 _pos, int _fingerId, TouchPhase _phase)
    {
        TLTouch tLTouch = m_touches.AddItem();
        tLTouch.m_currentPosition = _pos;
        tLTouch.m_startPosition = _pos;
        tLTouch.m_fingerId = _fingerId;
        tLTouch.m_phase = _phase;
        return tLTouch;
    }

    private static void RemoveTouch(TLTouch _t)
    {
        _t.m_primaryArea = null;
        _t.m_secondaryArea = null;
        _t.m_consumed = false;
        m_touches.RemoveItem(_t);
    }

    public static TouchAreaC AddRectArea(TransformC _tc, string _name, float _width, float _height, Camera _camera, bool _consume = true, IComponent _customComponent = null)
    {
        TouchAreaC touchAreaC = m_areas.AddItem();
        touchAreaC.m_TC = _tc;
        touchAreaC.m_TC.transform.gameObject.layer = _camera.gameObject.layer;
        touchAreaC.m_consume = _consume;
        touchAreaC.m_customComponent = _customComponent;
        touchAreaC.m_camera = _camera;
        touchAreaC.m_name = _name;
        GameObject gameObject = touchAreaC.m_TC.transform.gameObject;
        MeshCollider meshCollider = gameObject.AddComponent("MeshCollider") as MeshCollider;
        meshCollider.sharedMesh = GetRectMesh(_width, _height);
        touchAreaC.m_collider = meshCollider;
        touchAreaC.m_colliderShape = ColliderShape.Rect;
        TouchAreaBootstrap touchAreaBootstrap = gameObject.AddComponent("TouchAreaBootstrap") as TouchAreaBootstrap;
        touchAreaBootstrap.m_TAC = touchAreaC;
        EntityManager.AddComponentToEntity(_tc.p_entity, touchAreaC);
        return touchAreaC;
    }

    public static TouchAreaC AddCircleArea(TransformC _tc, string _name, float _radius, Camera _camera, bool _consume = true, IComponent _customComponent = null)
    {
        TouchAreaC touchAreaC = m_areas.AddItem();
        touchAreaC.m_TC = _tc;
        touchAreaC.m_TC.transform.gameObject.layer = _camera.gameObject.layer;
        touchAreaC.m_consume = _consume;
        touchAreaC.m_customComponent = _customComponent;
        touchAreaC.m_camera = _camera;
        touchAreaC.m_name = _name;
        GameObject gameObject = touchAreaC.m_TC.transform.gameObject;
        MeshCollider meshCollider = gameObject.AddComponent("MeshCollider") as MeshCollider;
        meshCollider.sharedMesh = GetCircleMesh(_radius);
        touchAreaC.m_collider = meshCollider;
        touchAreaC.m_colliderShape = ColliderShape.Circle;
        TouchAreaBootstrap touchAreaBootstrap = gameObject.AddComponent("TouchAreaBootstrap") as TouchAreaBootstrap;
        touchAreaBootstrap.m_TAC = touchAreaC;
        EntityManager.AddComponentToEntity(_tc.p_entity, touchAreaC);
        return touchAreaC;
    }

    public static TouchAreaC AddMeshArea(TransformC _tc, string _name, Mesh _mesh, Camera _camera, bool _consume = true, IComponent _customComponent = null)
    {
        if (!m_colliderMeshesCreated)
        {
            CreateColliderMeshes();
        }
        TouchAreaC touchAreaC = m_areas.AddItem();
        touchAreaC.m_TC = _tc;
        touchAreaC.m_TC.transform.gameObject.layer = _camera.gameObject.layer;
        touchAreaC.m_consume = _consume;
        touchAreaC.m_customComponent = _customComponent;
        touchAreaC.m_camera = _camera;
        touchAreaC.m_name = _name;
        GameObject gameObject = touchAreaC.m_TC.transform.gameObject;
        MeshCollider meshCollider = gameObject.AddComponent("MeshCollider") as MeshCollider;
        meshCollider.sharedMesh = _mesh;
        touchAreaC.m_collider = meshCollider;
        touchAreaC.m_colliderShape = ColliderShape.Mesh;
        TouchAreaBootstrap touchAreaBootstrap = gameObject.AddComponent("TouchAreaBootstrap") as TouchAreaBootstrap;
        touchAreaBootstrap.m_TAC = touchAreaC;
        EntityManager.AddComponentToEntity(_tc.p_entity, touchAreaC);
        return touchAreaC;
    }

    public static void RemoveArea(TouchAreaC _c)
    {
        GameObject gameObject = _c.m_TC.transform.gameObject;
        TouchAreaBootstrap component = gameObject.GetComponent<TouchAreaBootstrap>();
        MeshCollider component2 = gameObject.GetComponent<MeshCollider>();
        UnityEngine.Object.DestroyImmediate(component);
        UnityEngine.Object.DestroyImmediate(component2);
        if (_c.m_delegatedCount > 0)
        {
            Delegate[] invocationList = _c.d_TouchEventDelegate.GetInvocationList();
            Delegate[] array = invocationList;
            foreach (Delegate obj in array)
            {
                _c.d_TouchEventDelegate = (TouchEventDelegate)Delegate.Remove(_c.d_TouchEventDelegate, (TouchEventDelegate)obj);
            }
            _c.m_delegatedCount = 0;
        }
        EntityManager.RemoveComponentFromEntity(_c);
        m_areas.RemoveItem(_c);
    }

    public static void SetCamera(TouchAreaC _c, Camera _camera)
    {
        _c.m_camera = _camera;
        _c.m_TC.transform.gameObject.layer = _camera.gameObject.layer;
    }

    public static void SetTouchCameraFilter(Camera _camera)
    {
        m_touchCameraFilter = _camera;
    }

    public static void SetClip(TouchAreaC _c, float _left, float _right, float _bottom, float _top)
    {
        _c.m_clip = true;
        _c.m_clipBB.r = _right;
        _c.m_clipBB.l = _left;
        _c.m_clipBB.t = _top;
        _c.m_clipBB.b = _bottom;
    }

    public static void LockSecondaryTouchArea(TLTouch _t)
    {
        _t.m_secondaryLocked = true;
    }

    public static void UnlockSecondaryTouchArea(TLTouch _t)
    {
        _t.m_secondaryLocked = false;
    }

    public static bool IsTouchInside(Vector2 touchPos, Vector2 position, float radius)
    {
        Vector2 vector = touchPos - position;
        float num = vector.x * vector.x + vector.y * vector.y;
        if (num < radius * radius)
        {
            return true;
        }
        return false;
    }

    public static bool IsTouchInside(Vector2 touchPos, Vector2 position, Vector2 dimensions, float rotAngle)
    {
        float num = touchPos.x;
        float num2 = touchPos.y;
        if (rotAngle != 0f)
        {
            Vector2 vec = new Vector2(num, num2) - position;
            float num3 = ToolBox.getAngleFromVector2(vec) * 57.29578f;
            float num4 = rotAngle - num3;
            num = position.x + Mathf.Cos(num4 * ((float)Math.PI / 180f)) * vec.magnitude;
            num2 = position.y + Mathf.Sin(num4 * ((float)Math.PI / 180f)) * vec.magnitude;
        }
        position.x -= dimensions.x * 0.5f;
        position.y -= dimensions.y * 0.5f;
        if (num > position.x && num < position.x + dimensions.x && num2 > position.y && num2 < position.y + dimensions.y)
        {
            return true;
        }
        return false;
    }

    public static Vector3 GetTouchWorldPos(Camera _camera, Vector2 _screenPos, float _zOffset)
    {
        Vector3 result = -_camera.ScreenToWorldPoint(new Vector3(_screenPos.x, _screenPos.y, _camera.transform.position.z + _zOffset));
        result += _camera.transform.position * 2f;
        result.z = 0f;
        return result;
    }

    public static Vector3 GetTouchWorldPos(Camera _camera, Vector2 _screenPos)
    {
        return GetTouchWorldPos(_camera, _screenPos, 0f);
    }

    public static void Update()
    {
        while (m_touchRemoveList.Count > 0)
        {
            int index = m_touchRemoveList.Count - 1;
            RemoveTouch(m_touchRemoveList[index]);
            m_touchRemoveList.RemoveAt(index);
        }
        m_touches.Update();

        // Gather both hardware touches and mouse emulation into a single unified list
        m_activeTouches.Clear();

        // 1. Hardware Touches (iOS/Android/Touchscreens)
        int realTouchCount = Input.touchCount;
        for (int i = 0; i < realTouchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            CustomTouch ct;
            ct.fingerId = t.fingerId;
            ct.position = t.position;
            ct.deltaPosition = t.deltaPosition;
            ct.phase = t.phase;
            ct.tapCount = t.tapCount;
            m_activeTouches.Add(ct);
        }

        // 2. Mouse Emulation (Unity Editor / PC / Mac)
        bool mouseAllowed = realTouchCount == 0;

        if (mouseAllowed && Input.GetMouseButtonDown(0))
        {
            m_mouseActive = true;
            CustomTouch ct = default(CustomTouch);
            ct.fingerId = 99; // Unique ID to act as a virtual finger
            ct.position = (Vector2)Input.mousePosition;
            ct.deltaPosition = Vector2.zero;
            ct.phase = TouchPhase.Began;
            ct.tapCount = 1;
            m_prevMousePosition = ct.position;
            m_activeTouches.Add(ct);
        }
        else if (m_mouseActive)
        {
            Vector2 curPos = (Vector2)Input.mousePosition;
            Vector2 delta = curPos - m_prevMousePosition;
            m_prevMousePosition = curPos;

            CustomTouch ct = default(CustomTouch);
            ct.fingerId = 99;
            ct.position = curPos;
            ct.deltaPosition = delta;
            ct.tapCount = 1;

            if (Input.GetMouseButtonUp(0))
            {
                m_mouseActive = false;
                ct.phase = TouchPhase.Ended;
                m_activeTouches.Add(ct);
            }
            else if (Input.GetMouseButton(0))
            {
                ct.phase = (delta.sqrMagnitude > 0.0001f) ? TouchPhase.Moved : TouchPhase.Stationary;
                m_activeTouches.Add(ct);
            }
            else
            {
                m_mouseActive = false;
                ct.phase = TouchPhase.Ended;
                m_activeTouches.Add(ct);
            }
        }

        // Process all touches (hardware + mouse)
        int touchCount = m_activeTouches.Count;
        for (int i = 0; i < touchCount; i++)
        {
            TLTouch tLTouch = null;
            bool flag = true;
            CustomTouch touch = m_activeTouches[i];
            int aliveCount = m_touches.m_aliveCount;
            for (int j = 0; j < aliveCount; j++)
            {
                tLTouch = m_touches.m_array[m_touches.m_aliveIndices[j]];
                if (tLTouch.m_fingerId == touch.fingerId)
                {
                    flag = false;
                    tLTouch.m_currentPosition = touch.position;
                    tLTouch.m_deltaPosition = touch.deltaPosition;
                    tLTouch.m_phase = touch.phase;
                    tLTouch.m_tapCount = touch.tapCount;
                    break;
                }
            }
            if (flag)
            {
                tLTouch = AddTouch(touch.position, touch.fingerId, TouchPhase.Began);
            }
            int count = CameraS.m_cameras.Count;
            for (int num = count - 1; num > -1; num--)
            {
                Camera camera = CameraS.m_cameras[num];
                if ((!(m_touchCameraFilter != null) || !(camera != m_touchCameraFilter)) && (tLTouch.m_primaryArea == null || !(tLTouch.m_primaryArea.m_camera != camera)))
                {
                    Vector3 touchWorldPos = GetTouchWorldPos(camera, touch.position);
                    Vector3 position = camera.transform.position;
                    Vector3 direction = touchWorldPos - position;
                    RaycastHit[] array = null;
                    if (camera.isOrthoGraphic)
                    {
                        Vector3 vector = -touchWorldPos + position * 2f;
                        vector.z = position.z;
                        array = Physics.RaycastAll(vector, Vector3.forward, Math.Abs(position.z) + 500f, 1 << camera.gameObject.layer);
                        UnityEngine.Debug.DrawRay(vector, Vector3.forward * (Math.Abs(position.z) + 500f), Color.red);
                    }
                    else
                    {
                        array = Physics.RaycastAll(position, direction, Math.Abs(position.z) + 500f, 1 << camera.gameObject.layer);
                        UnityEngine.Debug.DrawRay(position, direction.normalized * (Math.Abs(position.z) + 500f), Color.yellow);
                    }
                    TouchAreaC touchAreaC = null;
                    float num2 = 99999f;
                    TouchAreaC touchAreaC2 = null;
                    float num3 = 99999f;
                    bool flag2 = false;
                    for (int k = 0; k < array.Length; k++)
                    {
                        RaycastHit raycastHit = array[k];
                        TouchAreaBootstrap touchAreaBootstrap = raycastHit.transform.GetComponent("TouchAreaBootstrap") as TouchAreaBootstrap;
                        TouchAreaC tAC = touchAreaBootstrap.m_TAC;
                        if ((camera.isOrthoGraphic && tAC.m_clip && !ChipmunkProWrapper.ucpBBContainsVect(tAC.m_clipBB, touch.position)) || !tAC.m_active)
                        {
                            continue;
                        }
                        if (touchAreaC2 == null)
                        {
                            touchAreaC2 = tAC;
                            num3 = raycastHit.distance;
                        }
                        else if (raycastHit.distance < num3)
                        {
                            if (touchAreaC2.m_allowSecondary)
                            {
                                touchAreaC = touchAreaC2;
                                num2 = num3;
                            }
                            touchAreaC2 = tAC;
                            num3 = raycastHit.distance;
                        }
                        else if ((touchAreaC == null || raycastHit.distance < num2) && tAC.m_allowSecondary)
                        {
                            touchAreaC = tAC;
                            num2 = raycastHit.distance;
                        }
                        if (tAC == tLTouch.m_primaryArea)
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        if (tLTouch.m_primaryArea != null)
                        {
                            if (tLTouch.m_primaryPhase == TouchAreaPhase.Began || tLTouch.m_primaryPhase == TouchAreaPhase.RollIn || tLTouch.m_primaryPhase == TouchAreaPhase.MoveIn || tLTouch.m_primaryPhase == TouchAreaPhase.StationaryIn)
                            {
                                tLTouch.m_primaryPhase = TouchAreaPhase.RollOut;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                                if (tLTouch.m_phase == TouchPhase.Ended || tLTouch.m_phase == TouchPhase.Canceled)
                                {
                                    tLTouch.m_primaryArea.m_touchCount--;
                                    if (tLTouch.m_secondaryArea != null)
                                    {
                                        tLTouch.m_secondaryArea.m_touchCount--;
                                    }
                                    tLTouch.m_primaryPhase = TouchAreaPhase.ReleaseOut;
                                    if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                    }
                                    if (tLTouch.m_secondaryArea != null)
                                    {
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.RollOut;
                                        if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                        {
                                            HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                        }
                                    }
                                    m_touchRemoveList.Add(tLTouch);
                                }
                            }
                            else if (tLTouch.m_phase == TouchPhase.Moved)
                            {
                                if (!tLTouch.m_primaryArea.m_isDragged && Vector2.Distance(tLTouch.m_startPosition, tLTouch.m_currentPosition) > tLTouch.m_primaryArea.m_dragThreshold)
                                {
                                    tLTouch.m_primaryArea.m_isDragged = true;
                                    tLTouch.m_primaryPhase = TouchAreaPhase.DragStart;
                                    if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                    }
                                    if (tLTouch.m_secondaryArea != null && tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        TouchAreaPhase secondaryPhase = tLTouch.m_secondaryPhase;
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.DragStart;
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, false);
                                        tLTouch.m_secondaryPhase = secondaryPhase;
                                    }
                                }
                                tLTouch.m_primaryPhase = TouchAreaPhase.MoveOut;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                            }
                            else if (tLTouch.m_phase == TouchPhase.Stationary)
                            {
                                tLTouch.m_primaryPhase = TouchAreaPhase.StationaryOut;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                            }
                            else if (tLTouch.m_phase == TouchPhase.Ended || tLTouch.m_phase == TouchPhase.Canceled)
                            {
                                tLTouch.m_primaryArea.m_touchCount--;
                                if (tLTouch.m_secondaryArea != null)
                                {
                                    tLTouch.m_secondaryArea.m_touchCount--;
                                }
                                if (tLTouch.m_primaryArea.m_isDragged)
                                {
                                    tLTouch.m_primaryArea.m_isDragged = false;
                                    tLTouch.m_primaryPhase = TouchAreaPhase.DragEnd;
                                    if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                    }
                                    if (tLTouch.m_secondaryArea != null && tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        TouchAreaPhase secondaryPhase2 = tLTouch.m_secondaryPhase;
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.DragEnd;
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, false);
                                        tLTouch.m_secondaryPhase = secondaryPhase2;
                                    }
                                }
                                tLTouch.m_primaryPhase = TouchAreaPhase.ReleaseOut;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                                if (tLTouch.m_secondaryArea != null)
                                {
                                    tLTouch.m_secondaryPhase = TouchAreaPhase.RollOut;
                                    if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                    }
                                }
                                m_touchRemoveList.Add(tLTouch);
                            }
                        }
                        if (!tLTouch.m_consumed && touchAreaC2 != null && tLTouch.m_phase == TouchPhase.Began && touchAreaC2.m_touchCount < touchAreaC2.m_maxTouches)
                        {
                            tLTouch.m_primaryArea = touchAreaC2;
                            tLTouch.m_primaryAreaDepth = num3;
                            if (touchAreaC2.m_consume)
                            {
                                tLTouch.m_consumed = true;
                            }
                            tLTouch.m_primaryPhase = TouchAreaPhase.Began;
                            tLTouch.m_primaryArea.m_touchCount++;
                            tLTouch.m_primaryArea.m_wasDragged = false;
                            if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                            {
                                HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                            }
                        }
                    }
                    else if (tLTouch.m_primaryArea != null && touchAreaC2 != null)
                    {
                        if (tLTouch.m_primaryPhase == TouchAreaPhase.RollOut || tLTouch.m_primaryPhase == TouchAreaPhase.MoveOut || tLTouch.m_primaryPhase == TouchAreaPhase.StationaryOut)
                        {
                            tLTouch.m_primaryPhase = TouchAreaPhase.RollIn;
                            if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                            {
                                HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                            }
                            if (tLTouch.m_phase == TouchPhase.Ended || tLTouch.m_phase == TouchPhase.Canceled)
                            {
                                tLTouch.m_primaryArea.m_touchCount--;
                                if (tLTouch.m_secondaryArea != null)
                                {
                                    tLTouch.m_secondaryArea.m_touchCount--;
                                }
                                tLTouch.m_primaryPhase = TouchAreaPhase.ReleaseIn;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                                if (tLTouch.m_secondaryArea != null)
                                {
                                    tLTouch.m_secondaryPhase = TouchAreaPhase.RollOut;
                                    if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                    }
                                }
                                m_touchRemoveList.Add(tLTouch);
                            }
                        }
                        else if (tLTouch.m_phase == TouchPhase.Moved)
                        {
                            if (!tLTouch.m_primaryArea.m_wasDragged && Vector2.Distance(tLTouch.m_startPosition, tLTouch.m_currentPosition) > tLTouch.m_primaryArea.m_dragThreshold)
                            {
                                tLTouch.m_primaryArea.m_isDragged = true;
                                tLTouch.m_primaryArea.m_wasDragged = true;
                                tLTouch.m_primaryPhase = TouchAreaPhase.DragStart;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                                if (tLTouch.m_secondaryArea != null && tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                {
                                    TouchAreaPhase secondaryPhase3 = tLTouch.m_secondaryPhase;
                                    tLTouch.m_secondaryPhase = TouchAreaPhase.DragStart;
                                    HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, false);
                                    tLTouch.m_secondaryPhase = secondaryPhase3;
                                }
                            }
                            tLTouch.m_primaryPhase = TouchAreaPhase.MoveIn;
                            if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                            {
                                HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                            }
                        }
                        else if (tLTouch.m_phase == TouchPhase.Stationary)
                        {
                            tLTouch.m_primaryPhase = TouchAreaPhase.StationaryIn;
                            if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                            {
                                HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                            }
                        }
                        else if (tLTouch.m_phase == TouchPhase.Ended || tLTouch.m_phase == TouchPhase.Canceled)
                        {
                            tLTouch.m_primaryArea.m_touchCount--;
                            if (tLTouch.m_secondaryArea != null)
                            {
                                tLTouch.m_secondaryArea.m_touchCount--;
                            }
                            if (tLTouch.m_primaryArea.m_wasDragged)
                            {
                                tLTouch.m_primaryArea.m_isDragged = false;
                                tLTouch.m_primaryPhase = TouchAreaPhase.DragEnd;
                                if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                                }
                                if (tLTouch.m_secondaryArea != null && tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                {
                                    TouchAreaPhase secondaryPhase4 = tLTouch.m_secondaryPhase;
                                    tLTouch.m_secondaryPhase = TouchAreaPhase.DragEnd;
                                    HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, false);
                                    tLTouch.m_secondaryPhase = secondaryPhase4;
                                }
                            }
                            tLTouch.m_primaryPhase = TouchAreaPhase.ReleaseIn;
                            if (tLTouch.m_primaryArea.m_delegatedCount > 0)
                            {
                                HandleTouch(tLTouch.m_primaryArea, tLTouch, tLTouch.m_primaryPhase, false);
                            }
                            if (tLTouch.m_secondaryArea != null)
                            {
                                tLTouch.m_secondaryPhase = TouchAreaPhase.RollOut;
                                if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                }
                            }
                            m_touchRemoveList.Add(tLTouch);
                        }
                    }
                    if (tLTouch.m_primaryArea != null && tLTouch.m_primaryPhase != TouchAreaPhase.ReleaseIn && tLTouch.m_primaryPhase != TouchAreaPhase.ReleaseOut)
                    {
                        if (touchAreaC2 != tLTouch.m_primaryArea && touchAreaC2 != null && touchAreaC2.m_allowSecondary)
                        {
                            touchAreaC = touchAreaC2;
                        }
                        if (tLTouch.m_secondaryLocked)
                        {
                            if (tLTouch.m_secondaryArea != null)
                            {
                                if (tLTouch.m_phase == TouchPhase.Moved)
                                {
                                    if (touchAreaC == tLTouch.m_secondaryArea)
                                    {
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.MoveIn;
                                    }
                                    else
                                    {
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.MoveOut;
                                    }
                                    if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                    }
                                }
                                else if (tLTouch.m_phase == TouchPhase.Stationary)
                                {
                                    if (touchAreaC == tLTouch.m_secondaryArea)
                                    {
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.StationaryIn;
                                    }
                                    else
                                    {
                                        tLTouch.m_secondaryPhase = TouchAreaPhase.StationaryOut;
                                    }
                                    if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                    }
                                }
                            }
                        }
                        else if (touchAreaC != tLTouch.m_secondaryArea)
                        {
                            if (touchAreaC == null && tLTouch.m_secondaryPhase != TouchAreaPhase.RollOut)
                            {
                                tLTouch.m_secondaryPhase = TouchAreaPhase.RollOut;
                                tLTouch.m_secondaryArea.m_touchCount--;
                                if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                {
                                    HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                }
                                if (!tLTouch.m_secondaryLocked)
                                {
                                    tLTouch.m_secondaryArea = null;
                                }
                            }
                            else
                            {
                                if (tLTouch.m_secondaryArea != null && tLTouch.m_secondaryPhase != TouchAreaPhase.RollOut)
                                {
                                    tLTouch.m_secondaryPhase = TouchAreaPhase.RollOut;
                                    tLTouch.m_secondaryArea.m_touchCount--;
                                    if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                    }
                                    if (!tLTouch.m_secondaryLocked)
                                    {
                                        tLTouch.m_secondaryArea = null;
                                    }
                                }
                                if (tLTouch.m_secondaryPhase != TouchAreaPhase.RollIn && touchAreaC.m_touchCount < touchAreaC.m_maxTouches && touchAreaC.m_maxTouches == 1 && !tLTouch.m_secondaryLocked)
                                {
                                    tLTouch.m_secondaryArea = touchAreaC;
                                    tLTouch.m_secondaryPhase = TouchAreaPhase.RollIn;
                                    tLTouch.m_secondaryArea.m_touchCount++;
                                    if (tLTouch.m_secondaryArea.m_delegatedCount > 0)
                                    {
                                        HandleTouch(tLTouch.m_secondaryArea, tLTouch, tLTouch.m_secondaryPhase, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int l = 0; l < m_touchEventCount; l++)
        {
            TEvent tEvent = m_touchEvents[l];
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            bool flag3 = false;
            bool flag4 = false;
            for (int m = 0; m < tEvent.touchCount; m++)
            {
                if (tEvent.m_touchPhases[m] == TouchAreaPhase.DragStart)
                {
                    num4++;
                }
                else if (tEvent.m_touchPhases[m] == TouchAreaPhase.DragEnd)
                {
                    num5++;
                }
                else if (tEvent.m_touchPhases[m] == TouchAreaPhase.RollOut)
                {
                    num6++;
                }
                else if (tEvent.m_touchPhases[m] == TouchAreaPhase.ReleaseOut && num6 > 0)
                {
                    flag3 = true;
                }
                else if (tEvent.m_touchPhases[m] == TouchAreaPhase.RollIn)
                {
                    num7++;
                }
                else if (tEvent.m_touchPhases[m] == TouchAreaPhase.ReleaseIn && num7 > 0)
                {
                    flag4 = true;
                }
            }
            if (num4 > 0)
            {
                tEvent.m_touchArea.d_TouchEventDelegate(tEvent.m_touchArea, tEvent.touchCount - num4, tEvent.m_touches, tEvent.m_touchPhases, tEvent.m_touchHots);
                int num8 = 0;
                while (num8 < tEvent.touchCount)
                {
                    if (tEvent.m_touchPhases[num8] == TouchAreaPhase.DragStart)
                    {
                        for (int n = num8; n < tEvent.touchCount; n++)
                        {
                            tEvent.m_touches[n] = tEvent.m_touches[n + 1];
                            tEvent.m_touchPhases[n] = tEvent.m_touchPhases[n + 1];
                            tEvent.m_touchHots[n] = tEvent.m_touchHots[n + 1];
                        }
                        tEvent.touchCount--;
                    }
                    else
                    {
                        num8++;
                    }
                }
            }
            else if (num5 > 0)
            {
                tEvent.m_touchArea.d_TouchEventDelegate(tEvent.m_touchArea, tEvent.touchCount - num5, tEvent.m_touches, tEvent.m_touchPhases, tEvent.m_touchHots);
                int num9 = 0;
                while (num9 < tEvent.touchCount)
                {
                    if (tEvent.m_touchPhases[num9] == TouchAreaPhase.DragEnd)
                    {
                        for (int num10 = num9; num10 < tEvent.touchCount; num10++)
                        {
                            tEvent.m_touches[num10] = tEvent.m_touches[num10 + 1];
                            tEvent.m_touchPhases[num10] = tEvent.m_touchPhases[num10 + 1];
                            tEvent.m_touchHots[num10] = tEvent.m_touchHots[num10 + 1];
                        }
                        tEvent.touchCount--;
                    }
                    else
                    {
                        num9++;
                    }
                }
            }
            if (flag3)
            {
                tEvent.m_touchArea.d_TouchEventDelegate(tEvent.m_touchArea, tEvent.touchCount - num6, tEvent.m_touches, tEvent.m_touchPhases, tEvent.m_touchHots);
                int num11 = 0;
                while (num11 < tEvent.touchCount)
                {
                    if (tEvent.m_touchPhases[num11] == TouchAreaPhase.RollOut)
                    {
                        for (int num12 = num11; num12 < tEvent.touchCount; num12++)
                        {
                            tEvent.m_touches[num12] = tEvent.m_touches[num12 + 1];
                            tEvent.m_touchPhases[num12] = tEvent.m_touchPhases[num12 + 1];
                            tEvent.m_touchHots[num12] = tEvent.m_touchHots[num12 + 1];
                        }
                        tEvent.touchCount--;
                    }
                    else
                    {
                        num11++;
                    }
                }
            }
            else if (flag4)
            {
                tEvent.m_touchArea.d_TouchEventDelegate(tEvent.m_touchArea, tEvent.touchCount - num7, tEvent.m_touches, tEvent.m_touchPhases, tEvent.m_touchHots);
                int num13 = 0;
                while (num13 < tEvent.touchCount)
                {
                    if (tEvent.m_touchPhases[num13] == TouchAreaPhase.RollIn)
                    {
                        for (int num14 = num13; num14 < tEvent.touchCount; num14++)
                        {
                            tEvent.m_touches[num14] = tEvent.m_touches[num14 + 1];
                            tEvent.m_touchPhases[num14] = tEvent.m_touchPhases[num14 + 1];
                            tEvent.m_touchHots[num14] = tEvent.m_touchHots[num14 + 1];
                        }
                        tEvent.touchCount--;
                    }
                    else
                    {
                        num13++;
                    }
                }
            }
            if (tEvent.m_touchArea.m_delegatedCount > 0)
            {
                tEvent.m_touchArea.d_TouchEventDelegate(tEvent.m_touchArea, tEvent.touchCount, tEvent.m_touches, tEvent.m_touchPhases, tEvent.m_touchHots);
            }
            tEvent.touchCount = 0;
        }
        m_touchEventCount = 0;
    }

    private static void HandleTouch(TouchAreaC _touchArea, TLTouch _touch, TouchAreaPhase _phase, bool _hot)
    {
        TEvent tEvent = null;
        for (int i = 0; i < m_touchEventCount; i++)
        {
            if (_touchArea == m_touchEvents[i].m_touchArea)
            {
                tEvent = m_touchEvents[i];
                tEvent.m_touches[tEvent.touchCount] = _touch;
                tEvent.m_touchPhases[tEvent.touchCount] = _phase;
                tEvent.m_touchHots[tEvent.touchCount] = _hot;
                tEvent.touchCount++;
            }
        }
        if (tEvent == null)
        {
            TEvent tEvent2 = m_touchEvents[m_touchEventCount];
            tEvent2.m_touchArea = _touchArea;
            tEvent2.m_touches[tEvent2.touchCount] = _touch;
            tEvent2.m_touchPhases[tEvent2.touchCount] = _phase;
            tEvent2.m_touchHots[tEvent2.touchCount] = _hot;
            tEvent2.touchCount++;
            m_touchEventCount++;
        }
    }
}