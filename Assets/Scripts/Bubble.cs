using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Bubble : MonoBehaviour
{
    [SerializeField] private Transform _scaleTransform;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private AnimationCurve _scaleAnimCurveX;
    [SerializeField] private AnimationCurve _scaleAnimCurveY;
    [SerializeField] private AnimationCurve _scaleAnimCurveZ;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private AnimationCurve _mergeAlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float _scaleAnimDuration = 1f;
    [SerializeField] private float _size = 1;
    [SerializeField] private AnimationCurve _collisionAnimCurveX;
    [SerializeField] private AnimationCurve _collisionAnimCurveY;
    [SerializeField] private AnimationCurve _collisionAnimCurveZ;
    [SerializeField] private float _collisionAnimDuration = 1f;
    [SerializeField] private Vector3 _debugStartVelocity = Vector3.zero;
    [SerializeField] private float _lifetime = 2f;
    [SerializeField] private Collider _collisionCollider;
    [SerializeField] private Collider _triggerCollider;
    private const float c_BaseMass = 2;
    private static float s_MergeVolumeMultiplicator = 1.2f;
    private float _timer = 0f;
    [SerializeField, HideInInspector]
    private float _volume = 1;

    public Rigidbody Rigidbody => _rigidbody;
    public float Size => _size;
    public float Volume => _volume;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(Application.isPlaying)
            return;
        
        if (!_rigidbody)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        if (_scaleTransform != null && (!Mathf.Approximately(_size, _scaleTransform.localScale.x) || !Mathf.Approximately(_rigidbody.mass, c_BaseMass * _size)) || _volume == 0f)
        {
            SetSize(_size);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(_rigidbody);
        }
    }
