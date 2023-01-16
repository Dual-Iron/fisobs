﻿using Fisobs.Core;
using Menu;
using RWCustom;
using System.Linq;
using UnityEngine;
using static Menu.SandboxSettingsInterface;

namespace Fisobs.Sandbox;

public sealed partial class SandboxRegistry : Registry
{
    // Per-controller offsets. pos = offset * index
    const float xOffset = 88.666f + 0.01f;
    const float yOffset = -30f;

    // Cross-fisobs compatibility
    const int paginatorVersion = 0;
    const string paginatorKey = "paginator";
    const int paginatorKeyLength = 9; // paginatorKey.Length

    sealed class PageButton : SymbolButton
    {
        public readonly int dir;

        public PageButton(MenuObject owner, int dir, Vector2 pos) : base(owner.menu, owner, "Menu_Symbol_Arrow", "", pos)
        {
            this.dir = dir;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            symbolSprite.rotation = dir * 90 + 90;
        }
    }

    sealed class Paginator : PositionedMenuObject
    {
        // VANILLA NOTES
        // There are 36 slots to display entries. 35 of them have ScoreControllers occupying them.
        // 32 of them [0..32)  are page entries. They may move.
        //  1 of them (32..32) is NULL. Exclude it from count.
        //  3 of them [32..36) are food, survival, and spearhit scores. They should never move.

        new private readonly SandboxSettingsInterface owner;
        private readonly PageButton up;
        private readonly PageButton down;

        const int Columns = 3;
        const int RowsDisplayed = 9;
        const int RowMin = 0;

        int Rows => Mathf.CeilToInt((owner.scoreControllers.Count - 3) / (float)Columns); // ceil(slots_used / column_count)
        int RowMax => Rows - RowsDisplayed;

        int rowOffset;
        float rowSmoothed;

        public Paginator(SandboxSettingsInterface owner, Vector2 pos) : base(owner.menu, owner, pos)
        {
            this.owner = owner;

            float xOffset = 88.666f + 0.01f;

            subObjects.Add(up = new PageButton(this, -1, new(xOffset * 3f, yOffset * 0f)));
            subObjects.Add(down = new PageButton(this, 1, new(xOffset * 3f, yOffset * 1f)));
        }

        public override string ToString() => $"{paginatorKey}{paginatorVersion}";

        public override void Update()
        {
            up.GetButtonBehavior.greyedOut = rowOffset == RowMin;
            down.GetButtonBehavior.greyedOut = rowOffset == RowMax;

            rowSmoothed = Custom.LerpAndTick(rowSmoothed, rowOffset, 1f / 10f, 1f / 40f);

            base.Update();
        }

        public override void Singal(MenuObject sender, string message)
        {
            // Pressed a page button
            if (sender is PageButton pageButton && rowOffset + pageButton.dir >= RowMin && rowOffset + pageButton.dir <= RowMax) {
                rowOffset += pageButton.dir;

                menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            int rows = Rows;

            // Fix position of creature scores
            int i = -1;
            foreach (ScoreController score in owner.scoreControllers.Where(s => s is SandboxSettingsInterface.KillScore or LockedScore)) {
                i++;

                int x = i / rows;
                int y = i % rows;

                if (x > 2) {
                    continue;
                }

                score.pos.x = x * xOffset;
                score.pos.y = y * yOffset - rowSmoothed * yOffset;

                float offsetToRowMin = 0                 - (y - rowSmoothed);
                float offsetToRowMax = RowsDisplayed - 1 - (y - rowSmoothed);
                float alpha = 1 - Mathf.Max(offsetToRowMin, -offsetToRowMax);

                SetAlpha(score, Mathf.Pow(Mathf.Clamp01(alpha), 2));

                if (alpha < 0.99f) {
                    score.page.selectables.Remove(score.scoreDragger);
                } else if (score.page.selectables.LastIndexOf(score.scoreDragger) == -1) {
                    score.page.selectables.Add(score.scoreDragger);
                }
            }

            // Fix position of misc scores
            float miscX = 266f;
            float miscY = -180f;
            foreach (ScoreController score in owner.scoreControllers.OfType<MiscScore>()) {
                score.pos.x = miscX;
                score.pos.y = miscY;
                miscY -= 30;
            }

            base.GrafUpdate(timeStacker);
        }

        void SetAlpha(ScoreController score, float alpha)
        {
            if (score?.scoreDragger == null) {
                return;
            }

            score.scoreDragger.buttonBehav.greyedOut = alpha < 0.5f;
            score.scoreDragger.label.label.alpha = alpha;
            foreach (var sprite in score.scoreDragger.roundedRect.sprites) {
                sprite.alpha = alpha;
            }

            if (score is LockedScore locked) {
                if (locked.shadowSprite1 != null) locked.shadowSprite1.alpha = alpha;
                if (locked.shadowSprite2 != null) locked.shadowSprite2.alpha = alpha;
                if (locked.symbolSprite != null) locked.symbolSprite.alpha = alpha;
            } else if (score is SandboxSettingsInterface.KillScore kill && kill.symbol != null) {
                if (kill.symbol.shadowSprite1 != null) kill.symbol.shadowSprite1.alpha = alpha;
                if (kill.symbol.shadowSprite2 != null) kill.symbol.shadowSprite2.alpha = alpha;
                if (kill.symbol.symbolSprite != null) kill.symbol.symbolSprite.alpha = alpha;
            }
        }
    }
}
