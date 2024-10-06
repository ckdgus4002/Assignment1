using System.Linq;
using CookApps.Managers;
using LCHFramework.Components;
using LCHFramework.Extensions;
using UnityEngine;

namespace CookApps.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Window : LCHMonoBehaviour
    {
        protected CanvasGroup canvasGroup;
        protected RectTransform[] wrappers;
        
        
        public override bool IsShown => wrappers.Any(t => t.gameObject.activeInHierarchy);

        protected RectTransform Wrapper => wrappers[0];
        
        
        
        protected override void Awake()
        {
            base.Awake();

            canvasGroup = GetComponent<CanvasGroup>();
            wrappers = transform.GetChildren().Select(t => (RectTransform)t).ToArray();
        }
        
        protected override void Start()
        {
            base.Start();

            Main.Instance.OnCurrentStepIndexChanged += OnCurrentStepIndexChanged;
        }

        private void OnDestroy()
        {
            if (!Main.InstanceIsNull) Main.Instance.OnCurrentStepIndexChanged -= OnCurrentStepIndexChanged;
        }
        
        
        
        private void OnCurrentStepIndexChanged(int prevStepIndex, int currentStepIndex) => Show(State.Appear);

        protected void Show(State state) => Show(Main.Instance.PrevStepIndex, Main.Instance.CurrentStepIndex, state);
        
        protected abstract void Show(int prevStepIndex, int currentStepIndex, State state);
    }
}