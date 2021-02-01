using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Quests;

namespace MultipleMiniObelisks.UI
{
	public class TeleportMenu : IClickableMenu
	{
		public const int questsPerPage = 6;

		public const int region_forwardButton = 101;

		public const int region_backButton = 102;

		public const int region_rewardBox = 103;

		public const int region_cancelQuestButton = 104;

		private List<List<StardewValley.Object>> pages;

		public List<ClickableComponent> teleportDestinationButtons;
		public List<ClickableTextureComponent> renameObeliskButtons;

		private int currentPage;

		private int questPage = -1;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent backButton;

		protected IQuest _shownQuest;

		protected List<string> _objectiveText;

		protected float _contentHeight;

		protected float _scissorRectHeight;

		public float scrollAmount;

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		public ClickableTextureComponent scrollBar;

		private bool scrolling;

		public Rectangle scrollBarBounds;

		private string hoverText = "";

		private List<StardewValley.Object> miniObelisks = new List<StardewValley.Object>();

		public TeleportMenu(List<StardewValley.Object> miniObelisks) : base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			this.miniObelisks = miniObelisks;

			Game1.dayTimeMoneyBox.DismissQuestPing();
			Game1.playSound("bigSelect");
			this.paginateQuests();
			base.width = 832;
			base.height = 576;

			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				base.height += 64;
			}
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
			base.xPositionOnScreen = (int)topLeft.X;
			base.yPositionOnScreen = (int)topLeft.Y + 32;

			// Create the buttons used for teleporting and renaming
			this.teleportDestinationButtons = new List<ClickableComponent>();
			this.renameObeliskButtons = new List<ClickableTextureComponent>();

