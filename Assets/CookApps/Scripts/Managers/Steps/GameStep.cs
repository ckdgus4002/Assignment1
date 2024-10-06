using System.Collections.Generic;
using System.Linq;
using CookApps.UI;
using LCHFramework.Data;
using LCHFramework.Managers;
using UnityEngine;
using Debug = LCHFramework.Utilities.Debug;

namespace CookApps.Managers
{
    public class GameStep : Step
    {
        private int defaultSpinningTopCount;
        private int defaultMoveCount;
        
        
        public ReactiveProperty<int> SpinningTopCount { get; } = new(10);
        public ReactiveProperty<int> MoveCount { get; }= new(20);
        public ReactiveProperty<int> Score { get; } = new();
        
        
        
        protected override void Awake()
        {
            base.Awake();

            defaultSpinningTopCount = SpinningTopCount.Value;
            defaultMoveCount = MoveCount.Value;
        }

        protected override void Start()
        {
            base.Start();

            BlockManager.Instance.onDestroyBlockBegin += OnDestroyBlockBegin;
            BlockManager.Instance.onBlockDragEnd += OnBlockDragEnd;
        }
        
        private void OnDestroy()
        {
            if (BlockManager.InstanceIsNull)
            {
                BlockManager.Instance.onDestroyBlockBegin -= OnDestroyBlockBegin;
                BlockManager.Instance.onBlockDragEnd -= OnBlockDragEnd;
            }
        }



        private async void OnDestroyBlockBegin(List<BlockType> destroyBlocks)
        {
            var destroyedSpinningTopCount = destroyBlocks.Count(t => t == BlockType.SpinningTop);
            Debug.Log($"{Time.frameCount}/ {destroyedSpinningTopCount}");
            SpinningTopCount.Value = Mathf.Max(0, SpinningTopCount.Value - destroyedSpinningTopCount);
            Score.Value += destroyBlocks.Sum(t => t != BlockType.SpinningTop ? 20 : 500);
            if (0 < SpinningTopCount.Value) return;

            await Awaitable.WaitForSecondsAsync(Block.DestroyJumpDuration);
            Main.Instance.CurrentStep = Main.Instance.LastStep;
        }

        private void OnBlockDragEnd()
        {
            MoveCount.Value = Mathf.Max(0, MoveCount.Value - 1);
            if (0 < MoveCount.Value) return;
            
            Main.Instance.PassCurrentStep();
        }
        
        public override async void Show()
        {
            base.Show();

            if (Main.Instance.playOnStart.stepOrNull == this) return;

            Toast.Instance.Show("Woohoo! Its a spinning TOP!\nMatch blocks around it to spin\nit!");
            SpinningTopCount.Value = defaultSpinningTopCount;
            MoveCount.Value = defaultMoveCount;
            Score.Value = 0;
            await Awaitable.WaitForSecondsAsync(Toast.DefaultDuration);
        }

        public override void Hide()
        {
            Toast.Instance.Hide();
            
            base.Hide();
        }
    }
}