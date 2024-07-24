using System;
using CookApps.Managers;
using DG.Tweening;
using LCHFramework.Extensions;
using LCHFramework.Utilities;
using UnityEngine;

namespace CookApps.UI
{
    public class ToyTopsWindow : Window
    {
        private RectTransform imageWrapper;
        
        
        
        protected override void Awake()
        {
            base.Awake();

            imageWrapper = (RectTransform)Wrapper.GetChild(1);
        }

        // UnityEvent event.
        public void OnPlayButtonClick() => Show(Main.Instance.PrevStepIndex, Main.Instance.CurrentStepIndex, State.Complete);
        
        
        
        protected override async void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            var toyTopsStep = Main.Instance.CurrentStep as ToyTopsStep;
            var isToyTopsStep = toyTopsStep != null;
            Wrapper.SetActive(isToyTopsStep);
            if (!isToyTopsStep) return;
            
            CancellationTokenSourceUtility.ClearTokenSources(_ctses);
            canvasGroup.interactable = state != State.Complete;
            switch (state)
            {
                case State.Appear:
                {
                    imageWrapper.DOScale(1, 1).SetEase(Ease.OutBack).From(0);
                    break;
                }
                case State.Default:
                    break;
                case State.Complete:
                {
                    imageWrapper.DOScale(0, 1).From(1);
                    await Awaitable.WaitForSecondsAsync(.5f);
                
                    Main.Instance.PassCurrentStep();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