			for (int i = 0; i < 6; i++)
			{
				ClickableComponent teleportButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 32) / 6), base.width - 32, (base.height - 32) / 6 + 4), string.Concat(i))
				{
					myID = i,
					downNeighborID = -7777,
					upNeighborID = ((i > 0) ? (i - 1) : (-1)),
					rightNeighborID = -7777,
					leftNeighborID = -7777,
					fullyImmutable = true
				};
				this.teleportDestinationButtons.Add(teleportButton);

				this.renameObeliskButtons.Add(new ClickableTextureComponent(new Rectangle(teleportButton.bounds.Right - teleportButton.bounds.Width / 8, teleportButton.bounds.Y + teleportButton.bounds.Height / 4, 32, 32), Game1.mouseCursors, new Rectangle(66, 4, 14, 12), 4f)
				{
					myID = i + 103,
					downNeighborID = -7777,
					upNeighborID = ((i > 0) ? (i + 103 - 1) : (-1)),
					rightNeighborID = -7777,
					leftNeighborID = i
				});
			}
			base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 20, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
			this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 102,
				rightNeighborID = -7777
			};
			this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 64 - 48, base.yPositionOnScreen + base.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 101
			};

			int scrollbar_x = base.xPositionOnScreen + base.width + 16;
			this.upArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, base.yPositionOnScreen + 96, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			this.downArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
			this.scrollBarBounds = default(Rectangle);
			this.scrollBarBounds.X = this.upArrow.bounds.X + 12;
			this.scrollBarBounds.Width = 24;
			this.scrollBarBounds.Y = this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4;
			this.scrollBarBounds.Height = this.downArrow.bounds.Y - 4 - this.scrollBarBounds.Y;
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.scrollBarBounds.X, this.scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			if (Game1.options.SnappyMenus)
			{
				base.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID >= 0 && oldID < 6 && this.questPage == -1)
			{
				switch (direction)
				{
					case 2:
						if (oldID < 5 && this.pages[this.currentPage].Count - 1 > oldID)
						{
							base.currentlySnappedComponent = base.getComponentWithID(oldID + 1);
						}
						break;
					case 1:
						if (this.currentPage < this.pages.Count - 1)
						{
							base.currentlySnappedComponent = base.getComponentWithID(101);
							base.currentlySnappedComponent.leftNeighborID = oldID;
						}
						break;
					case 3:
						if (this.currentPage > 0)
						{
							base.currentlySnappedComponent = base.getComponentWithID(102);
							base.currentlySnappedComponent.rightNeighborID = oldID;
						}
						break;
				}
			}
			else if (oldID == 102)
			{
				if (this.questPage != -1)
				{
					return;
				}
				base.currentlySnappedComponent = base.getComponentWithID(0);
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		public override void snapToDefaultClickableComponent()
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.RightTrigger && this.questPage == -1 && this.currentPage < this.pages.Count - 1)
			{
				this.nonQuestPageForwardButton();
			}
			else if (b == Buttons.LeftTrigger && this.questPage == -1 && this.currentPage > 0)
			{
				this.nonQuestPageBackButton();
			}
		}

		private void paginateQuests()
		{
			this.pages = new List<List<StardewValley.Object>>();
			for (int i = miniObelisks.Count - 1; i >= 0; i--)
			{
				if (miniObelisks[i] == null)
				{
					miniObelisks.RemoveAt(i);
				}
				else
				{
					int which2 = miniObelisks.Count - 1 - i;
					while (this.pages.Count <= which2 / 6)
					{
						this.pages.Add(new List<StardewValley.Object>());
					}
					this.pages[which2 / 6].Add(miniObelisks[i]);
				}
			}
			if (this.pages.Count == 0)
			{
				this.pages.Add(new List<StardewValley.Object>());
			}
			this.currentPage = Math.Min(Math.Max(this.currentPage, 0), this.pages.Count - 1);
			this.questPage = -1;
		}

		public bool NeedsScroll()
		{
			if (this._shownQuest != null && this._shownQuest.ShouldDisplayAsComplete())
			{
				return false;
			}
			if (this.questPage != -1)
			{
				return this._contentHeight > this._scissorRectHeight;
			}
			return false;
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (this.NeedsScroll())
			{
				float new_scroll = this.scrollAmount - (float)(Math.Sign(direction) * 64 / 2);
				if (new_scroll < 0f)
				{
					new_scroll = 0f;
				}
				if (new_scroll > this._contentHeight - this._scissorRectHeight)
				{
					new_scroll = this._contentHeight - this._scissorRectHeight;
				}
				if (this.scrollAmount != new_scroll)
				{
					this.scrollAmount = new_scroll;
					Game1.playSound("shiny4");
					this.SetScrollBarFromAmount();
				}
			}
			base.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			base.performHoverAction(x, y);
			if (this.questPage == -1)
			{
				for (int i = 0; i < this.teleportDestinationButtons.Count; i++)
				{
					if (this.pages.Count > 0 && this.pages[0].Count > i && this.renameObeliskButtons[i].containsPoint(x, y))
                    {
						this.hoverText = "Rename Obelisk";
					}
				}
			}

			this.forwardButton.tryHover(x, y, 0.2f);
			this.backButton.tryHover(x, y, 0.2f);

			if (this.NeedsScroll())
			{
				this.upArrow.tryHover(x, y);
				this.downArrow.tryHover(x, y);
				this.scrollBar.tryHover(x, y);
				_ = this.scrolling;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.doesInputListContain(Game1.options.journalButton, key) && this.readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
		}

		private void nonQuestPageForwardButton()
		{
			this.currentPage++;
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus && this.currentPage == this.pages.Count - 1)
			{
				base.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		private void nonQuestPageBackButton()
		{
			this.currentPage--;
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus && this.currentPage == 0)
			{
				base.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!GameMenu.forcePreventClose)
			{
				base.leftClickHeld(x, y);
				if (this.scrolling)
				{
					this.SetScrollFromY(y);
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!GameMenu.forcePreventClose)
			{
				base.releaseLeftClick(x, y);
				this.scrolling = false;
			}
		}

		public virtual void SetScrollFromY(int y)
		{
			int y2 = this.scrollBar.bounds.Y;
			float percentage = (float)(y - this.scrollBarBounds.Y) / (float)(this.scrollBarBounds.Height - this.scrollBar.bounds.Height);
			percentage = Utility.Clamp(percentage, 0f, 1f);
			this.scrollAmount = percentage * (this._contentHeight - this._scissorRectHeight);
			this.SetScrollBarFromAmount();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}

		public void UpArrowPressed()
		{
			this.upArrow.scale = this.upArrow.baseScale;
			this.scrollAmount -= 64f;
			if (this.scrollAmount < 0f)
			{
				this.scrollAmount = 0f;
			}
			this.SetScrollBarFromAmount();
		}

		public void DownArrowPressed()
		{
			this.downArrow.scale = this.downArrow.baseScale;
			this.scrollAmount += 64f;
			if (this.scrollAmount > this._contentHeight - this._scissorRectHeight)
			{
				this.scrollAmount = this._contentHeight - this._scissorRectHeight;
			}
			this.SetScrollBarFromAmount();
		}

		private void SetScrollBarFromAmount()
		{
			if (!this.NeedsScroll())
			{
				this.scrollAmount = 0f;
				return;
			}
			if (this.scrollAmount < 8f)
			{
				this.scrollAmount = 0f;
			}
			if (this.scrollAmount > this._contentHeight - this._scissorRectHeight - 8f)
			{
				this.scrollAmount = this._contentHeight - this._scissorRectHeight;
			}
			this.scrollBar.bounds.Y = (int)((float)this.scrollBarBounds.Y + (float)(this.scrollBarBounds.Height - this.scrollBar.bounds.Height) / Math.Max(1f, this._contentHeight - this._scissorRectHeight) * this.scrollAmount);
		}

		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			if (this.NeedsScroll())
			{
				switch (direction)
				{
					case 0:
						this.UpArrowPressed();
						break;
					case 2:
						this.DownArrowPressed();
						break;
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (Game1.activeClickableMenu == null)
			{
				return;
			}

			for (int i = 0; i < this.teleportDestinationButtons.Count; i++)
			{
				if (this.pages.Count > 0 && this.pages[this.currentPage].Count > i && this.teleportDestinationButtons[i].containsPoint(x, y))
				{
					base.exitThisMenu();

					AttemptTeleport(Game1.player, this.pages[this.currentPage][i]);
					return;
				}
			}
			if (this.currentPage < this.pages.Count - 1 && this.forwardButton.containsPoint(x, y))
			{
				this.nonQuestPageForwardButton();
				return;
			}
			if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
			{
				this.nonQuestPageBackButton();
				return;
			}
		}

		private bool AttemptTeleport(Farmer who, StardewValley.Object obelisk)
        {
			Vector2 target = obelisk.TileLocation;
			foreach (Vector2 v in new List<Vector2>
			{
				new Vector2(target.X, target.Y + 1f),
				new Vector2(target.X - 1f, target.Y),
				new Vector2(target.X + 1f, target.Y),
				new Vector2(target.X, target.Y - 1f)
			})
			{
				if (who.currentLocation.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v))
				{
					for (int i = 0; i < 12; i++)
					{
						who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
					}
					who.currentLocation.playSound("wand");
					Game1.displayFarmer = false;
					Game1.player.freezePause = 800;
					Game1.flashAlpha = 1f;
					DelayedAction.fadeAfterDelay(delegate
					{
						who.setTileLocation(v);
						Game1.displayFarmer = true;
						Game1.globalFadeToClear();
					}, 800);
					new Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
					int j = 0;
					for (int x = who.getTileX() + 8; x >= who.getTileX() - 8; x--)
					{
						who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = j * 25,
							motion = new Vector2(-0.25f, 0f)
						});
						j++;
					}

					return true;
				}
			}

			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsSpace"));
			return false;
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			SpriteText.drawStringWithScrollCenteredAt(b, "Choose a Destination", base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);
			for (int j = 0; j < this.teleportDestinationButtons.Count; j++)
			{
				if (this.pages.Count() > 0 && this.pages[this.currentPage].Count() > j)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.teleportDestinationButtons[j].bounds.X, this.teleportDestinationButtons[j].bounds.Y, this.teleportDestinationButtons[j].bounds.Width, this.teleportDestinationButtons[j].bounds.Height, this.teleportDestinationButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
					Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2(this.teleportDestinationButtons[j].bounds.X + 32, this.teleportDestinationButtons[j].bounds.Y + 28), new Rectangle(0, 512, 16, 16), Color.White, 0f, Vector2.Zero, 2f, flipped: false, 0.99f, shadowIntensity: 0f);
					SpriteText.drawString(b, this.pages[this.currentPage][j].DisplayName, this.teleportDestinationButtons[j].bounds.X + 128 + 4, this.teleportDestinationButtons[j].bounds.Y + 20);

					// Draw the rename button
					this.renameObeliskButtons[j].draw(b);
				}
			}

			if (this.NeedsScroll())
			{
				this.upArrow.draw(b);
				this.downArrow.draw(b);
				this.scrollBar.draw(b);
			}
			if (this.currentPage < this.pages.Count - 1 && this.questPage == -1)
			{
				this.forwardButton.draw(b);
			}
			if (this.currentPage > 0 || this.questPage != -1)
			{
				this.backButton.draw(b);
			}

			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b);

			if (this.hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont);
			}
		}
	}
}