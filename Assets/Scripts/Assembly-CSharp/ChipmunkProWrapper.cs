using System;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class ChipmunkProWrapper
{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
    private const string lookFrom = "__Internal";
#else
    private const string lookFrom = "chipmunk";
#endif

    [DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpInitialize();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpAddBody(float mass, float inertia, int componentIndex);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpAddRogueBody(int componentIndex);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpAddStaticBody(int componentIndex);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpAddConstraint(IntPtr constraint, int componentIndex);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpRemoveBody(IntPtr body, ucpConstraintData[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRemoveShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRemoveConstraint(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpRemoveConstraintsFromBody(IntPtr body, ucpConstraintData[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ucpBodyHasCustomProperties(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetAngularDamp(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetGravity(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetLinearDamp(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetAngularDamp(IntPtr body, float angularDamp);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetGravity(IntPtr body, Vector2 gravity);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetLinearDamp(IntPtr body, Vector2 linearDamp);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceAddCollisionHandler(ucpCollisionType collisionTypeA, ucpCollisionType collisionTypeB, bool trackBegin, bool trackPersist, bool trackSeparate);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceRemoveCollisionHandler(ucpCollisionType collisionTypeA, ucpCollisionType collisionTypeB);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpGetBeginCollisionCount();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpGetPersistCollisionCount();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpGetSeparateCollisionCount();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpGetBeginCollisions(ucpCollisionPair[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpGetPersistCollisions(ucpCollisionPair[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpGetSeparateCollisions(ucpCollisionPair[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpGetSpace();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpHastySpaceGetThreads();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpHastySpaceSetThreads(int threads);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceGetStaticBody();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSpaceGetCollisionBias();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSpaceGetCollisionPersistence();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSpaceGetCollisionSlop();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSpaceGetDamping();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpSpaceGetGravity();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSpaceGetIdleSpeedThreshold();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpSpaceGetIterations();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSpaceGetSleepTimeThreshold();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetCollisionBias(float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetCollisionPersistence(uint value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetCollisionSlop(float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetDamping(float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetEnableContactGraph(bool value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetGravity(Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetIdleSpeedThreshold(float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetIterations(int value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceSetSleepTimeThreshold(float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpEnableSegmentToSegmentCollisions();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceStep(float dt);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpClearCollisionLists();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyUpdatePosition(IntPtr body, float dt);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyUpdateVelocity(IntPtr body, Vector2 gravity, float damping, float dt);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern cpBB ucpShapeUpdate(IntPtr shape, Vector2 pos, Vector2 rot);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpSpaceContainsBody(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpSpaceContainsConstraint(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpSpaceContainsShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceConvertBodyToDynamic(IntPtr body, float mass, float moment);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceConvertBodyToStatic(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceReindexShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceReindexShapesForBody(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceReindexStatic();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpAreaForCircle(float r1, float r2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpAreaForPoly(int numVerts, Vector2[] verts);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpAreaForSegment(Vector2 a, Vector2 b, float r);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpCenteroidForPoly(int numVerts, Vector2[] verts);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpMomentForBox(float m, float width, float height);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpMomentForBox2(float m, cpBB box);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpMomentForCircle(float m, float r1, float r2, Vector2 offset);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpMomentForSegment(float m, Vector2 a, Vector2 b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpPolyValidate(Vector2[] verts, int numVerts);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRecenterPoly(int numVerts, Vector2[] verts);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpResetShapeIdCounter();

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpConvexHull(int count, Vector2[] verts, Vector2[] result, float tol);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceAddBody(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceAddConstraint(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceAddShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceAddStaticShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceRemoveBody(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceRemoveConstraint(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceRemoveShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceRemoveStaticShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyActivate(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySleep(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySleepWithGroup(IntPtr body, IntPtr group);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyActivateStatic(IntPtr body, IntPtr filter);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSpaceActivateShapesTouchingShape(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyApplyForce(IntPtr body, Vector2 f, Vector2 r);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyApplyImpulse(IntPtr body, Vector2 f, Vector2 r);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetAngle(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetAngVel(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetAngVelLimit(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetForce(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetMass(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetMoment(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetPos(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetRot(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetTorque(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetVel(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetVelAtLocalPoint(IntPtr body, Vector2 point);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyGetVelAtWorldPoint(IntPtr body, Vector2 point);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetVelLimit(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBodyIsRogue(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBodyIsSleeping(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBodyIsStatic(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyKineticEnergy(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyLocal2World(IntPtr body, Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpBodyWorld2Local(IntPtr body, Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodyResetForces(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetAngle(IntPtr body, float a);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetAngVel(IntPtr body, float a);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetAngVelLimit(IntPtr body, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetForce(IntPtr body, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetMass(IntPtr body, float m);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetMoment(IntPtr body, float i);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetPos(IntPtr body, Vector2 pos);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetTorque(IntPtr body, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetVel(IntPtr body, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetVelLimit(IntPtr body, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBodyGetScale(IntPtr body);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpBodySetScale(IntPtr body, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern cpBB ucpShapeGetBB(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpShapeGetBody(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ucpShapeGetCollisionType(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpShapeGetElasticity(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpShapeGetFriction(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ucpShapeGetGroup(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern uint ucpShapeGetLayers(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpShapeGetSensor(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpShapeGetSurfaceVelocity(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetCollisionType(IntPtr shape, ucpCollisionType value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetElasticity(IntPtr shape, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetFriction(IntPtr shape, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetGroup(IntPtr shape, uint value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetLayers(IntPtr shape, uint value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetSensor(IntPtr shape, bool value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpShapeSetSurfaceVelocity(IntPtr shape, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpCircleShapeNew(IntPtr body, float radius, Vector2 offset, ucpCollisionType collisionType);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpCircleShapeGetRadius(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpCircleShapeGetOffset(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpCircleShapeSetRadius(IntPtr shape, float radius);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpCircleShapeSetOffset(IntPtr shape, Vector2 offset);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSegmentShapeNew(IntPtr body, Vector2 a, Vector2 b, float radius, ucpCollisionType collisionType);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpSegmentShapeGetA(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpSegmentShapeGetB(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpSegmentShapeGetNormal(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSegmentShapeGetRadius(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSegmentShapeSetEndpoints(IntPtr shape, Vector2 a, Vector2 b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSegmentShapeSetRadius(IntPtr shape, float radius);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSegmentShapeSetNeighbors(IntPtr shape, Vector2 prev, Vector2 next);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSegmentShapeSetPrevNeighborTangent(IntPtr shape, Vector2 tangent);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSegmentShapeSetNextNeighborTangent(IntPtr shape, Vector2 tangent);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpPolyShapeNew(IntPtr body, int numVerts, Vector2[] verts, Vector2 offset, ucpCollisionType collisionType);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpPolyShapeGetNumVerts(IntPtr shape);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpPolyShapeGetVert(IntPtr shape, int idx);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpPolyShapeSetVerts(IntPtr shape, int numVerts, Vector2[] verts, Vector2 offset);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpConstraintActivateBodies(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpConstraintGetA(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpConstraintGetB(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpConstraintGetErrorBias(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpConstraintGetImpulse(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpConstraintGetMaxBias(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpConstraintGetMaxForce(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpConstraintSetErrorBias(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpConstraintSetMaxBias(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpConstraintSetMaxForce(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpDampedRotarySpringNew(IntPtr a, IntPtr b, float restAngle, float stiffness, float damping);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpDampedRotarySpringGetDamping(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpDampedRotarySpringGetRestAngle(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpDampedRotarySpringGetStiffness(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedRotarySpringSetDamping(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedRotarySpringSetRestAngle(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedRotarySpringSetStiffness(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpDampedSpringNew(IntPtr a, IntPtr b, Vector2 anchr1, Vector2 anchr2, float restLength, float stiffness, float damping);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpDampedSpringGetAnchr1(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpDampedSpringGetAnchr2(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpDampedSpringGetDamping(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpDampedSpringGetRestLength(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpDampedSpringGetStiffness(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedSpringSetAnchr1(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedSpringSetAnchr2(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedSpringSetDamping(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedSpringSetRestLength(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpDampedSpringSetStiffness(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpGearJointNew(IntPtr a, IntPtr b, float phase, float ratio);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpGearJointGetPhase(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpGearJointGetRatio(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpGearJointSetPhase(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpGearJointSetRatio(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpGrooveJointNew(IntPtr a, IntPtr b, Vector2 groove_a, Vector2 groobe_b, Vector2 anchr2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpGrooveJointGetAnchr2(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpGrooveJointGetGrooveA(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpGrooveJointGetGrooveB(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpGrooveJointSetAnchr2(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpGrooveJointSetGrooveA(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpGrooveJointSetGrooveB(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpPinJointNew(IntPtr a, IntPtr b, Vector2 anchr1, Vector2 anchr2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpPinJointGetAnchr1(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpPinJointGetAnchr2(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpPinJointGetDist(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpPinJointSetAnchr1(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpPinJointSetAnchr2(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpPinJointSetDist(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpPivotJointNew(IntPtr a, IntPtr b, Vector2 pivot);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpPivotJointNew2(IntPtr a, IntPtr b, Vector2 anchr1, Vector2 anchr2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpPivotJointGetAnchr1(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpPivotJointGetAnchr2(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpPivotJointSetAnchr1(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpPivotJointSetAnchr2(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpRatchetJointNew(IntPtr a, IntPtr b, float phase, float ratchet);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpRatchetJointGetAngle(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpRatchetJointGetPhase(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpRatchetJointGetRatchet(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRatchetJointSetAngle(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRatchetJointSetPhase(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRatchetJointSetRatchet(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpRotaryLimitJointNew(IntPtr a, IntPtr b, float min, float max);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpRotaryLimitJointGetMax(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpRotaryLimitJointGetMin(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRotaryLimitJointSetMax(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpRotaryLimitJointSetMin(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSimpleMotorNew(IntPtr a, IntPtr b, float rate);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSimpleMotorGetRate(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSimpleMotorSetRate(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSlideJointNew(IntPtr a, IntPtr b, Vector2 anchr1, Vector2 anchr2, float min, float max);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpSlideJointGetAnchr1(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpSlideJointGetAnchr2(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSlideJointGetMax(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpSlideJointGetMin(IntPtr constraint);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSlideJointSetAnchr1(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSlideJointSetAnchr2(IntPtr constraint, Vector2 value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSlideJointSetMax(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern void ucpSlideJointSetMin(IntPtr constraint, float value);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern cpBB ucpBBNew(float l, float b, float r, float t);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern cpBB ucpBBNewForCircle(Vector2 p, float r);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern cpBB ucpBBExpand(cpBB bb, Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern cpBB ucpBBMerge(cpBB a, cpBB b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBBMergedArea(cpBB a, cpBB b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBBContainsBB(cpBB bb, cpBB other);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBBContainsVect(cpBB bb, Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBBIntersects(cpBB a, cpBB b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpBBIntersectsSegment(cpBB bb, Vector2 a, Vector2 b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpBBSegmentQuery(cpBB bb, Vector2 a, Vector2 b);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvadd(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvclamp(Vector2 v, float len);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvcross(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvdist(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvdistsq(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvdot(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpveql(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvforangle(float a);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvlength(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvlengthsq(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvlerp(Vector2 v1, Vector2 v2, float t);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvlerpconst(Vector2 v1, Vector2 v2, float d);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvmult(Vector2 v, float s);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpvnear(Vector2 v1, Vector2 v2, float dist);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvneg(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvnormalize_safe(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvperp(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvproject(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvrotate(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvrperp(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvslerp(Vector2 v1, Vector2 v2, float t);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvslerpconst(Vector2 v1, Vector2 v2, float a);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvsub(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpvtoangle(Vector2 v);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern Vector2 ucpvunrotate(Vector2 v1, Vector2 v2);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern float ucpShapeNearestPointQuery(IntPtr shape, Vector2 p, ref cpNearestPointQueryInfo info);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpShapePointQuery(IntPtr shape, Vector2 p);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ucpShapeSegmentQuery(IntPtr shape, Vector2 a, Vector2 b, ref cpSegmentQueryInfo info);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceNearestPointQueryNearest(Vector2 point, float maxDistance, uint layers, uint group, ref cpNearestPointQueryInfo info);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpacePointQueryFirst(Vector2 point, uint layers, uint group);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr ucpSpaceSegmentQueryFirst(Vector2 start, Vector2 end, uint layers, uint group, ref cpSegmentQueryInfo info);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpSpacePointQuery(Vector2 point, uint layers, uint group, ucpQueryInfo[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpSpaceBBQuery(cpBB bb, uint layers, uint group, ucpQueryInfo[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpSpaceNearestPointQuery(Vector2 point, float maxDistance, uint layers, uint group, ucpQueryInfo[] results, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpSpaceSegmentQuery(Vector2 a, Vector2 b, uint layers, uint group, ref cpSegmentQueryInfo info, int maxResults);

	[DllImport(lookFrom, CallingConvention = CallingConvention.Cdecl)]
	public static extern int ucpSpaceShapeQuery(IntPtr shape, ucpQueryInfo[] results, int maxResults);
}
