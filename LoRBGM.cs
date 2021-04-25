using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using System;
using MonoMod.Cil;
using System.Threading;
using Terraria.ModLoader.Audio;
using Microsoft.Xna.Framework.Audio;
using Terraria.Localization;

namespace LoRBGM
{
	public class LoRBGM : Mod
	{
		public static LoRConfig LoRConfig;
		public bool BossActive = false;
		public int CurrentID = -1;
		public int Timer = 0;
		public int LifeMax = 0;


		private bool ILpatched = false;
		private int customTitleMusicSlot = 6;

		public override void Unload()
        {
			IL.Terraria.Main.UpdateAudio -= new ILContext.Manipulator(TitleMusicIL);
			customTitleMusicSlot = 6;
			Close();
			LoRConfig = null;
        }
        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
			if (LoRConfig == null) return;
			if (Main.gameMenu && LoRConfig.LobbyBGM)
            {
				customTitleMusicSlot = GetSoundSlot(SoundType.Music, "Sounds/Music/Lobby");
			}
            else
            {
				if (Main.gameMenu)
				{
					music = 6;
				}
				customTitleMusicSlot = 6;
				Music music2 = GetMusic("Sounds/Music/Lobby");
				if (music2.IsPlaying)
				{
					music2.Stop(AudioStopOptions.AsAuthored);
				}

			}

