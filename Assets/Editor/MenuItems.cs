using UnityEditor;
using UnityEngine;

public static class SDFMenuItems
{
    private const string groupPath = "GameObject/SDF/Group/";
    private const string surfacePath = "GameObject/SDF/Surface/";

    [MenuItem(groupPath + "Smooth Union Group", false, 0)]
    public static void CreateSmoothUnioutGroup()
    {
        CreateGroupObject<SmoothUnionShapeGroup>("Smooth Union Group");
    }

    [MenuItem(groupPath + "Smooth Subtract Group", false, 1)]
    public static void CreateSmoothSubtractGroup()
    {
        CreateGroupObject<SmoothSubtractShapeGroup>("Smooth Subtract Group");
    }

    [MenuItem(groupPath + "Smooth Intersect Group", false, 2)]
    public static void CreateSmoothIntersectGroup()
    {
        CreateGroupObject<SmoothIntersectShapeGroup>("Smooth Intersect Group");
    }

    [MenuItem(groupPath + "Union Group", false, 200)]
    public static void CreateUnionGroup()
    {
        CreateGroupObject<UnionShapeGroup>("Union Group");
    }

    [MenuItem(groupPath + "Subtract Group", false, 201)]
    public static void CreateSubtarctGroup()
    {
        CreateGroupObject<SubtractShapeGroup>("Subtract Group");
    }

    [MenuItem(groupPath + "Intersect Group", false, 202)]
    public static void CreateIntersectGroup()
    {
        CreateGroupObject<IntersectShapeGroup>("Intersect Group");
    }

    [MenuItem(groupPath + "Summation Group", false, 303)]
    public static void CreateSummationGroup()
    {
        CreateGroupObject<SummationShapeGroup>("Summation Group");
    }

    [MenuItem(surfacePath + "Sphere", false, 0)]
    public static void CreateSphere()
    {
        CreateSDFObject<SphereSDF>("Sphere");
    }

    [MenuItem(surfacePath + "Cube")]
    public static void CreateCube()
    {
        CreateSDFObject<CubeSDF>("Cube");
    }

    [MenuItem(surfacePath + "Perlin")]
    public static void CreatePerlin()
    {
        CreateSDFObject<PerlinSDF>("Perlin");
    }

    [MenuItem(surfacePath + "Torus")]
    public static void CreateTorus()
    {
        CreateSDFObject<TorusSDF>("Torus");
    }

    [MenuItem(surfacePath + "Capsule")]
    public static void CreateCapsule()
    {
        CreateSDFObject<CapsuleSDF>("Capsule");
    }

    [MenuItem(surfacePath + "Plane")]
    public static void CreatePlane()
    {
        CreateSDFObject<PlaneSDF>("Plane");
    }

    [MenuItem(surfacePath + "Gyroid")]
    public static void CreateGyroid()
    {
        CreateSDFObject<GyroidSDF>("Gyroid");
    }

    public static void CreateSDFObject<T>(string name) where T : Component
    {
        var go = CreateNewGameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.AddComponent<T>();   
        Selection.activeGameObject = go;

        GameObject CreateNewGameObject(string gameObjectName)
        {
            var newGameObject = new GameObject(gameObjectName);

            GameObjectUtility.SetParentAndAlign(newGameObject, Selection.activeGameObject);
            GameObjectUtility.EnsureUniqueNameForSibling(newGameObject);

            return newGameObject;
        }
    }

    public static void CreateGroupObject<T>(string name) where T : Component
    {
        var go = CreateNewGameObject(name);
        go.AddComponent<T>();   
        Selection.activeGameObject = go;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        GameObject CreateNewGameObject(string gameObjectName)
        {
            var newGameObject = new GameObject(gameObjectName);

            GameObjectUtility.SetParentAndAlign(newGameObject, Selection.activeGameObject);
            GameObjectUtility.EnsureUniqueNameForSibling(newGameObject);

            return newGameObject;
        }
    }
}

