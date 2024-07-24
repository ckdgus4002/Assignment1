using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CookApps.Managers;
using DG.Tweening;
using LCHFramework.Attributes;
using LCHFramework.Components;
using LCHFramework.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using State = CookApps.Managers.State;

namespace CookApps.UI
{
    public enum BlockType
    {
        Red,
        Blue,
        Green,
        Orange,
        Yellow,
        Purple,
        SpinningTop,
        // Munchkin,
    }
    
    [RequireComponent(typeof(Image))]
    public class Block : LCHMonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public const float MoveDuration = 0.2f;
        public const float DestroyJumpDuration = 2f;
        public const float DestroyDuration = 0.5f;
        
        public static int TypeMaxIndex => _typeMaxIndex < 0 ? _typeMaxIndex = Enum.GetValues(typeof(BlockType)).Cast<int>().Max() : _typeMaxIndex;
        private static int _typeMaxIndex = -1;
        
        
        
        private bool _isDragging;
        private Image image;

        
        public BlockType Type
        {
            get => _type;
            set
            {
                Models[(int)value].RadioActiveInSiblings(true);
                _type = value;
            }
        }
        private BlockType _type;

        private bool RaycastTarget
        {
            set
            {
                if (_graphics.IsEmpty()) _graphics = GetComponentsInChildren<Graphic>();
                
                _graphics.ForEach((t, _) => t.raycastTarget = value);
            }
        }
        private Graphic[] _graphics;
        
        public Tile GetTile() => _tile;
        
        public void SetTile(Tile value, float moveDuration = default)
        {
            _tile = value;
            if (0 < moveDuration)
                if (!isDestroying && !IsDestroyed)
                    RectTransformOrNull.DOMove(_tile.RectTransformOrNull.position, moveDuration);
        }
        private Tile _tile;

        public Tile UnderEmptyTileOrNull => GetTile().under.FirstOrDefault(t => t.BlockOrNull == null);
        
        private List<Image> Models
        {
            get
            {
                if (_models.IsEmpty())
                {
                    _models = new List<Image>();
                    for (var i = 0; i <= TypeMaxIndex; i++)
                        _models.Add(RectTransformOrNull.GetChild(i).GetComponent<Image>());
                }
                
                return _models;
            }
        }
        private List<Image> _models;
        
        
        
        protected override void Awake()
        {
            base.Awake();

            image = GetComponent<Image>();
            image.alphaHitTestMinimumThreshold = 1 / 255f;
        }
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(Coroutine());
            IEnumerator Coroutine()
            {
                while (true)
                {
                    var nextTileOrNull = UnderEmptyTileOrNull;
                    if (nextTileOrNull != null) SetTile(nextTileOrNull, MoveDuration);

                    yield return new WaitForSeconds(MoveDuration);    
                }                
            }
        }



        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Type == BlockType.SpinningTop) return;

            _isDragging = true;   
        }
        
        public void OnDrag(PointerEventData eventData) { }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            if (eventData.pointerCurrentRaycast.gameObject != null 
                && eventData.pointerCurrentRaycast.gameObject.TryGetComponentInParent<Block>(out var block)
                && block.Type != BlockType.SpinningTop)
                BlockManager.Instance.OnBlockDragEnd(this, block);
            
            _isDragging = false;
        }



        public void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            gameObject.SetActive(true);
            
            if (Main.Instance.playOnStart.stepOrNull.Index == currentStepIndex && state == State.Appear)
                RectTransformOrNull.DOScale(1, 1);
        }

        private bool isDestroying;
        [ShowInInspector] 
        public void Destroy()
        {
            RaycastTarget = false;
            isDestroying = true;
            switch (Type)
            {
                case BlockType.Red:
                case BlockType.Blue:
                case BlockType.Green:
                case BlockType.Orange:
                case BlockType.Yellow:
                case BlockType.Purple:
                {
                    RectTransformOrNull.DOScale(0, DestroyDuration).SetEase(Ease.InOutBack).OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
                    break;
                }
                case BlockType.SpinningTop:
                {
                    RectTransformOrNull.DOJump(SpinningTopCountText.Instance.RectTransformOrNull.position, -750, 1, DestroyJumpDuration - 1f);
                    RectTransformOrNull.DOScale(0, DestroyJumpDuration).SetEase(Ease.InOutBack).OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}