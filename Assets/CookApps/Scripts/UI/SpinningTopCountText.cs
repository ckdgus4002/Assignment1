using System.Linq;
using CookApps.Managers;
using LCHFramework.Managers;
using TMPro;
using UnityEngine;

namespace CookApps.UI
{
    public class SpinningTopCountText : MonoSingleton<SpinningTopCountText>
    {
        private GameObject wrapper;
        private TMP_Text text;
        
        
        public override bool IsShown => wrapper.activeInHierarchy;
        
        
        
        protected override void Awake()
        {
            base.Awake();

            wrapper = RectTransformOrNull.GetChild(0).gameObject;
            text = wrapper.GetComponentsInChildren<TMP_Text>().Last();
        }

        protected override void Start()
        {
            Main.Instance.OnCurrentStepIndexChanged += OnCurrentStepIndexChanged;
        }
        
        private void OnDestroy()
        {
            if (Main.Instance != null) Main.Instance.OnCurrentStepIndexChanged -= OnCurrentStepIndexChanged;
        }
        
        
        
        private void OnCurrentStepIndexChanged(int prevStepIndex, int currentStepIndex) => Show(prevStepIndex, currentStepIndex, State.Appear);

        private void OnSpinningTopCountChanged(int prevValue, int currentValue) => text.text = 0 < currentValue ? $"{currentValue}" : "Clear";
        
        public void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            var currentGameStepOrNull = Main.Instance.CurrentStep as GameStep;
            var prevGameStepOrNull = Main.Instance.PrevStepOrNull as GameStep;
            wrapper.SetActive(currentGameStepOrNull != null);
            if (currentGameStepOrNull != null)
            {
                text.text = $"{currentGameStepOrNull.spinningTopCount.Value}";
                currentGameStepOrNull.spinningTopCount.OnValueChanged += OnSpinningTopCountChanged;
            }
            if (prevGameStepOrNull != null)
                prevGameStepOrNull.spinningTopCount.OnValueChanged -= OnSpinningTopCountChanged;
        }
    }
}