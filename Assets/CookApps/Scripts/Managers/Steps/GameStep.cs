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
        public ReactiveProperty<int> spinningTopCount = new(10 * 5);
        public ReactiveProperty<int> moveCount = new(20 * 5);
        public ReactiveProperty<int> score = new();
        private int defaultSpinningTopCount;
        private int defaultMoveCount;
        
        

        protected override void Awake()
        {
            base.Awake();

            defaultSpinningTopCount = spinningTopCount.Value;
            defaultMoveCount = moveCount.Value;
        }

        protected override void Start()
        {
            base.Start();

            BlockManager.Instance.onDestroyBlockBegin += OnDestroyBlockBegin;
            BlockManager.Instance.onBlockDragEnd += OnBlockDragEnd;
        }
        
        private void OnDestroy()
        {
            if (BlockManager.Instance != null)
            {
                BlockManager.Instance.onDestroyBlockBegin -= OnDestroyBlockBegin;
                BlockManager.Instance.onBlockDragEnd -= OnBlockDragEnd;
            }
        }



        private async void OnDestroyBlockBegin(List<BlockType> destroyBlocks)
        {
            var destroyedSpinningTopCount = destroyBlocks.Count(t => t == BlockType.SpinningTop);
            Debug.Log($"{Time.frameCount}/ {destroyedSpinningTopCount}");
            spinningTopCount.Value = Mathf.Max(0, spinningTopCount.Value - destroyedSpinningTopCount);
            score.Value += destroyBlocks.Sum(t => t != BlockType.SpinningTop ? 20 : 500);
            if (0 < spinningTopCount.Value) return;

            await Awaitable.WaitForSecondsAsync(Block.DestroyJumpDuration);
            Main.Instance.CurrentStep = Main.Instance.LastStep;
        }

        private void OnBlockDragEnd()
        {
            moveCount.Value = Mathf.Max(0, moveCount.Value - 1);
            if (0 < moveCount.Value) return;
            
            Main.Instance.PassCurrentStep();
        }
        
        public override async void Show()
        {
            base.Show();

            if (Main.Instance.playOnStart.stepOrNull == this) return;

            Toast.Instance.Show("Woohoo! Its a spinning TOP!\nMatch blocks around it to spin\nit!");
            spinningTopCount.Value = defaultSpinningTopCount;
            moveCount.Value = defaultMoveCount;
            score.Value = 0;
            await Awaitable.WaitForSecondsAsync(Toast.DefaultDuration);
        }

        public override void Hide()
        {
            Toast.Instance.Hide();
            
            base.Hide();
        }
    }
}