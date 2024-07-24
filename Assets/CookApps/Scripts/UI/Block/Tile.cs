using System.Linq;
using CookApps.Managers;
using DG.Tweening;
using LCHFramework.Components;
using LCHFramework.Extensions;
using TMPro;
using UnityEngine;

namespace CookApps.UI
{
    public class Tile : LCHMonoBehaviour
    {
        public int number;
        [SerializeField] public Tile[] sqaure1;
        [SerializeField] public Tile[] sqaure2;
        [SerializeField] public Tile[] sqaure3;
        [SerializeField] public Tile[] sqaure4;
        [SerializeField] public Tile[] sqaure5;
        [SerializeField] public Tile[] sqaure6;
        [SerializeField] public Tile[] sqaure7;
        [SerializeField] public Tile[] sqaure8;
        [SerializeField] public Tile[] line1;
        [SerializeField] public Tile[] line2;
        [SerializeField] public Tile[] line3;
        [SerializeField] public Tile[] line4;
        [SerializeField] public Tile[] line5;
        [SerializeField] public Tile[] line6;
        [SerializeField] public Tile[] under;
        [SerializeField] public Tile[] near;
        
        
        public (int, int) IJ
        {
            get
            {
                for (var i = 0; i < BlockManager.Instance.Tiles.Length; i++)
                    for (var j = 0; j < BlockManager.Instance.Tiles[i].Length; j++)
                        if (BlockManager.Instance.Tiles[i][j] == this)
                            return (i, j);

                return (-1, -1);
            }
        }
        
        public Block BlockOrNull => BlockManager.Instance.Blocks.FirstOrDefault(t => t.GetTile() == this);

        private TMP_Text Text => _text == null ? _text = GetComponentInChildren<TMP_Text>(true) : _text;
        private TMP_Text _text;
        
        

        private void OnValidate()
        {
            name = $"Tile ({number})";
            Text.text = $"{number}";
        }

        protected override void Awake()
        {
            base.Awake();
            
            Text.SetActive(false);
        }
        
        
        
        public void Show(int prevStepIndex, int currentStepIndex, State state)
        {
            if (Main.Instance.playOnStart.stepOrNull.Index == currentStepIndex && state == State.Appear)
            {
                RectTransformOrNull.DOScale(1, 1f).From(0);
            }
        }
    }
}