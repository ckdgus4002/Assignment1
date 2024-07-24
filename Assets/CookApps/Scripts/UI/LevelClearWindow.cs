using System;
using CookApps.Managers;
using DG.Tweening;
using LCHFramework.Utilities;
using UnityEngine;

namespace CookApps.UI
{
    public class LevelClearWindow : Window
    {
        [SerializeField] private WrapperInfo textWrapper;
        [SerializeField] private WrapperInfo panelWrapper;

        

        protected override void Awake()
        {
            base.Awake();

            textWrapper.GameObject = wrappers[0].gameObject;
            panelWrapper.GameObject = wrappers[1].gameObject;
        }
        
        
        
        // UnityEvent event.
        public void OnCloseButtonClick() => Show(State.Complete);
        
        // UnityEvent event.
        public void OnNextButtonClick() =>  Show(State.Complete);
        
        
        
        protected override async void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            var levelClearStep = Main.Instance.CurrentStep as LevelClearStep;
            var isLevelClearStep = levelClearStep != null; 
            textWrapper.SetActive(isLevelClearStep && state == State.Appear);
            panelWrapper.SetActive(isLevelClearStep && state == State.Complete);
            if (!isLevelClearStep) return;
            
            CancellationTokenSourceUtility.ClearTokenSources(_ctses);
            canvasGroup.interactable = state != State.Complete;
            switch (state)
            {
                case State.Appear:
                {
                    textWrapper.tweenWrapper.DOPivotY(.5f, 1.5f).From(new Vector2(.5f, 0));
                    textWrapper.tweenWrapper.DOMoveY(0, 1.5f).From(HalfHeight);
                    textWrapper.tweenWrapper.DOScale(1, 1.5f).From(0);
                    await Awaitable.WaitForSecondsAsync(1.5f);
                    
                    await Awaitable.WaitForSecondsAsync(2.25f);
                
                    textWrapper.tweenWrapper.DOMoveY(-Height, 2).From(0);
                    await Awaitable.WaitForSecondsAsync(1);
                
                    textWrapper.SetActive(false);
                    panelWrapper.SetActive(true);
                    panelWrapper.tweenWrapper.DOScale(1, 1f).SetEase(Ease.OutBack).From(0);
                    break;
                }
                case State.Default:
                    break;
                case State.Complete:
                {
                    panelWrapper.tweenWrapper.DOScale(1.15f, .5f).From(1).SetLoops(2, LoopType.Yoyo);
                    await Awaitable.WaitForSecondsAsync(1);
                    
                    canvasGroup.interactable = true;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        
        
        [Serializable]
        private struct WrapperInfo
        {
            public RectTransform tweenWrapper;
            
            public GameObject GameObject { private get; set; }

            public void SetActive(bool value) => GameObject.SetActive(value);
        }
    }
}