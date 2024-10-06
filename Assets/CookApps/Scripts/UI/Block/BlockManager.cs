using System;
using System.Collections.Generic;
using System.Linq;
using CookApps.Managers;
using LCHFramework.Extensions;
using LCHFramework.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CookApps.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BlockManager : MonoSingleton<BlockManager>
    {
        public int firstRowCount = 3;
        public int columnCount = 7;
        public float blockMoveSpeed = 1;
        [SerializeField] private Block defaultBlock;


        private int tilesTotalLength;
        private CanvasGroup canvasGroup;
        private GameObject wrapper;
        private RectTransform tileGroupsWrapper;
        private RectTransform blockGroupsWrapper;


        public event Action<List<BlockType>> onDestroyBlockBegin;
        public event Action onBlockDragEnd;
        
        
        public Tile[][] Tiles { get; private set; }
        
        public List<Block> Blocks { get; } = new();
        
        
        public int ColumnHalfCount => columnCount / 2;
        
        
        
        protected override void Awake()
        {
            base.Awake();

            canvasGroup = GetComponent<CanvasGroup>();
            wrapper = RectTransformOrNull.GetChild(0).gameObject;
            tileGroupsWrapper = (RectTransform)wrapper.transform.GetChild(0);
            blockGroupsWrapper = (RectTransform)wrapper.transform.GetChild(1);
        }
        
        protected override async void Start()
        {
            base.Start();

            Main.Instance.OnCurrentStepIndexChanged += OnCurrentStepIndexChanged;
            
            await Awaitable.EndOfFrameAsync();
            
            Tiles = new Tile[columnCount][];
            var rowCount = firstRowCount;
            for (var i = 0; i < columnCount; i++)
            {
                var tileGroup = tileGroupsWrapper.GetChild(i);
                Tiles[i] = new Tile[rowCount];
                for (var j = 0; j < rowCount; j++)
                {
                    var tile = tileGroup.GetChild(j).GetComponent<Tile>();
                    tile.RectTransformOrNull.localScale = Vector3.zero;
                    Tiles[i][j] = tile;
                    tilesTotalLength++;
                    
                    var block = InstantiateBlock(tile.RectTransformOrNull.position);
                    block.RectTransformOrNull.localScale = Vector3.zero;
                    block.SetTile(tile);
                }
                
                if (i < ColumnHalfCount) rowCount++;
                else rowCount--;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (!Main.InstanceIsNull) Main.Instance.OnCurrentStepIndexChanged -= OnCurrentStepIndexChanged;
        }
        
        
        
        private void OnCurrentStepIndexChanged(int prevStepIndex, int currentStepIndex) => Show(prevStepIndex, currentStepIndex, State.Appear);

        private bool _onBlockDragEnd;
        public async void OnBlockDragEnd(Block from, Block to)
        {
            if (from == to) return;
            
            var distance = (1, 1);
            var (fromI, fromJ) = from.GetTile().IJ;
            var (toI, toJ) = to.GetTile().IJ;
            if (distance.Item1 < Mathf.Abs(fromI - toI) || distance.Item2 < Mathf.Abs(fromJ - toJ)) return;

            onBlockDragEnd?.Invoke();
            SwapTile(from, to);
            if (!GetDestroyBlocks(true).IsEmpty()) return;
            
            _onBlockDragEnd = true;
            await Awaitable.WaitForSecondsAsync(Block.MoveDuration);
                
            SwapTile(from, to);
            _onBlockDragEnd = false;
        }

        private bool _canInstantiate;
        private async void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            wrapper.SetActive(true);
            canvasGroup.blocksRaycasts = false;
            foreach (var t1 in Tiles) foreach (var t in t1) t.Show(prevStepIndex, currentStepIndex, state);
            foreach (var b in Blocks) b.Show(prevStepIndex, currentStepIndex, state);

            while (Main.Instance.CurrentStep is GameStep)
            {
                await Awaitable.NextFrameAsync();
                
                var shownUI = Toast.Instance.IsShown || AlertManager.Instance.IsShown;
                var destroyBlocks = GetDestroyBlocks();
                var canDestroy = !shownUI && !_onBlockDragEnd && !destroyBlocks.IsEmpty();
                if (canDestroy)
                {
                    canvasGroup.blocksRaycasts = false;
                    // 블럭 삭제.
                    var destroyBlockTypes = destroyBlocks.Select(t => t.Type).ToList();
                    foreach (var t in destroyBlocks)
                    {
                        t.RectTransformOrNull.SetAsLastSibling();
                        t.Destroy();
                    }
                    onDestroyBlockBegin?.Invoke(destroyBlockTypes);
                    await Awaitable.WaitForSecondsAsync(Block.DestroyDuration);
                }
                
                var instantiateBlocksCount = tilesTotalLength - Blocks.Count;
                if (_canInstantiate)
                {
                    canvasGroup.blocksRaycasts = false;
                    for (var i = instantiateBlocksCount; 0 < i; i--)
                    {
                        // 블럭 생성.
                        var tile = Tiles[ColumnHalfCount].Last();
                        var block = InstantiateBlock(tile.RectTransformOrNull.position.AddY(tile.Height));
                        block.SetTile(tile, Block.MoveDuration);
                        await Awaitable.WaitForSecondsAsync(Block.MoveDuration);    
                    }
                }
                
                var canMove = canDestroy || _canInstantiate;
                if (canMove)
                {
                    canvasGroup.blocksRaycasts = false;
                    // 블럭 이동 Waiting.
                    foreach (var t in destroyBlocks) Blocks.Remove(t);
                    while (Blocks.Any(t => t.UnderEmptyTileOrNull != null)) await Awaitable.WaitForSecondsAsync(Block.MoveDuration);
                    
                    // 마지막 블럭 이동 시작.
                    await Awaitable.WaitForSecondsAsync(Block.MoveDuration);

                    _destroyBlocks.Clear();
                }

                _canInstantiate = !canDestroy && !canMove && 0 < instantiateBlocksCount;
                canvasGroup.blocksRaycasts = !shownUI && !_onBlockDragEnd && !canDestroy && !_canInstantiate && !canMove;
            }
        }
        
        private List<Block> _destroyBlocks = new();
        private List<Block> GetDestroyBlocks(bool force = false)
        {
            if (force || _destroyBlocks.IsEmpty())
            {
                _destroyBlocks.Clear();
                
                var result = new List<Block>();
                for (var i = 0; i < Tiles.Length; i++)
                    for (var j = 0; j < Tiles[i].Length; j++)
                    {
                        var tile = Tiles[i][j]; 
                        if (tile.BlockOrNull == null) continue;

                        var block = tile.BlockOrNull;
                        if (tile.sqaure1.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 3) result.AddRange(tile.sqaure1.Select(t => t.BlockOrNull));
                        if (tile.sqaure2.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 3) result.AddRange(tile.sqaure2.Select(t => t.BlockOrNull));
                        if (tile.sqaure3.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 3) result.AddRange(tile.sqaure3.Select(t => t.BlockOrNull));
                        if (tile.sqaure4.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 3) result.AddRange(tile.sqaure4.Select(t => t.BlockOrNull));
                        if (tile.sqaure5.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 3) result.AddRange(tile.sqaure5.Select(t => t.BlockOrNull));
                        if (tile.sqaure6.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 3) result.AddRange(tile.sqaure6.Select(t => t.BlockOrNull));
                        
                        if (tile.line1.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 2) result.AddRange(tile.line1.Select(t => t.BlockOrNull));
                        if (tile.line2.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 2) result.AddRange(tile.line2.Select(t => t.BlockOrNull));
                        if (tile.line3.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 2) result.AddRange(tile.line3.Select(t => t.BlockOrNull));
                        if (tile.line4.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 2) result.AddRange(tile.line4.Select(t => t.BlockOrNull));
                        if (tile.line5.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 2) result.AddRange(tile.line5.Select(t => t.BlockOrNull));
                        if (tile.line6.Count(t => t.BlockOrNull != null && block.Type == t.BlockOrNull.Type) == 2) result.AddRange(tile.line6.Select(t => t.BlockOrNull));
                    }
                
                _destroyBlocks.AddRange(result.Distinct());
                
                _destroyBlocks.AddRange(_destroyBlocks.Select(destroyBlock => destroyBlock.GetTile())
                    .SelectMany(t => t.near)
                    .Where(t => t.BlockOrNull != null && t.BlockOrNull.Type == BlockType.SpinningTop)
                    .Select(t => t.BlockOrNull)
                    .Distinct()
                    .Where((t, i) => i < _destroyBlocks.Count)
                );
            }
            
            return _destroyBlocks;
        }
        
        #region Helper
        private Block InstantiateBlock(Vector3 position)
        {
            var block = Instantiate(defaultBlock, position, Quaternion.identity, blockGroupsWrapper).GetComponent<Block>();
            block.Type = (BlockType)Random.Range((int)BlockType.Red, Block.TypeMaxIndex + 1);
            Blocks.Add(block);
            return block;
        }
        
        private void SwapTile(Block a, Block b)
        {
            var t = a.GetTile();
            a.SetTile(b.GetTile(), Block.MoveDuration);
            b.SetTile(t, Block.MoveDuration);
        }
        #endregion
    }
}