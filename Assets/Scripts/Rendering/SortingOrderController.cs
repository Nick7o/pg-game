using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SortingOrderController : MonoBehaviour
{
    private static float _precision = 100;
    private static int _orderOffset = 1000;
    private static int _highestOrder = 1000000;
    
    [Tooltip("If true, recalculates order only at the start.")]
    [SerializeField] private bool _static;
    [SerializeField] private bool _getChildrenAtStart = true;
    [SerializeField] private List<SpriteRenderer> _sprites = new();
    [SerializeField] private float _offset = 0.0f;
    [Range(0f, 1f)]
    [SerializeField] private float _offsetPercent = 0.5f;
    
    private bool _initialized;
    
    private void Start()
    {
        if (_getChildrenAtStart)
            _sprites.AddRange(gameObject.GetComponentsInChildren<SpriteRenderer>());
    }

    private void Update()
    {
        if (_static && _initialized)
        {
            _initialized = true;
            return;
        }

        foreach (var sprite in _sprites)
        {
            float position = TransformSpriteYPosition(sprite);
            sprite.sortingOrder = TransformYPositionToOrder(position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_sprites is not { Count: < 10 })
            return;

        foreach (var sprite in _sprites)
        {
            var pos = sprite.transform.position;
            pos.y = TransformSpriteYPosition(sprite);
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }

    private float TransformSpriteYPosition(SpriteRenderer sprite)
    {
        return sprite.bounds.min.y + _offset + sprite.bounds.extents.y * _offsetPercent;
    }

    private int TransformYPositionToOrder(float yPosition)
    {
        return _highestOrder - (int)(yPosition * _precision) + _orderOffset;
    }
}
