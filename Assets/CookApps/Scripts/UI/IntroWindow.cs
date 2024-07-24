using CookApps.Managers;
using DG.Tweening;
using LCHFramework.Extensions;
using LCHFramework.Utilities;
using UnityEngine;

namespace CookApps.UI
{
    public class IntroWindow : Window
    {
        protected override async void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            var introStepOrNull = Main.Instance.CurrentStep as IntroStep;
            var isIntroStep = introStepOrNull != null;
            Wrapper.SetActive(isIntroStep);
            if (!isIntroStep) return;
            
            CancellationTokenSourceUtility.ClearTokenSources(_ctses);
            
            Wrapper.transform.DOMoveX(0, 1.5f).SetEase(Ease.OutBack).From(Width);
            await Awaitable.WaitForSecondsAsync(1.5f);
            
            await Awaitable.WaitForSecondsAsync(2.25f);
            
            Wrapper.transform.DOMoveX(-Width, 1.5f);
            await Awaitable.WaitForSecondsAsync(1.25f);
            
            Main.Instance.PassCurrentStep();
        }
    }
}

