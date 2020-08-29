﻿using Planes262.GameLogic;
using UnityEngine.UI;

namespace Planes262.UnityLayer
{
    public class UnityScore : Score
    {
        private readonly Text scoreText;

        public UnityScore(Text scoreText)
        {
            this.scoreText = scoreText;
        }

        public override void Increment(PlayerSide player)
        {
            base.Increment(player);
            scoreText.text = ToString();
        }

        public override void Reset()
        {
            base.Reset();
            scoreText.text = ToString();
        }
    }
}
