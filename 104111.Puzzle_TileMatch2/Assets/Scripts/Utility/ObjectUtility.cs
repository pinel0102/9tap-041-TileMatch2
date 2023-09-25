#nullable enable

public static class ObjectUtility
{
    public static T? GetRawObject<T>(T obj)
        where T : class
    {
        if (obj is UnityEngine.Object unityObj)
        {
            return unityObj != null ? obj : null;
        }
        else
        {
            return obj;
        }
    }

    public static bool IsNullOrDestroyed<T>(T? obj)
    {
        if (obj == null)
        {
            return true;
        }

        return obj is UnityEngine.Object unityObj ? unityObj == null : obj == null;
    }
}
