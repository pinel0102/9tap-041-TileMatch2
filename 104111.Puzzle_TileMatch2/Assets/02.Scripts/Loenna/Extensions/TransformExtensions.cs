using UnityEngine;

public static class TransformExtensions
{
	public static void GetComponent<T>(this Transform transform, out T component)
		where T : Component
	{
		component = null;

		if (transform == null)
		{
			return;
		}

		component = transform.GetComponent<T>();
	}

	public static GameObject GetGameObjectAtPath(this Transform transform, string path)
	{
		GetGameObjectAtPath(transform, path, out GameObject go);
		return go;
	}

	public static void GetGameObjectAtPath(this Transform transform, string path, out GameObject go)
	{
		TryGetGameObjectAtPath(transform, path, out go);
	}

	public static bool TryGetGameObjectAtPath(this Transform transform, string path, out GameObject go)
	{
		go = null;

		if (transform == null)
		{
			return false;
		}

		if (string.IsNullOrEmpty(path))
		{
			go = transform.gameObject;
			return true;
		}

		Transform t = transform.Find(path);
		if (t == null)
		{
			return false;
		}

		go = t.gameObject;
		return true;
	}

	public static bool TryGetComponentAtPath<T>(this Transform transform, string path, out T component)
	{
		component = default(T);

		if (transform.TryGetGameObjectAtPath(path , out GameObject go))
		{
			component = go.GetComponent<T>();
			return true;
		}

		return false;
	}

	public static void SetLayer(this Transform t, int layer)
	{
		if (t == null)
		{
			return;
		}
		t.gameObject.layer = layer;

		for (int i = 0; i < t.childCount; ++i)
		{
			Transform childTrans = t.GetChild(i);
			if (childTrans == null)
			{
				continue;
			}
			childTrans.SetLayer(layer);
		}
	}

	public static void Reset(this Transform transform)
	{
		if (transform == null)
		{
			return;
		}
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}

	public static void SetParentReset(this Transform transform, Transform parent, bool syncLayer = false)
	{
		if (transform == null)
		{
			return;
		}
		transform.SetParent(parent);
		if (syncLayer && parent != null)
		{
			transform.SetLayer(parent.gameObject.layer);
		}
		transform.Reset();
	}

	public static void SetPosition(this Transform transform, Transform other)
	{
		transform.position = other.position;
	}

	public static void SetPosition(this Transform transform, Vector3 position)
	{
		transform.position = position;
	}

	public static void SetPosition(this Transform transform, float x, float y, float z)
	{
		transform.position = new Vector3(x, y, z);
	}

	public static void SetPositionXY(this Transform transform, Vector2 xy)
	{
		Vector3 pos = transform.position;
		pos.x = xy.x;
		pos.y = xy.y;
		transform.position = pos;
	}

	public static void SetPositionXY(this Transform transform, float x, float y)
	{
		Vector3 pos = transform.position;
		pos.x = x;
		pos.y = y;
		transform.position = pos;
	}

	public static void SetPositionXZ(this Transform transform, Vector2 xz)
	{
		Vector3 pos = transform.position;
		pos.x = xz.x;
		pos.z = xz.y;
		transform.position = pos;
	}

	public static void SetPositionXZ(this Transform transform, float x, float z)
	{
		Vector3 pos = transform.position;
		pos.x = x;
		pos.z = z;
		transform.position = pos;
	}

	public static void SetPositionYZ(this Transform transform, Vector2 yz)
	{
		Vector3 pos = transform.position;
		pos.y = yz.x;
		pos.z = yz.y;
		transform.position = pos;
	}

	public static void SetPositionYZ(this Transform transform, float y, float z)
	{
		Vector3 pos = transform.position;
		pos.y = y;
		pos.z = z;
		transform.position = pos;
	}

	public static void SetPositionX(this Transform transform, float x)
	{
		Vector3 pos = transform.position;
		pos.x = x;
		transform.position = pos;
	}

	public static void SetPositionY(this Transform transform, float y)
	{
		Vector3 pos = transform.position;
		pos.y = y;
		transform.position = pos;
	}

	public static void SetPositionZ(this Transform transform, float z)
	{
		Vector3 pos = transform.position;
		pos.z = z;
		transform.position = pos;
	}

	public static void SetLocalPosition(this Transform transform, Transform other)
	{
		transform.localPosition = other.localPosition;
	}

	public static void SetLocalPosition(this Transform transform, Vector3 position)
	{
		transform.localPosition = position;
	}

	public static void SetLocalPosition(this Transform transform, float x, float y, float z)
	{
		transform.localPosition = new Vector3(x, y, z);
	}

