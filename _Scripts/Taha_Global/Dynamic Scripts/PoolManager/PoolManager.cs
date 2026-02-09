using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton_Abs<PoolManager>
{
    Dictionary<_PoolType, Dictionary<GameObject, List<GameObject>>> _typePools = new Dictionary<_PoolType, Dictionary<GameObject, List<GameObject>>>();
    [SerializeField] private int _preloadAmount = 5;

    public GameObject _Instantiate(_PoolType _type, GameObject _iGameObject)
    {
        return _Instantiate(_type, _iGameObject, Vector3.zero, Quaternion.identity);
    }
    public GameObject _Instantiate(_PoolType _type, GameObject _iGameObject, Vector3 _position, Quaternion _rotation,Transform iParent = null)
    {
        if (!_typePools.TryGetValue(_type, out Dictionary<GameObject, List<GameObject>> _prefabMap))
        {
            _prefabMap = new Dictionary<GameObject, List<GameObject>>();
            _typePools[_type] = _prefabMap;
        }

        if (!_prefabMap.TryGetValue(_iGameObject, out List<GameObject> _list))
        {
            _list = new List<GameObject>();
            _prefabMap[_iGameObject] = _list;
            _Preload(_iGameObject, _list);
        }

        if (_list.Count > 0)
        {
            int _lastIndex = _list.Count - 1;
            GameObject _pooledObject = _list[_lastIndex];
            _list.RemoveAt(_lastIndex);
            _pooledObject.transform.SetPositionAndRotation(_position, _rotation);
            _pooledObject.SetActive(true);
            _pooledObject.transform.parent = iParent;
            return _pooledObject;
        }

        GameObject _newObject = Instantiate(_iGameObject, _position, _rotation, iParent);
        _newObject.AddComponent<_PooledObject>()._prefab = _iGameObject;
        _newObject.GetComponent<_PooledObject>()._type = _type;
        return _newObject;
    }
    public void _Despawn(GameObject _iGameObject)
    {
        _iGameObject.SetActive(false);
        _PooledObject _pooledComponent = _iGameObject.GetComponent<_PooledObject>();
        if (_pooledComponent != null &&
            _typePools.TryGetValue(_pooledComponent._type, out Dictionary<GameObject, List<GameObject>> _prefabMap) &&
            _prefabMap.TryGetValue(_pooledComponent._prefab, out List<GameObject> _list))
        {
            _list.Add(_iGameObject);
        }
    }
    private void _Preload(GameObject _iGameObject, List<GameObject> _list)
    {
        for (int i = 0; i < _preloadAmount; i++)
        {
            GameObject _newObject = Instantiate(_iGameObject);
            _newObject.SetActive(false);
            _newObject.AddComponent<_PooledObject>()._prefab = _iGameObject;
            _list.Add(_newObject);
        }
    }
}

public class _PooledObject : MonoBehaviour
{
    public GameObject _prefab;
    public _PoolType _type;
}

public enum _PoolType
{
    block
}
