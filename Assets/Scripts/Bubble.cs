using System;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Bubble : MonoBehaviour
{
    [SerializeField] private Transform _scaleTransform;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _size = 1;
    [SerializeField] private Vector3 _debugStartVelocity = Vector3.zero;
    private const float c_BaseMass = 2;

    public Rigidbody Rigidbody => _rigidbody;
    public float Size => _size;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_rigidbody)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        if (_scaleTransform != null && (!Mathf.Approximately(_size, _scaleTransform.localScale.x) || !Mathf.Approximately(_rigidbody.mass, c_BaseMass * _size)))
        {
            SetSize(_size);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(_rigidbody);
        }
    }
#endif

    private void Awake()
    {
        _rigidbody.linearVelocity = _debugStartVelocity;
    }

    public void Update()
    {
        
    }

    public void AddVelocity(Vector3 velocity)
    {
        _rigidbody.AddForce(velocity, ForceMode.Impulse);
    }

    public void SetSize(float size, bool instant = true)
    {
        _size = size;
        if (!instant)
        {
            transform.DOScale(size, .1f);
        }
        else
        {
            _scaleTransform.localScale = new Vector3(size, size, size);
        }
        #if UNITY_EDITOR
        if(_rigidbody == null)
            _rigidbody = GetComponentInParent<Rigidbody>();
        #endif
        _rigidbody.mass = c_BaseMass * size;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_disabled)
            return;
        Debug.Log("OnTriggerEnter " + other.name);
        Bubble bubble = other.GetComponentInParent<Bubble>();
        if (bubble != null)
        {
            MergeWith(bubble);
        }
    }

    private void MergeWith(Bubble bubble)
    {
        Vector3 selfPos = transform.position;
        Vector3 otherPos = bubble.transform.position;
        
        Vector3 selfVelocity = _rigidbody.linearVelocity;
        Vector3 otherVelocity = bubble.Rigidbody.linearVelocity;

        float selfVolume = 4f/3f * Mathf.PI * Mathf.Pow(_size * 0.5f, 3);
        float otherVolume = 4f/3f * Mathf.PI * Mathf.Pow(bubble._size * 0.5f, 3);
        
        float totalVolume = selfVolume + otherVolume;
        float totalRadius = Mathf.Pow((totalVolume * 3f) / (4f * Mathf.PI), 1f/3f);
        
        float totalSize = _size + bubble._size;
        float biggestSize = Mathf.Max(_size, bubble._size);
        
        Vector3 fromSelfToOther = otherPos - selfPos;
        float selfWeight = (_size / totalSize);
        float otherWeight = (bubble._size / totalSize);
        Vector3 weightedCenter = selfPos + fromSelfToOther * otherWeight;
        transform.position = weightedCenter;
        Vector3 combinedVelocity = selfVelocity * selfWeight + otherVelocity * otherWeight;

        float newSize = totalRadius * 2f;
        transform.localScale = new Vector3(biggestSize, biggestSize, biggestSize);
        SetSize(newSize, false);
        _rigidbody.linearVelocity = combinedVelocity;
        bubble.Disable();
        Destroy(bubble.gameObject);
    }
    bool _disabled = false;
    private void Disable()
    {
        _disabled = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 velocity = Application.isPlaying ? _rigidbody.linearVelocity : _debugStartVelocity;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}
