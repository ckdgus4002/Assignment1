using System.Linq;
using LCHFramework.Managers;
using LCHFramework.Utilities;
using TMPro;
using UnityEngine;

namespace CookApps.UI
{
    public class Toast : MonoSingleton<Toast>
    {
        public const float DefaultDuration = 3;
        
        
        
        private GameObject wrapper;
        private TMP_Text text;


        public override bool IsShown => wrapper.activeInHierarchy;

        
        
        protected override void Awake()
        {
            base.Awake();

            wrapper = RectTransformOrNull.GetChild(0).gameObject;
            text = wrapper.GetComponentsInChildren<TMP_Text>().Last();
        }
        
        
        
        public async void Show(string message, float duration = DefaultDuration)
        {
            CancellationTokenSourceUtility.ClearTokenSources(_ctses);
            
            wrapper.SetActive(true);
            text.text = message;
            await Awaitable.WaitForSecondsAsync(duration);
            
            Hide();
        }
        
        public void Hide() => wrapper.SetActive(false);
    }
}