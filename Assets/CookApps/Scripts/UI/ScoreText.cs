using System.Linq;
using CookApps.Managers;
using LCHFramework.Managers;
using TMPro;
using UnityEngine;

namespace CookApps.UI
{
    public class ScoreText : MonoSingleton<ScoreText>
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
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!Main.InstanceIsNull) Main.Instance.OnCurrentStepIndexChanged -= OnCurrentStepIndexChanged;
        }
        
        
        
        private void OnCurrentStepIndexChanged(int prevStepIndex, int currentStepIndex) => Show(prevStepIndex, currentStepIndex, State.Appear);

        private void OnScoreChanged(int prevValue, int currentValue) => text.text = $"{currentValue}";
        
        public void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            var currentGameStepOrNull = Main.Instance.CurrentStep as GameStep;
            var prevGameStepOrNull = Main.Instance.PrevStepOrNull as GameStep;
            wrapper.SetActive(currentGameStepOrNull != null);
            if (currentGameStepOrNull != null)
            {
                text.text = $"{currentGameStepOrNull.Score.Value}";
                currentGameStepOrNull.Score.OnValueChanged += OnScoreChanged;
            }
            if (prevGameStepOrNull != null)
                prevGameStepOrNull.Score.OnValueChanged -= OnScoreChanged;
        }
    }
}

