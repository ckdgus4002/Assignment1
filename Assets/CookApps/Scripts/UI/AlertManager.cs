using System;
using DG.Tweening;
using LCHFramework.Managers;
using LCHFramework.Utilities;
using UnityEngine;

namespace CookApps.UI
{
    public enum AlertType
    {
        Fun = 1,
    }
    
    public class AlertManager : MonoSingleton<AlertManager>
    {
        [SerializeField] private Wrapper funWrapper;
        
        
        public override bool IsShown => funWrapper.gameObject.activeInHierarchy;
        
        
        
        protected override void Awake()
        {
            base.Awake();
            
            funWrapper.gameObject = RectTransformOrNull.Find("FunWrapper").gameObject;
        }
        
        
        
        public async void Show(AlertType alertType)
        {
            CancellationTokenSourceUtility.ClearTokenSources(_ctses);
            
            funWrapper.gameObject.SetActive(alertType == AlertType.Fun);
            switch (alertType)
            {
                case AlertType.Fun:
                {
                    funWrapper.tweenWrapper.DOPivotY(0.5f, 2).From(new Vector2(.5f, 1));
                    funWrapper.tweenWrapper.DOMoveY(0, 2).From(HalfHeight);
                    funWrapper.alphaWrapper.alpha = 1;
                    await Awaitable.WaitForSecondsAsync(4);
                
                    funWrapper.tweenWrapper.DOMoveY(0, 2).From(HalfHeight);
                    funWrapper.alphaWrapper.DOFade(0, 2).From(1);
                    await Awaitable.WaitForSecondsAsync(2);
                
                    funWrapper.gameObject.SetActive(false);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(alertType), alertType, null);
            }
        }
        
        
        
        [Serializable]
        private struct Wrapper
        {
            public RectTransform tweenWrapper;
            public CanvasGroup alphaWrapper;

            [NonSerialized] public GameObject gameObject;
        }
    }
}