#endif

    private void Awake()
    {
        if (_debugStartVelocity != Vector3.zero)
        {
            // _rigidbody.linearVelocity = _debugStartVelocity;
        }
        _timer = 0f;
    }

    public void Update()
    {
        if (_isMergingAndDestroying)
        {
            if(_transformToMergeInto == null)
            {
                Destroy(gameObject);
                return;
            }

            _mergeAndDestroyTimer += Time.deltaTime;
            var color = _renderer.material.color;
            float t = _mergeAndDestroyTimer / _mergeAndDestroyDuration;
            color.a = Mathf.Lerp(1f, 0f, _mergeAlphaCurve.Evaluate(t));
            _renderer.material.color = color;
            
            transform.position = Vector3.Lerp(_mergeStartPos, _transformToMergeInto.position, t);
            _scaleTransform.localScale = Vector3.Lerp(_mergeStartScale, _transformToMergeInto.localScale, t);
            return;
        }
        if (!_disabled)
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                Pop();
            }
        }
    }

    public void Pop()
    {
        Destroy(gameObject);
    }

    public void AddVelocity(Vector3 velocity)
    {
        _rigidbody.AddForce(velocity, ForceMode.Impulse);
    }

    private void ComputeVolume()
    {
        _volume = ComputeVolumeFromRadius(_size * 0.5f);
    }
    
    public void SetSize(float size, bool instant = true)
    {
        _size = size;
        if (!instant)
        {
            _scaleTransform.DOKill();
            _scaleTransform.DOScaleX(size, _scaleAnimDuration).SetEase(_scaleAnimCurveX);
            _scaleTransform.DOScaleY(size, _scaleAnimDuration).SetEase(_scaleAnimCurveY);
            _scaleTransform.DOScaleZ(size, _scaleAnimDuration).SetEase(_scaleAnimCurveZ);
        }
        else
        {
            _scaleTransform.localScale = new Vector3(size, size, size);
        }
        #if UNITY_EDITOR
        if(_rigidbody == null)
            _rigidbody = GetComponentInParent<Rigidbody>();
        #endif
        ComputeVolume();
        // _rigidbody.mass = c_BaseMass * size;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_disabled)
            return;
        Debug.Log("OnTriggerEnter " + other.name);
        Bubble bubble = other.GetComponentInParent<Bubble>();
        if (bubble != null && !bubble._disabled && bubble != this)
        {
            MergeWith(bubble);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        PlayCollisionAnim();
    }

    private void PlayCollisionAnim()
    {
        _scaleTransform.localScale = Vector3.one * _size * 0.9f;
        _scaleTransform.DOScaleX(_size, _collisionAnimDuration).SetEase(_collisionAnimCurveX);
        _scaleTransform.DOScaleY(_size, _collisionAnimDuration).SetEase(_collisionAnimCurveY);
        _scaleTransform.DOScaleZ(_size, _collisionAnimDuration).SetEase(_collisionAnimCurveZ);
    }

    private void MergeWith(Bubble bubble)
    {
        Vector3 selfPos = transform.position;
        Vector3 otherPos = bubble.transform.position;
        
        Vector3 selfVelocity = _rigidbody.linearVelocity;
        Vector3 otherVelocity = bubble.Rigidbody.linearVelocity;

        float selfVolume = _volume;
        float otherVolume = bubble._volume;
        float minVolume = Mathf.Min(selfVolume, otherVolume);
        float maxVolume = Mathf.Max(selfVolume, otherVolume);
        minVolume *= s_MergeVolumeMultiplicator;
        float totalVolume = minVolume + maxVolume;
        totalVolume *= s_MergeVolumeMultiplicator;
        float totalRadius = ComputeRadiusFromVolume(totalVolume);
        
        float totalSize = _size + bubble._size;
        float biggestSize = Mathf.Max(_size, bubble._size);
        
        Vector3 fromSelfToOther = otherPos - selfPos;
        float selfWeight = (_size / totalSize);
        float otherWeight = (bubble._size / totalSize);
        Vector3 weightedCenter = selfPos + (fromSelfToOther * otherWeight + (-fromSelfToOther * selfWeight));
        float currentY = selfPos.y;
        // transform.position = weightedCenter;
        if (_size < bubble._size)
        {
            transform.position = otherPos;
        }
        else
        {
            
        }
        Vector3 combinedVelocity = selfVelocity * selfWeight + otherVelocity * otherWeight;

        float newSize = totalRadius * 2f;
        _scaleTransform.localScale = new Vector3(biggestSize, biggestSize, biggestSize);
        transform.position = new Vector3(transform.position.x, currentY + (newSize - _size) * 0.5f, transform.position.z);
        SetSize(newSize, false);
        _rigidbody.linearVelocity = combinedVelocity;
        _lifetime = Mathf.Max(_lifetime, bubble._lifetime);
        bubble.Disable();
        bubble.MergeIntoAndSelfDestruct(_scaleTransform);
        // Destroy(bubble.gameObject);
        _timer = 0f;
    }

    [SerializeField] private float _mergeAndDestroyDuration = 0.1f;
    private float _mergeAndDestroyTimer = 0f;
    private Vector3 _mergeStartScale = Vector3.zero;
    private Vector3 _mergeStartPos = Vector3.zero;
    private bool _isMergingAndDestroying = false;
    // private Bubble _bubbleToMergeInto = null;
    private Transform _transformToMergeInto = null;
    public void MergeIntoAndSelfDestruct(Transform mergeIntoTransform)
    {
        Disable();
        Destroy(_rigidbody);
        var colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                Destroy(colliders[i]);
            }
        }
        _mergeStartScale = _scaleTransform.localScale;
        _mergeStartPos = transform.position;
        _isMergingAndDestroying = true;
        _mergeAndDestroyTimer = 0f;
        _transformToMergeInto = mergeIntoTransform;
        Destroy(gameObject, _mergeAndDestroyDuration);
    }

    bool _disabled = false;
    public void Disable()
    {
        _disabled = true;
        _collisionCollider.enabled = false;
        _triggerCollider.enabled = false;
    }
    public void Enable()
    {
        _disabled = false;
        _collisionCollider.enabled = true;
        _triggerCollider.enabled = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(!_rigidbody)
            return;
        Vector3 velocity = Application.isPlaying ? _rigidbody.linearVelocity : _debugStartVelocity;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }

    public void SetLifetime(float lifetime)
    {
        _lifetime = lifetime;
    }

    public static float ComputeVolumeFromRadius(float radius)
    {
        return 4f / 3f * Mathf.PI * Mathf.Pow(radius, 3f);
    }
    public static float ComputeRadiusFromVolume(float volume)
    {
        return Mathf.Pow((volume * 3f) / (4f * Mathf.PI), 1f/3f);
    }
}