	public static void SetLocalPositionXY(this Transform transform, Vector2 xy)
	{
		Vector3 pos = transform.localPosition;
		pos.x = xy.x;
		pos.y = xy.y;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionXY(this Transform transform, float x, float y)
	{
		Vector3 pos = transform.localPosition;
		pos.x = x;
		pos.y = y;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionXZ(this Transform transform, Vector2 xz)
	{
		Vector3 pos = transform.localPosition;
		pos.x = xz.x;
		pos.z = xz.y;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionXZ(this Transform transform, float x, float z)
	{
		Vector3 pos = transform.localPosition;
		pos.x = x;
		pos.z = z;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionYZ(this Transform transform, Vector2 yz)
	{
		Vector3 pos = transform.localPosition;
		pos.y = yz.x;
		pos.z = yz.y;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionYZ(this Transform transform, float y, float z)
	{
		Vector3 pos = transform.localPosition;
		pos.y = y;
		pos.z = z;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionX(this Transform transform, float x)
	{
		Vector3 pos = transform.localPosition;
		pos.x = x;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionY(this Transform transform, float y)
	{
		Vector3 pos = transform.localPosition;
		pos.y = y;
		transform.localPosition = pos;
	}

	public static void SetLocalPositionZ(this Transform transform, float z)
	{
		Vector3 pos = transform.localPosition;
		pos.z = z;
		transform.localPosition = pos;
	}

	public static void SetEulerAngles(this Transform transform, Transform other)
	{
		transform.eulerAngles = other.eulerAngles;
	}

	public static void SetEulerAngles(this Transform transform, Vector3 eulerAngles)
	{
		transform.eulerAngles = eulerAngles;
	}

	public static void SetEulerAngles(this Transform transform, float x, float y, float z)
	{
		transform.eulerAngles = new Vector3(x, y, z);
	}

	public static void SetEulerAnglesXY(this Transform transform, Vector2 xy)
	{
		Vector3 rot = transform.eulerAngles;
		rot.x = xy.x;
		rot.y = xy.y;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesXY(this Transform transform, float x, float y)
	{
		Vector3 rot = transform.eulerAngles;
		rot.x = x;
		rot.y = y;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesYZ(this Transform transform, Vector2 yz)
	{
		Vector3 rot = transform.eulerAngles;
		rot.y = yz.x;
		rot.z = yz.y;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesYZ(this Transform transform, float y, float z)
	{
		Vector3 rot = transform.eulerAngles;
		rot.y = y;
		rot.z = z;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesXZ(this Transform transform, Vector2 xz)
	{
		Vector3 rot = transform.eulerAngles;
		rot.x = xz.x;
		rot.z = xz.y;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesXZ(this Transform transform, float x, float z)
	{
		Vector3 rot = transform.eulerAngles;
		rot.x = x;
		rot.z = z;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesX(this Transform transform, float x)
	{
		Vector3 rot = transform.eulerAngles;
		rot.x = x;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesY(this Transform transform, float y)
	{
		Vector3 rot = transform.eulerAngles;
		rot.y = y;
		transform.eulerAngles = rot;
	}

	public static void SetEulerAnglesZ(this Transform transform, float z)
	{
		Vector3 rot = transform.eulerAngles;
		rot.z = z;
		transform.eulerAngles = rot;
	}

	public static void SetLocalEulerAngles(this Transform transform, Transform other)
	{
		transform.localEulerAngles = other.localEulerAngles;
	}

	public static void SetLocalEulerAngles(this Transform transform, Vector3 eulerAngles)
	{
		transform.localEulerAngles = eulerAngles;
	}

	public static void SetLocalEulerAngles(this Transform transform, float x, float y, float z)
	{
		transform.localEulerAngles = new Vector3(x, y, z);
	}

	public static void SetLocalEulerAnglesXY(this Transform transform, Vector2 xy)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x = xy.x;
		rot.y = xy.y;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesXY(this Transform transform, float x, float y)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x = x;
		rot.y = y;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesYZ(this Transform transform, Vector2 yz)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.y = yz.x;
		rot.z = yz.y;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesYZ(this Transform transform, float y, float z)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.y = y;
		rot.z = z;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesXZ(this Transform transform, Vector2 xz)
	{
		Vector3 rpt = transform.localEulerAngles;
		rpt.x = xz.x;
		rpt.z = xz.y;
		transform.localEulerAngles = rpt;
	}

	public static void SetLocalEulerAnglesXZ(this Transform transform, float x, float z)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x = x;
		rot.z = z;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesX(this Transform transform, float x)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x = x;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesY(this Transform transform, float y)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.y = y;
		transform.localEulerAngles = rot;
	}

	public static void SetLocalEulerAnglesZ(this Transform transform, float z)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.z = z;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAngles(this Transform transform, Vector3 eulerAngles)
	{
		transform.localEulerAngles += eulerAngles;
	}

	public static void AddLocalEulerAngles(this Transform transform, float x, float y, float z)
	{
		transform.localEulerAngles += new Vector3(x, y, z);
	}

	public static void AddLocalEulerAnglesXY(this Transform transform, Vector2 xy)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x += xy.x;
		rot.y += xy.y;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesXY(this Transform transform, float x, float y)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x += x;
		rot.y += y;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesYZ(this Transform transform, Vector2 yz)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.y += yz.x;
		rot.z += yz.y;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesYZ(this Transform transform, float y, float z)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.y += y;
		rot.z += z;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesXZ(this Transform transform, Vector2 xz)
	{
		Vector3 rpt = transform.localEulerAngles;
		rpt.x += xz.x;
		rpt.z += xz.y;
		transform.localEulerAngles = rpt;
	}

	public static void AddLocalEulerAnglesXZ(this Transform transform, float x, float z)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x += x;
		rot.z += z;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesX(this Transform transform, float x)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.x += x;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesY(this Transform transform, float y)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.y += y;
		transform.localEulerAngles = rot;
	}

	public static void AddLocalEulerAnglesZ(this Transform transform, float z)
	{
		Vector3 rot = transform.localEulerAngles;
		rot.z += z;
		transform.localEulerAngles = rot;
	}

	public static void SetRotation(this Transform transform, Transform other)
	{
		transform.rotation = other.rotation;
	}

	public static void SetRotation(this Transform transform, Quaternion rotation)
	{
		transform.rotation = rotation;
	}

	public static void SetRotation(this Transform transform, float x, float y, float z, float w)
	{
		transform.rotation = new Quaternion(x, y, z, w);
	}

	public static void SetLocalRotation(this Transform transform, Transform other)
	{
		transform.localRotation = other.localRotation;
	}

	public static void SetLocalRotation(this Transform transform, Quaternion rotation)
	{
		transform.localRotation = rotation;
	}

	public static void SetLocalRotation(this Transform transform, float x, float y, float z, float w)
	{
		transform.localRotation = new Quaternion(x, y, z, w);
	}

	public static void SetLocalScale(this Transform transform, Transform other)
	{
		transform.localScale = other.localScale;
	}

	public static void SetLocalScale(this Transform transform, Vector3 scale)
	{
		transform.localScale = scale;
	}

	public static void SetLocalScale(this Transform transform, float x, float y, float z)
	{
		transform.localScale = new Vector3(x, y, z);
	}

	public static void SetLocalScale(this Transform transform, float scale)
	{
		transform.localScale = new Vector3(scale, scale, scale);
	}

	public static void SetLocalScaleXY(this Transform transform, Vector2 xy)
	{
		Vector3 scl = transform.localScale;
		scl.x = xy.x;
		scl.y = xy.y;
		transform.localScale = scl;
	}

	public static void SetLocalScaleXY(this Transform transform, float scale)
	{
		Vector3 scl = transform.localScale;
		scl.x = scale;
		scl.y = scale;
		transform.localScale = scl;
	}

	public static void SetLocalScaleXY(this Transform transform, float x, float y)
	{
		Vector3 scl = transform.localScale;
		scl.x = x;
		scl.y = y;
		transform.localScale = scl;
	}

	public static void SetLocalScaleXZ(this Transform transform, Vector2 xz)
	{
		Vector3 scl = transform.localScale;
		scl.x = xz.x;
		scl.z = xz.y;
		transform.localScale = scl;
	}

	public static void SetLocalScaleXZ(this Transform transform, float scale)
	{
		Vector3 scl = transform.localScale;
		scl.x = scale;
		scl.z = scale;
		transform.localScale = scl;
	}

	public static void SetLocalScaleXZ(this Transform transform, float x, float z)
	{
		Vector3 scl = transform.localScale;
		scl.x = x;
		scl.z = z;
		transform.localScale = scl;
	}

	public static void SetLocalScaleYZ(this Transform transform, Vector2 yz)
	{
		Vector3 scl = transform.localScale;
		scl.y = yz.x;
		scl.z = yz.y;
		transform.localScale = scl;
	}

	public static void SetLocalScaleYZ(this Transform transform, float scale)
	{
		Vector3 scl = transform.localScale;
		scl.y = scale;
		scl.z = scale;
		transform.localScale = scl;
	}

	public static void SetLocalScaleYZ(this Transform transform, float y, float z)
	{
		Vector3 scl = transform.localScale;
		scl.y = y;
		scl.z = z;
		transform.localScale = scl;
	}

	public static void SetLocalScaleX(this Transform transform, float x)
	{
		Vector3 scl = transform.localScale;
		scl.x = x;
		transform.localScale = scl;
	}

	public static void SetLocalScaleY(this Transform transform, float y)
	{
		Vector3 scl = transform.localScale;
		scl.y = y;
		transform.localScale = scl;
	}

	public static void SetLocalScaleZ(this Transform transform, float z)
	{
		Vector3 scl = transform.localScale;
		scl.z = z;
		transform.localScale = scl;
	}
}
