﻿using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FrostHelper
{
    public class SpeedChallengePage : CustomJournalPage
    {
        public SpeedChallengePage(CustomJournal journal, string[] challenges) : base(journal)
        {
            PageTexture = "page";
            Vector2 justify = new Vector2(0.5f, 0.5f);
            float num = 0.5f;
            Color color = Color.Black * 0.6f;
            table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_speedruns", null), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f, 0f, false)).AddColumn(new TextCell(Dialog.Clean("FH_PB", null), justify, num + 0.1f, color, 240f, false)).AddColumn(new TextCell(Dialog.Clean("FH_Goal", null), justify, num + 0.1f, color, 240f, false));

            for (int i = 0; i < challenges.Length; i++)
            {
                Logger.Log("", challenges[i]);
                long time = FrostModule.SaveData.GetChallengeTime(challenges[i]);
                long targetTime = TimeSpan.FromSeconds(FrostMapDataProcessor.SpeedChallenges[challenges[i]].GoalTime).Ticks;
                Row row = table.AddRow().Add(new TextCell(Dialog.Clean(challenges[i].Remove(0, challenges[i].IndexOf('>') +1)), new Vector2(1f, 0.5f), 0.6f, TextColor))
                    .Add(new TextCell(time == -1 ? "-" : TimeSpan.FromTicks(time).ShortGameplayFormat(), new Vector2(0.5f, 0.5f), 0.6f, time < targetTime ? Calc.HexToColor("B07A00") : TextColor)) // PB
                    .Add(new TextCell(TimeSpan.FromSeconds(FrostMapDataProcessor.SpeedChallenges[challenges[i]].GoalTime).ShortGameplayFormat(), new Vector2(0.5f, 0.5f), 0.6f, TextColor)); // Goal Time
            }
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            table.Render(new Vector2(60f, 20f));
            Draw.SpriteBatch.End();
        }

        private Table table;
    }

    public abstract class CustomJournalPage
    {
        public CustomJournalPage(CustomJournal journal)
        {
            this.TextJustify = new Vector2(0.5f, 0.5f);
            this.TextColor = Color.Black * 0.6f;
            this.Journal = journal;
        }

        public virtual void Redraw(VirtualRenderTarget buffer)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        }

        public virtual void Update()
        {
        }

        public const int PageWidth = 1610;

        public const int PageHeight = 1000;

        public readonly Vector2 TextJustify;

        public const float TextScale = 0.5f;

        public readonly Color TextColor;

        public int PageIndex;

        public string PageTexture;

        public CustomJournal Journal;

        public class Table
        {
            public int Rows
            {
                get
                {
                    return this.rows.Count;
                }
            }

            public CustomJournalPage.Row Header
            {
                get
                {
                    if (this.rows.Count <= 0)
                    {
                        return null;
                    }
                    return this.rows[0];
                }
            }

            public CustomJournalPage.Table AddColumn(CustomJournalPage.Cell label)
            {
                if (this.rows.Count == 0)
                {
                    this.AddRow();
                }
                this.rows[0].Add(label);
                return this;
            }

            public CustomJournalPage.Row AddRow()
            {
                CustomJournalPage.Row row = new CustomJournalPage.Row();
                this.rows.Add(row);
                return row;
            }

            public float Height()
            {
                return 100f + 60f * (float)(this.rows.Count - 1);
            }

            public void Render(Vector2 position)
            {
                if (this.Header == null)
                {
                    return;
                }
                float num = 0f;
                float num2 = 0f;
                for (int i = 0; i < this.Header.Count; i++)
                {
                    num2 += this.Header[i].Width() + 20f;
                }
                for (int j = 0; j < this.Header.Count; j++)
                {
                    float num3 = this.Header[j].Width();
                    this.Header[j].Render(position + new Vector2(num + num3 * 0.5f, 40f), num3);
                    int num4 = 1;
                    float num5 = 130f;
                    for (int k = 1; k < this.rows.Count; k++)
                    {
                        Vector2 vector = position + new Vector2(num + num3 * 0.5f, num5);
                        if (this.rows[k].Count > 0)
                        {
                            if (num4 % 2 == 0)
                            {
                                Draw.Rect(vector.X - num3 * 0.5f, vector.Y - 27f, num3 + 20f, 54f, Color.Black * 0.08f);
                            }
                            if (j < this.rows[k].Count && this.rows[k][j] != null)
                            {
                                CustomJournalPage.Cell cell = this.rows[k][j];
                                if (cell.SpreadOverColumns > 1)
                                {
                                    for (int l = j + 1; l < j + cell.SpreadOverColumns; l++)
                                    {
                                        vector.X += this.Header[l].Width() * 0.5f;
                                    }
                                    vector.X += (float)(cell.SpreadOverColumns - 1) * 20f * 0.5f;
                                }
                                this.rows[k][j].Render(vector, num3);
                            }
                            num4++;
                            num5 += 60f;
                        }
                        else
                        {
                            Draw.Rect(vector.X - num3 * 0.5f, vector.Y - 25.5f, num3 + 20f, 6f, Color.Black * 0.3f);
                            num5 += 15f;
                        }
                    }
                    num += num3 + 20f;
                }
            }

            public Table()
            {
                rows = new List<Row>();
            }

            private const float headerHeight = 80f;

            private const float headerBottomMargin = 20f;

            private const float rowHeight = 60f;

            private List<Row> rows;
        }

        public class Row
        {
            public Row Add(Cell entry)
            {
                this.Entries.Add(entry);
                return this;
            }

            public int Count
            {
                get
                {
                    return this.Entries.Count;
                }
            }

            public Cell this[int index]
            {
                get
                {
                    return this.Entries[index];
                }
            }

            public Row()
            {
                this.Entries = new List<Cell>();
            }

            public List<CustomJournalPage.Cell> Entries;
        }

        public abstract class Cell
        {
            public virtual float Width()
            {
                return 0f;
            }

            public virtual void Render(Vector2 center, float columnWidth)
            {
            }

            protected Cell()
            {
                this.SpreadOverColumns = 1;
            }

            public int SpreadOverColumns;
        }

        public class EmptyCell : CustomJournalPage.Cell
        {
            public EmptyCell(float width)
            {
                this.width = width;
            }

            public override float Width()
            {
                return this.width;
            }

            private float width;
        }

        public class TextCell : CustomJournalPage.Cell
        {
            public TextCell(string text, Vector2 justify, float scale, Color color, float width = 0f, bool forceWidth = false)
            {
                this.text = text;
                this.justify = justify;
                this.scale = scale;
                this.color = color;
                this.width = width;
                this.forceWidth = forceWidth;
            }

            public override float Width()
            {
                if (this.forceWidth)
                {
                    return this.width;
                }
                return Math.Max(this.width, ActiveFont.Measure(this.text).X * this.scale);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num = ActiveFont.Measure(this.text).X * this.scale;
                float scaleFactor = 1f;
                if (!this.forceWidth && num > columnWidth)
                {
                    scaleFactor = columnWidth / num;
                }
                ActiveFont.Draw(this.text, center + new Vector2(-columnWidth / 2f + columnWidth * this.justify.X, 0f), this.justify, Vector2.One * this.scale * scaleFactor, this.color);
            }

            private string text;

            private Vector2 justify;

            private float scale;

            private Color color;

            private float width;

            private bool forceWidth;
        }

        public class IconCell : CustomJournalPage.Cell
        {
            public IconCell(string icon, float width = 0f)
            {
                this.icon = icon;
                this.width = width;
            }

            public override float Width()
            {
                return Math.Max((float)MTN.Journal[this.icon].Width, this.width);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                MTN.Journal[this.icon].DrawCentered(center);
            }

            private string icon;

            private float width;
        }

        public class IconsCell : CustomJournalPage.Cell
        {
            public IconsCell(float iconSpacing, params string[] icons)
            {
                this.iconSpacing = 4f;
                this.iconSpacing = iconSpacing;
                this.icons = icons;
            }

            public IconsCell(params string[] icons)
            {
                this.iconSpacing = 4f;
                this.icons = icons;
            }

            public override float Width()
            {
                float num = 0f;
                for (int i = 0; i < this.icons.Length; i++)
                {
                    num += (float)MTN.Journal[this.icons[i]].Width;
                }
                return num + (float)(this.icons.Length - 1) * this.iconSpacing;
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num = this.Width();
                Vector2 position = center + new Vector2(-num * 0.5f, 0f);
                for (int i = 0; i < this.icons.Length; i++)
                {
                    MTexture mtexture = MTN.Journal[this.icons[i]];
                    mtexture.DrawJustified(position, new Vector2(0f, 0.5f));
                    position.X += (float)mtexture.Width + this.iconSpacing;
                }
            }

            private float iconSpacing;

            private string[] icons;
        }
    }

    public class CustomJournal : Entity
    {
        public CustomJournalPage Page
        {
            get
            {
                return this.Pages[this.PageIndex];
            }
        }

        public CustomJournalPage NextPage
        {
            get
            {
                return this.Pages[this.PageIndex + 1];
            }
        }

        public CustomJournalPage PrevPage
        {
            get
            {
                return this.Pages[this.PageIndex - 1];
            }
        }

        public IEnumerator Enter()
        {
            PageIndex = 0;
            Visible = true;
            X = -1920f;
            this.turningPage = false;
            this.turningScale = 1f;
            this.rotation = 0f;
            this.dot = 0f;
            this.dotTarget = 0f;
            this.dotEase = 0f;
            this.leftArrowEase = 0f;
            this.rightArrowEase = 0f;
            this.NextPageBuffer = VirtualContent.CreateRenderTarget("journal-a", 1610, 1000, false, true, 0);
            this.CurrentPageBuffer = VirtualContent.CreateRenderTarget("journal-b", 1610, 1000, false, true, 0);

            int num = 0;
            foreach (CustomJournalPage CustomJournalPage in this.Pages)
            {
                CustomJournalPage.PageIndex = num++;
            }
            this.Pages[0].Redraw(this.CurrentPageBuffer);
            //this.cameraStart = this.Overworld.Mountain.UntiltedCamera;
            //this.cameraEnd = this.cameraStart;
            //this.cameraEnd.Position = this.cameraEnd.Position + -this.cameraStart.Rotation.Forward() * 1f;
            //this.Overworld.Mountain.EaseCamera(this.Overworld.Mountain.Area, this.cameraEnd, new float?(2f), true, false);
            //this.Overworld.Mountain.AllowUserRotation = false;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.4f)
            {
                this.rotation = -0.025f * Ease.BackOut(p);
                this.X = -1920f + 1920f * Ease.CubeInOut(p);
                this.dotEase = p;
                yield return null;
            }
            this.dotEase = 1f;
            yield break;
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
            if (this.Pages.Count > 0)
            {
                this.Page.Redraw(this.CurrentPageBuffer);
            }
        }

        public IEnumerator TurnPage(int direction)
        {
            this.turningPage = true;
            if (direction < 0)
            {
                this.PageIndex--;
                this.turningScale = -1f;
                this.dotTarget -= 1f;
                this.Page.Redraw(this.CurrentPageBuffer);
                this.NextPage.Redraw(this.NextPageBuffer);
                while ((this.turningScale = Calc.Approach(this.turningScale, 1f, Engine.DeltaTime * 8f)) < 1f)
                {
                    yield return null;
                }
            }
            else
            {
                this.NextPage.Redraw(this.NextPageBuffer);
                this.turningScale = 1f;
                this.dotTarget += 1f;
                while ((this.turningScale = Calc.Approach(this.turningScale, -1f, Engine.DeltaTime * 8f)) > -1f)
                {
                    yield return null;
                }
                this.PageIndex++;
                this.Page.Redraw(this.CurrentPageBuffer);
            }
            this.turningScale = 1f;
            this.turningPage = false;
            yield break;
        }

        public IEnumerator Leave()
        {
            Audio.Play("event:/ui/world_map/journal/back");
           // this.Overworld.Mountain.EaseCamera(this.Overworld.Mountain.Area, this.cameraStart, new float?(0.4f), true, false);
            //UserIO.SaveHandler(false, true);
            yield return this.EaseOut(0.4f);
            //while (UserIO.Saving)
            //{
            //    yield return null;
            //}
            CurrentPageBuffer.Dispose();
            NextPageBuffer.Dispose();

            Pages.Clear();
            Visible = false;
            Active = false;
            yield break;
        }

        private IEnumerator EaseOut(float duration)
        {
            float rotFrom = this.rotation;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                this.rotation = rotFrom * (1f - Ease.BackOut(p));
                this.X = 0f + -1920f * Ease.CubeInOut(p);
                this.dotEase = 1f - p;
                yield return null;
            }
            this.dotEase = 0f;
            yield break;
        }

        public override void Update()
        {
            base.Update();
            this.dot = Calc.Approach(this.dot, this.dotTarget, Engine.DeltaTime * 8f);
            this.leftArrowEase = Calc.Approach(this.leftArrowEase, (float)((this.dotTarget > 0f) ? 1 : 0), Engine.DeltaTime * 5f) * this.dotEase;
            this.rightArrowEase = Calc.Approach(this.rightArrowEase, (float)((this.dotTarget < (float)(this.Pages.Count - 1)) ? 1 : 0), Engine.DeltaTime * 5f) * this.dotEase;
            if (!turningPage && Pages.Count > 0)
            {
                this.Page.Update();
                if (!this.PageTurningLocked)
                {
                    if (Input.MenuLeft.Pressed && this.PageIndex > 0)
                    {
                        if (this.PageIndex == 1)
                        {
                            Audio.Play("event:/ui/world_map/journal/page_cover_back");
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/journal/page_main_back");
                        }
                        base.Add(new Coroutine(this.TurnPage(-1), true));
                    }
                    else if (Input.MenuRight.Pressed && this.PageIndex < this.Pages.Count - 1)
                    {
                        if (this.PageIndex == 0)
                        {
                            Audio.Play("event:/ui/world_map/journal/page_cover_forward");
                        }
                        else
                        {
                            Audio.Play("event:/ui/world_map/journal/page_main_forward");
                        }
                        base.Add(new Coroutine(this.TurnPage(1), true));
                    }
                }
                if (!this.PageTurningLocked && (Input.MenuJournal.Pressed || Input.MenuCancel.Pressed))
                {
                    this.Close();
                }
            }
        }

        private void Close()
        {
            //base.Overworld.Goto<OuiChapterSelect>();
        }

        public override void Render()
        {
            Vector2 vector = this.Position + new Vector2(128f, 120f);
            float num = Ease.CubeInOut(Math.Max(0f, this.turningScale));
            float num2 = Ease.CubeInOut(Math.Abs(Math.Min(0f, this.turningScale)));
            if (SaveData.Instance.CheatMode)
            {
                MTN.FileSelect["cheatmode"].DrawCentered(vector + new Vector2(80f, 360f), Color.White, 1f, 1.57079637f);
            }
            if (SaveData.Instance.AssistMode)
            {
                MTN.FileSelect["assist"].DrawCentered(vector + new Vector2(100f, 370f), Color.White, 1f, 1.57079637f);
            }
            MTexture mtexture = MTN.Journal["edge"];
            mtexture.Draw(vector + new Vector2((float)(-(float)mtexture.Width), 0f), Vector2.Zero, Color.White, 1f, this.rotation);
            if (this.PageIndex > 0)
            {
                MTN.Journal[this.PrevPage.PageTexture].Draw(vector, Vector2.Zero, this.backColor, new Vector2(-1f, 1f), this.rotation);
            }
            if (this.turningPage)
            {
                MTN.Journal[this.NextPage.PageTexture].Draw(vector, Vector2.Zero, Color.White, 1f, this.rotation);
                Draw.SpriteBatch.Draw(this.NextPageBuffer, vector, new Rectangle?(this.NextPageBuffer.Bounds), Color.White, this.rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            }
            if (this.turningPage && num2 > 0f)
            {
                MTN.Journal[this.Page.PageTexture].Draw(vector, Vector2.Zero, this.backColor, new Vector2(-1f * num2, 1f), this.rotation);
            }
            if (num > 0f)
            {
                MTN.Journal[this.Page.PageTexture].Draw(vector, Vector2.Zero, Color.White, new Vector2(num, 1f), this.rotation);
                //Page.Redraw(CurrentPageBuffer);
                Draw.SpriteBatch.Draw(this.CurrentPageBuffer, vector, new Rectangle?(this.CurrentPageBuffer.Bounds), Color.White, this.rotation, Vector2.Zero, new Vector2(num, 1f), SpriteEffects.None, 0f);
            }
            if (this.Pages.Count > 0)
            {
                int count = this.Pages.Count;
                MTexture mtexture2 = GFX.Gui["dot_outline"];
                int num3 = mtexture2.Width * count;
                Vector2 value = new Vector2(960f, 1040f - 40f * Ease.CubeOut(this.dotEase));
                for (int i = 0; i < count; i++)
                {
                    mtexture2.DrawCentered(value + new Vector2((float)(-(float)num3 / 2) + (float)mtexture2.Width * ((float)i + 0.5f), 0f), Color.White * 0.25f);
                }
                float x = 1f + Calc.YoYo(this.dot % 1f) * 4f;
                mtexture2.DrawCentered(value + new Vector2((float)(-(float)num3 / 2) + (float)mtexture2.Width * (this.dot + 0.5f), 0f), Color.White, new Vector2(x, 1f));
                GFX.Gui["dotarrow_outline"].DrawCentered(value + new Vector2((float)(-(float)num3 / 2 - 50), 32f * (1f - Ease.CubeOut(this.leftArrowEase))), Color.White * this.leftArrowEase, new Vector2(-1f, 1f));
                GFX.Gui["dotarrow_outline"].DrawCentered(value + new Vector2((float)(num3 / 2 + 50), 32f * (1f - Ease.CubeOut(this.rightArrowEase))), Color.White * this.rightArrowEase);
            }
        }

        public CustomJournal() : base()
        {
            //Pages = pages;
            backColor = Color.Lerp(Color.White, Color.Black, 0.2f);
            arrow = MTN.Journal["pageArrow"];
            Tag = Tags.HUD;
            Visible = true;
        }

        private const float onScreenX = 0f;

        private const float offScreenX = -1920f;

        public bool PageTurningLocked;

        public List<CustomJournalPage> Pages;

        public int PageIndex;

        public VirtualRenderTarget CurrentPageBuffer;

        public VirtualRenderTarget NextPageBuffer;

        private bool turningPage;

        private float turningScale;

        private Color backColor;

        private float rotation;

        private MTexture arrow;

        private float dot;

        private float dotTarget;

        private float dotEase;

        private float leftArrowEase;

        private float rightArrowEase;
    }
}