			if (Main.gameMenu || Main.myPlayer == -1)
			{
				Timer = 0;
				CurrentID = -1;
				BossActive = false;
				return;
            }
            if (!IsAnyBGMInConfig())
            {
				Timer = 0;
				CurrentID = -1;
				BossActive = false;
				return;
            }
			if (BossActive && !IsBosses())
			{
				if (Main.LocalPlayer.dead || Main.LocalPlayer.ghost || !Main.LocalPlayer.active)
				{
					Timer = 0;
				}
				CurrentID = -1;
				BossActive = false;
			}
			if (!BossActive && IsBosses())
			{
				BossActive = true;
				CurrentID = LoRID.GetRandomName();
				if (LoRConfig.BGMClaim)
				{
					string str = Language.ActiveCulture == GameCulture.Chinese ? "目前播放：" : "Currently playing: ";
					Main.NewText(str + LoRID.GetFloorName(CurrentID), LoRID.GetFloorColor(CurrentID));
				}
				Timer = 600;
			}
            if (IsBosses())
            {
                if (BossHealthMax() > LifeMax)
                {
					LifeMax = BossHealthMax();
                }
            }
            else
            {
				LifeMax = 0;
            }
            if (CurrentID == -1)
            { 
				if (Timer > 0)           //胜利
				{
					if (LoRConfig.ResultBGM)
					{
						music = GetSoundSlot(SoundType.Music, "Sounds/Music/Victory");
						priority = MusicPriority.Environment;
					}
					Timer--;
				}
			}
            else
            {                           //战斗中
				int s = 1;

				if ((LoRID.Asiyah.Contains(CurrentID) ||
					LoRID.Briah.Contains(CurrentID) ||
					LoRID.Atziluth.Contains(CurrentID) ||
					LoRID.BossBGM.Contains(CurrentID) ||
					LoRID.RealizeBGM.Contains(CurrentID) ||
					LoRID.NC2BGM.Contains(CurrentID)) && CurrentID != LoRID.Kether)
                {
					float k = (float)BossHealth() / LifeMax;
					if (k < 0.333)
					{
						s = 3;
					}
					else if (k >= 0.333 && k < 0.666)
                    {
						s = 2;
                    }
                    else if (k >= 0.666)
                    {
						s = 1;
                    }
					
					if (NPC.AnyNPCs(NPCID.MoonLordCore))
					{
						if (NPC.CountNPCS(NPCID.MoonLordFreeEye) >= NPC.CountNPCS(NPCID.MoonLordCore) * 2)
						{
							s = 2;
						}
						if (Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)].life < Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)].lifeMax)
						{
							s = 3;
						}
					}
				}
				else if (CurrentID == LoRID.Kether)
                {
					s = ((float)BossHealth() / LifeMax > 0.5f) ? 1 : 2;
					if (NPC.AnyNPCs(NPCID.MoonLordCore))
					{
						if (Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)].life < Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)].lifeMax)
						{
							s = 2;
						}
					}
				}
				else if (LoRID.NCBGM.Contains(CurrentID))
				{
					float k = (float)BossHealth() / LifeMax;
					if (k < 0.25)
					{
						s = 4;
					}
					else if (k < 0.5)
					{
						s = 3;
					}
					else if (k < 0.75)
					{
						s = 2;
					}
                    else
                    {
						s = 1;
                    }
					if (NPC.AnyNPCs(NPCID.MoonLordCore))
					{
                        if (NPC.AnyNPCs(NPCID.MoonLordFreeEye))
                        {
							s = 2;
                        }
						if (NPC.CountNPCS(NPCID.MoonLordFreeEye) >= NPC.CountNPCS(NPCID.MoonLordCore) * 2)
						{
							s = 3;
						}
						if (Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)].life < Main.npc[NPC.FindFirstNPC(NPCID.MoonLordCore)].lifeMax)
						{
							s = 4;
						}
					}
				}
				else
                {
					s = 1;
                }

				music = GetSoundSlot(SoundType.Music, LoRID.IDToPath(CurrentID, s));
				priority = MusicPriority.BossHigh;
			}
        }


        public bool IsBosses()
		{
			foreach (NPC npc in Main.npc)
			{
				if (npc.active)
				{
					if (npc.boss || (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsAnyBGMInConfig()
        {
			return LoRConfig.BGMAsiyah || 
				LoRConfig.BGMAtziluth || 
				LoRConfig.BGMBriah || 
				LoRConfig.BossBGM || 
				LoRConfig.RealizeBGM || 
				LoRConfig.Reverberation2BGM || 
				LoRConfig.ReverberationBGM;

        }

		public int BossHealth()
        {
			int Life = 0;
			foreach (NPC npc in Main.npc)
			{
				if (npc.active)
				{
					if ((npc.boss && npc.realLife < 0) || (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
						|| npc.type == NPCID.Creeper || npc.type == NPCID.SkeletronHand || npc.type == NPCID.PrimeCannon || npc.type == NPCID.PrimeLaser ||
						npc.type == NPCID.PrimeSaw || npc.type == NPCID.PrimeVice || npc.type == NPCID.GolemHead)
					{
						Life += npc.life;
					}
				}
			}
			return Life;
		}


		public int BossHealthMax()
		{
			int LifeMax = 0;
			foreach (NPC npc in Main.npc)
			{
				if (npc.active)
				{
					if ((npc.boss && npc.realLife < 0) || (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
						|| npc.type == NPCID.Creeper || npc.type == NPCID.SkeletronHand || npc.type == NPCID.PrimeCannon || npc.type == NPCID.PrimeLaser ||
						npc.type == NPCID.PrimeSaw || npc.type == NPCID.PrimeVice || npc.type == NPCID.GolemHead)
					{
						LifeMax += npc.lifeMax;
					}
				}
			}
			return LifeMax;
		}



		private void TitleMusicIL(ILContext il)
		{
			ILCursor ilcursor = new ILCursor(il);
			ILCursor ilcursor2 = ilcursor;
			MoveType moveType = MoveType.After;
			Func<Instruction, bool>[] array = new Func<Instruction, bool>[1];
			array[0] = ((Instruction i) => ILPatternMatchingExt.MatchLdfld<Main>(i, "newMusic"));
			ilcursor2.GotoNext(moveType, array);
			ilcursor.EmitDelegate<Func<int, int>>(delegate (int newMusic)
			{
				if (newMusic != 6)
				{
					return newMusic;
				}
				return customTitleMusicSlot;
			});
		}

		private void SetTitleMusic()
		{
			if (LoRConfig == null) return;
			if (LoRConfig.LobbyBGM)
			{
				if (!ILpatched)
				{
					customTitleMusicSlot = GetSoundSlot(SoundType.Music, "Sounds/Music/Lobby");
					IL.Terraria.Main.UpdateAudio += new ILContext.Manipulator(TitleMusicIL);
					ILpatched = true;
				}
			}
			else
			{
				customTitleMusicSlot = 6;
			}
		}

		public override void Close()
		{
			int soundSlot = GetSoundSlot(SoundType.Music, "Sounds/Music/Lobby");
			if (Main.music.IndexInRange(soundSlot))
			{
				Music music = Main.music[soundSlot];
				if (music != null && music.IsPlaying)
				{
					Main.music[soundSlot].Stop(AudioStopOptions.AsAuthored);
				}
			}
			base.Close();
		}

        public override void PreSaveAndQuit()
        {
			SetTitleMusic();
		}

        public override void PostSetupContent()
        {
			SetTitleMusic();
		}


	}


	public static class LoRID
	{
		public const int Boss = 0;
		public const int Kether = 1;
		public const int Malkuth = 2;
		public const int Yesod = 3;
		public const int Hod = 4;
		public const int Netzach = 5;
		public const int Tiphereth = 6;
		public const int Chesed = 7;
		public const int Gebura = 8;
		public const int Binah = 9;
		public const int Hokma = 10;
		public const int Angela = 11;
		public const int Roland = 12;
		public const int Reverberation = 13;
		public const int Reverberation2 = 14;
		public static readonly List<int> Atziluth = new List<int> { Kether, Binah, Hokma };
		public static readonly List<int> Briah = new List<int> { Tiphereth, Gebura, Chesed };
		public static readonly List<int> Asiyah = new List<int> { Malkuth, Yesod, Hod, Netzach };
		public static readonly List<int> RealizeBGM = new List<int> { Roland, Angela };
		public static readonly List<int> NCBGM = new List<int> { Reverberation };
		public static readonly List<int> NC2BGM = new List<int> { Reverberation2 };
		public static readonly List<int> BossBGM = new List<int> { Boss };
		public static int GetRandomName()
        {
			if(!LoRBGM.LoRConfig.BGMAtziluth && 
				!LoRBGM.LoRConfig.BGMBriah && 
				!LoRBGM.LoRConfig.BGMAsiyah && 
				!LoRBGM.LoRConfig.RealizeBGM && 
				!LoRBGM.LoRConfig.ReverberationBGM && 
				!LoRBGM.LoRConfig.BossBGM &&
				!LoRBGM.LoRConfig.Reverberation2BGM)
            {
				LoRBGM.LoRConfig.BossBGM = true;
            }
			List<int> Ranlist = new List<int>();
            if (LoRBGM.LoRConfig.BGMAsiyah)
            {
				Ranlist.AddRange(Asiyah);
            }
            if (LoRBGM.LoRConfig.BGMBriah)
            {
				Ranlist.AddRange(Briah);
            }
            if (LoRBGM.LoRConfig.BGMAtziluth)
            {
				Ranlist.AddRange(Atziluth);
            }
			if (LoRBGM.LoRConfig.RealizeBGM)
			{
				Ranlist.AddRange(RealizeBGM);
			}
            if (LoRBGM.LoRConfig.ReverberationBGM)
            {
				Ranlist.AddRange(NCBGM);
            }
			if (LoRBGM.LoRConfig.BossBGM)
			{
				Ranlist.AddRange(BossBGM);
			}
			if (LoRBGM.LoRConfig.Reverberation2BGM)
			{
				Ranlist.AddRange(NC2BGM);
			}

			return Ranlist[Main.rand.Next(Ranlist.Count)];
        }
		public static string IDToPath(int id,int phase) 
		{
			string a = "Sounds/Music/";
			if (id == Boss)
			{
				a += "Boss/";
			}
			else if (id == Reverberation2)
            {
				a += "Reverberation2/";
			}
            else
            {
				a += GetFloorName(id) + "/";
			}
			a += phase.ToString();
			return a;
		}

		public static string GetFloorName(int id)
		{
			switch (id)
			{
				case Boss:
					return "Early Battle";
				case Kether:
					return "Kether";
				case Malkuth:
					return "Malkuth";
				case Yesod:
					return "Yesod";
				case Hod:
					return "Hod";
				case Netzach:
					return "Netzach";
				case Tiphereth:
					return "Tiphereth";
				case Chesed:
					return "Chesed";
				case Gebura:
					return "Gebura";
				case Binah:
					return "Binah";
				case Hokma:
					return "Hokma";
				case Angela:
					return "Angela";
				case Roland:
					return "Roland";
				case Reverberation:
					return "Reverberation";
				case Reverberation2:
					return "Reverberation Distortion";
				default:
					break;
			}
			return "Kether";
		}

		public static Color GetFloorColor(int id)
        {
			switch (id)
			{
				case Boss:
					return Color.Silver;
				case Kether:
					return Color.White;
				case Malkuth:
					return Color.Orange;
				case Yesod:
					return Color.MediumPurple;
				case Hod:
					return Color.Yellow;
				case Netzach:
					return Color.Green;
				case Tiphereth:
					return Color.Gold;
				case Chesed:
					return Color.Blue;
				case Gebura:
					return Color.Red;
				case Binah:
					return Color.DarkGoldenrod;
				case Hokma:
					return Color.LightGray;
				case Angela:
					return Color.Cyan;
				case Roland:
					return Color.Gray;
				case Reverberation:
					return Color.LightBlue;
				case Reverberation2:
					return Color.LightBlue;
				default:
					break;
			}
			return Color.White;
		}

		public static string RainbowStr(string str)
        {
			string[] words = new string[str.Length];
			for(int i = 0; i < str.Length; i++)
            {
				words[i] = str.Substring(i, 1);
            }
			Color[] color = new Color[]
			{
					Color.Orange,
					Color.MediumPurple,
					Color.Yellow,
					Color.Green,
					Color.Gold,
					Color.Red,
					Color.Blue,
					Color.DarkGoldenrod,
					Color.LightGray,
					Color.Silver
			};
			for (int i = 0; i < words.Length; i++)
			{
				if (words[i] != " ")
				{
					int index = i % color.Length;
					words[i] = "[c/" + Convert.ToString(color[index].R, 16) + Convert.ToString(color[index].G, 16) + Convert.ToString(color[index].B, 16) + ":" + words[i] + "]";
				}
			}
			return string.Join("", words);
        }
	}
}