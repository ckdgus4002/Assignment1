using System;
using CookApps.Managers;
using DG.Tweening;
using LCHFramework.Extensions;
using LCHFramework.Utilities;
using UnityEngine;

namespace CookApps.UI
{
    public class LevelFailWindow : Window
    {
        [SerializeField] private RectTransform tweenWrapper;
        
        
        
        // UnityEvent event.
        public void OnClick() => Show(State.Complete);
        
        
        
        protected override async void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            var levelFailStep = Main.Instance.CurrentStep as LevelFailStep;
            var isLevelFailStep = levelFailStep != null;
            Wrapper.SetActive(isLevelFailStep);
            if (!isLevelFailStep) return;
            
            CancellationTokenSourceUtility.ClearTokenSources(_ctses);
            canvasGroup.interactable = state != State.Complete;
            switch (state)
            {
                case State.Appear:
                {
                    tweenWrapper.DOScale(1, 1f).SetEase(Ease.OutBack).From(0);
                    break;
                }
                case State.Default:
                    break;
                case State.Complete:
                {
                    tweenWrapper.DOScale(1.15f, .5f).From(1).SetLoops(2, LoopType.Yoyo);
                    await Awaitable.WaitForSecondsAsync(1);

                    canvasGroup.interactable = true;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}